using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mafia : MonoBehaviour
{
    [SerializeField] private bool isMafia;
    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float maxViewDistance = 5f;
    [SerializeField] private float waypointReachedThreshold = 0.1f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float movementSmoothing = 0.05f;
    [SerializeField] private float obstacleAvoidanceDistance = 1f;
    [SerializeField] private float collisionCheckDistance = 0.5f;

    [Header("Combat Settings")]
    [SerializeField] private int health = 2;
    [SerializeField] private float shootingDistance = 3f;
    [SerializeField] private float shootingCooldown = 1f;
    [SerializeField] private float stoppingDistance = 2.5f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletLifetime = 2f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackAngle = 120f;

    [Header("Sound Detection")]
    [SerializeField] private float soundDetectionRadius = 10f;

    [Header("Player Position Tracking")]
    [SerializeField] private int maxStoredPositions = 5;
    [SerializeField] private float positionRecordInterval = 0.5f;

    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float wanderInterval = 2f;
    [SerializeField] private float stuckTimeThreshold = 1f;

    [Header("Look Around Settings")]
    [SerializeField] private float forwardMoveDistance = 1f;
    [SerializeField] private float forwardMoveSpeed = 2f;
    [SerializeField] private float rotationDuration = 3f;

    [Header("Visual Effects")]
    [SerializeField] private float flashDuration = 0.2f;

    [Header("References")]
    private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private List<Transform> patrolWaypoints = new List<Transform>();
    [SerializeField] private LayerMask playerLayer;
    private SpriteRenderer spriteRenderer;

    private bool isSpotted = false;
    private Color originalColor;
    private bool isDead = false;
    private bool isChasing = false;
    private bool isInCombat = false;
    private bool isInvestigatingSound = false;
    private bool isFollowingLastPositions = false;
    private bool isWandering = false;
    private bool isLookingAround = false;
    private int currentWaypointIndex = 0;
    private Vector2 currentTarget;
    private Vector2 lookDirection = Vector2.right;
    private float lastShotTime;
    private Coroutine shootingCoroutine;
    private Vector2 m_Velocity = Vector2.zero;
    private Queue<Vector2> playerLastPositions = new Queue<Vector2>();
    private float lastPositionRecordTime;
    private Vector2 lastPosition;
    private float stuckTimer = 0f;
    private bool isAvoidingObstacle = false;
    private Vector2 avoidanceTarget;
    private float lastWanderTime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        if (patrolWaypoints.Count == 0)
        {
            Debug.LogError("No patrol waypoints assigned!");
            return;
        }
        currentTarget = patrolWaypoints[currentWaypointIndex].position;
        rb.drag = 5f;
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (!isChasing && !isInvestigatingSound)
        {
            DetectNearbyBullets();
        }

        if (CanSeePlayer())
        {
            HandlePlayerDetection();
            if (isChasing && Time.time - lastPositionRecordTime >= positionRecordInterval)
            {
                RecordPlayerPosition();
            }
        }
        else if (isChasing)
        {
            FollowLastKnownPositionsOrWander();
        }

        if (!isLookingAround)
        {
            MoveToTarget();
        }
        UpdateAnimation();
        UpdateRotation();

        if (!isChasing && !isFollowingLastPositions && !isWandering && !isLookingAround && Vector2.Distance(transform.position, currentTarget) < waypointReachedThreshold)
        {
            GetNextWaypoint();
        }

        if (health <= 0)
        {
            Death();
        }

        // Проверка застревания
        if (!isLookingAround && Vector2.Distance(transform.position, lastPosition) < 0.1f && rb.velocity.magnitude > 0.1f)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckTimeThreshold)
            {
                HandleObstacleAvoidance();
            }
        }
        else
        {
            stuckTimer = 0f;
            isAvoidingObstacle = false;
        }
        lastPosition = transform.position;
    }

    private void DetectNearbyBullets()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, soundDetectionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Bullet"))
            {
                StartCoroutine(InvestigateSound());
                break;
            }
        }
    }

    public IEnumerator InvestigateSound()
    {
        isInvestigatingSound = true;
        rb.velocity = Vector2.zero;

        Vector2 direction = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        lookDirection = direction;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        float timer = 0f;
        while (timer < 3f)
        {
            if (CanSeePlayer())
            {
                isInvestigatingSound = false;
                HandlePlayerDetection();
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (!CanSeePlayer())
        {
            isInvestigatingSound = false;
            Vector2 closestWaypoint = GetClosestWaypoint();
            if (CanMoveDirectlyTo(closestWaypoint))
            {
                currentTarget = closestWaypoint;
                Debug.Log($"{gameObject.name} returning to patrol waypoint: {currentTarget}");
            }
            else
            {
                Debug.Log($"{gameObject.name} cannot reach patrol waypoint {closestWaypoint}, starting to wander");
                StartWandering();
            }
        }
    }

    private void HandlePlayerDetection()
    {
        if (!isChasing)
        {
            StartChasing();
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= shootingDistance)
        {
            EngageCombat();
        }
        else if (distanceToPlayer > stoppingDistance)
        {
            ContinueChasing();
        }
    }

    private void StartChasing()
    {
        isChasing = true;
        isFollowingLastPositions = false;
        isWandering = false;
        isLookingAround = false;
        if (isMafia)
        {
            if (!isSpotted)
            {
                SoundManager.Instance.PlayEnemies(EnemiesSoundType.SpottedMaf);
                isSpotted = true;
            }
            animator.Play("Taking");
            TakeGun();
        }
        else
        {
            if (!isSpotted)
            {
                SoundManager.Instance.PlayEnemies(EnemiesSoundType.Spotted);
                isSpotted = true;
            }
            Debug.Log("Taking!");
            animator.Play("TakingDubinka");
        }
    }

    private void EngageCombat()
    {
        isInCombat = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (shootingCoroutine == null)
        {
            shootingCoroutine = StartCoroutine(ShootingRoutine());
        }
    }

    private void ContinueChasing()
    {
        isInCombat = false;
        currentTarget = player.position;

        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }

    private void StartWandering()
    {
        isChasing = false;
        isInCombat = false;
        isFollowingLastPositions = false;
        isWandering = true;
        isLookingAround = false;
        playerLastPositions.Clear();
        if (isMafia)
        {
            animator.SetBool("Gun", false);
        }
        else
        {
            animator.SetBool("Gun", false);
            animator.SetBool("GunTaking", false);
        }
        Debug.Log($"{gameObject.name} started wandering at position: {transform.position}");
        ChooseNewWanderPoint();

        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }

    private IEnumerator ShootingRoutine()
    {
        while (isInCombat && CanSeePlayer())
        {
            Shoot();
            yield return new WaitForSeconds(shootingCooldown);
        }
        shootingCoroutine = null;
    }

    private void Shoot()
    {
        if (!isDead)
        {
            if (Time.time - lastShotTime < shootingCooldown || player == null) return;

            lastShotTime = Time.time;

            if (isMafia)
            {
                Vector2 shootDirection = ((Vector2)player.position - (Vector2)firePoint.position).normalized;
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
                SoundManager.Instance.PlayEnemies(EnemiesSoundType.Shot);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                bulletRb.velocity = shootDirection * bulletSpeed;

                float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
                bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                Destroy(bullet, bulletLifetime);
            }
            else
            {
                Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

                foreach (Collider2D enemy in hitPlayer)
                {
                    if (enemy.gameObject.CompareTag("Player"))
                    {
                        Vector2 directionToEnemy = (enemy.transform.position - attackPoint.position).normalized;
                        float angleToEnemy = Vector2.Angle(attackPoint.right, directionToEnemy);
                        animator.SetBool("Gun", true);
                        animator.SetBool("GunTaking", false);
                        if (angleToEnemy <= attackAngle / 2f)
                        {
                            animator.PlayInFixedTime("Attack");
                        }
                    }
                }

                Debug.DrawRay(attackPoint.position, attackPoint.right * attackRange, Color.red, 0.5f);
            }
        }
    }

    public void PoliceHit()
    {
        Vector2 directionToEnemy = (player.transform.position - attackPoint.position).normalized;
        float angleToEnemy = Vector2.Angle(attackPoint.right, directionToEnemy);
        RaycastHit2D hit = Physics2D.Raycast(attackPoint.position, directionToEnemy, Vector2.Distance(player.transform.position, attackPoint.position), obstacleLayer);
        if (angleToEnemy <= attackAngle / 2f && hit.collider == null)
        {
            SoundManager.Instance.PlayEnemies(EnemiesSoundType.Hitted);
            player.GetComponent<PlayerHealth>().TakeDamage(1);
            Debug.Log($"Удар дубинкой!");
        }
    }

    private void RecordPlayerPosition()
    {
        playerLastPositions.Enqueue(player.position);
        if (playerLastPositions.Count > maxStoredPositions)
        {
            playerLastPositions.Dequeue();
        }
        lastPositionRecordTime = Time.time;
    }

    private void FollowLastKnownPositionsOrWander()
    {
        if (!isFollowingLastPositions)
        {
            isFollowingLastPositions = true;
            isChasing = false;
            if (playerLastPositions.Count > 0)
            {
                currentTarget = playerLastPositions.Peek();
                Debug.Log($"{gameObject.name} following last player position: {currentTarget}");
            }
            else
            {
                Debug.Log($"{gameObject.name} no player positions, starting to wander");
                StartCoroutine(MoveForwardAndRotate());
            }
        }
    }

    private IEnumerator MoveForwardAndRotate()
    {
        isLookingAround = true;
        isFollowingLastPositions = false;
        rb.velocity = Vector2.zero;

        // Движение вперёд
        Vector2 forwardTarget = (Vector2)transform.position + lookDirection * forwardMoveDistance;
        Debug.Log($"{gameObject.name} moving forward to: {forwardTarget}");
        float moveTime = 0f;
        Vector2 startPosition = transform.position;

        while (moveTime < forwardMoveDistance / forwardMoveSpeed)
        {
            moveTime += Time.deltaTime;
            float t = moveTime / (forwardMoveDistance / forwardMoveSpeed);
            Vector2 newPosition = Vector2.Lerp(startPosition, forwardTarget, t);
            rb.MovePosition(newPosition);
            UpdateAnimation();
            yield return null;
        }

        rb.MovePosition(forwardTarget);
        rb.velocity = Vector2.zero;

        // Вращение на 360 градусов с проверкой игрока
        Debug.Log($"{gameObject.name} starting 360-degree rotation");
        float startAngle = transform.eulerAngles.z;
        float targetAngle = startAngle + 360f;
        float rotationTime = 0f;

        while (rotationTime < rotationDuration)
        {
            if (CanSeePlayer())
            {
                Debug.Log($"{gameObject.name} spotted player during rotation, interrupting to chase");
                isLookingAround = false;
                HandlePlayerDetection();
                yield break;
            }

            rotationTime += Time.deltaTime;
            float t = rotationTime / rotationDuration;
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, t);
            transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            UpdateAnimation();
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0, 0, targetAngle % 360f);
        Debug.Log($"{gameObject.name} completed 360-degree rotation");

        // Переход к блужданию
        StartWandering();
    }

    private void ChooseNewWanderPoint()
    {
        if (Time.time - lastWanderTime < wanderInterval && !isAvoidingObstacle) return;

        Vector2 wanderPoint;
        int attempts = 0;
        const int maxAttempts = 5;

        do
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            wanderPoint = (Vector2)transform.position + randomDirection * wanderRadius;
            attempts++;
        } while (!CanMoveDirectlyTo(wanderPoint) && attempts < maxAttempts);

        if (CanMoveDirectlyTo(wanderPoint))
        {
            currentTarget = wanderPoint;
        }
        else
        {
            Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
            foreach (Vector2 dir in directions)
            {
                wanderPoint = (Vector2)transform.position + dir * wanderRadius;
                if (CanMoveDirectlyTo(wanderPoint))
                {
                    currentTarget = wanderPoint;
                    break;
                }
            }
        }
        Debug.Log($"{gameObject.name} chose new wander point: {currentTarget}");
        lastWanderTime = Time.time;
    }

    private bool CanMoveDirectlyTo(Vector2 target)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, target);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);
        return hit.collider == null;
    }

    private void HandleObstacleAvoidance()
    {
        if (isAvoidingObstacle || isLookingAround) return;

        isAvoidingObstacle = true;
        stuckTimer = 0f;

        Vector2 directionToTarget = (currentTarget - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, collisionCheckDistance, obstacleLayer);

        if (hit.collider != null)
        {
            Vector2[] avoidanceDirections = {
                new Vector2(-directionToTarget.y, directionToTarget.x),
                new Vector2(directionToTarget.y, -directionToTarget.x)
            };

            foreach (Vector2 avoidanceDir in avoidanceDirections)
            {
                RaycastHit2D checkHit = Physics2D.Raycast(transform.position, avoidanceDir, collisionCheckDistance, obstacleLayer);
                if (checkHit.collider == null)
                {
                    avoidanceTarget = (Vector2)transform.position + avoidanceDir * obstacleAvoidanceDistance;
                    currentTarget = avoidanceTarget;
                    return;
                }
            }

            Vector2 oppositeDir = -directionToTarget;
            RaycastHit2D oppositeHit = Physics2D.Raycast(transform.position, oppositeDir, collisionCheckDistance, obstacleLayer);
            if (oppositeHit.collider == null)
            {
                avoidanceTarget = (Vector2)transform.position + oppositeDir * obstacleAvoidanceDistance;
                currentTarget = avoidanceTarget;
                return;
            }

            // Если все направления заблокированы
            if (isFollowingLastPositions && playerLastPositions.Count > 0)
            {
                Debug.Log($"{gameObject.name} cannot reach last player position {currentTarget}, removing it");
                playerLastPositions.Dequeue();
                if (playerLastPositions.Count > 0)
                {
                    currentTarget = playerLastPositions.Peek();
                    Debug.Log($"{gameObject.name} moving to next player position: {currentTarget}");
                }
                else
                {
                    Debug.Log($"{gameObject.name} no more player positions, starting look around");
                    StartCoroutine(MoveForwardAndRotate());
                }
                isAvoidingObstacle = false;
                return;
            }
            else if (!isChasing && !isFollowingLastPositions && !isWandering)
            {
                Debug.Log($"{gameObject.name} cannot reach patrol waypoint {currentTarget}, skipping to next");
                GetNextWaypoint();
                isAvoidingObstacle = false;
                return;
            }
        }

        isAvoidingObstacle = false;

        if (isWandering)
        {
            ChooseNewWanderPoint();
        }
    }

    private void MoveToTarget()
    {
        if (isInvestigatingSound || isLookingAround) return;
        float speed = isChasing || isFollowingLastPositions || isWandering ? chaseSpeed : patrolSpeed;
        Vector2 direction = (currentTarget - (Vector2)transform.position).normalized;

        if (direction.magnitude > 0.1f && !isInCombat)
        {
            lookDirection = direction;
        }

        Vector2 targetVelocity = direction * speed;
        rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, movementSmoothing);

        if (isChasing && Vector2.Distance(transform.position, currentTarget) < stoppingDistance)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (isAvoidingObstacle && Vector2.Distance(transform.position, avoidanceTarget) < waypointReachedThreshold)
        {
            isAvoidingObstacle = false;
            if (isChasing && CanSeePlayer())
            {
                currentTarget = player.position;
            }
            else if (isFollowingLastPositions && playerLastPositions.Count > 0)
            {
                currentTarget = playerLastPositions.Peek();
                Debug.Log($"{gameObject.name} resuming to player position: {currentTarget}");
            }
            else if (isWandering)
            {
                ChooseNewWanderPoint();
            }
            else
            {
                currentTarget = GetClosestWaypoint();
            }
        }

        if (isFollowingLastPositions && Vector2.Distance(transform.position, currentTarget) < waypointReachedThreshold)
        {
            if (playerLastPositions.Count > 0)
            {
                Debug.Log($"{gameObject.name} reached player position: {currentTarget}, removing it");
                playerLastPositions.Dequeue();
                if (playerLastPositions.Count > 0)
                {
                    currentTarget = playerLastPositions.Peek();
                    Debug.Log($"{gameObject.name} moving to next player position: {currentTarget}");
                }
                else
                {
                    Debug.Log($"{gameObject.name} no more player positions, starting look around");
                    StartCoroutine(MoveForwardAndRotate());
                }
            }
        }
        else if (isWandering && Vector2.Distance(transform.position, currentTarget) < waypointReachedThreshold)
        {
            Debug.Log($"{gameObject.name} reached wander point: {currentTarget}");
            ChooseNewWanderPoint();
        }
    }

    private void UpdateAnimation()
    {
        animator.SetFloat("Speed", rb.velocity.magnitude);
        if (isInvestigatingSound || isLookingAround)
        {
            animator.SetFloat("Speed", 0);
        }
    }

    private void UpdateRotation()
    {
        if (isLookingAround) return;

        Vector2 targetDirection;

        if (isInCombat && player != null)
        {
            targetDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }
        else if (rb.velocity.magnitude > 0.1f)
        {
            targetDirection = rb.velocity.normalized;
        }
        else
        {
            return;
        }

        lookDirection = targetDirection;
        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Slerp(transform.rotation,
                                            Quaternion.Euler(0, 0, targetAngle),
                                            rotationSpeed * Time.deltaTime);
    }

    private void GetNextWaypoint()
    {
        if (patrolWaypoints.Count == 0) return;

        currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Count;
        currentTarget = patrolWaypoints[currentWaypointIndex].position;
        Debug.Log($"{gameObject.name} moving to next patrol waypoint: {currentTarget}");
    }

    private Vector2 GetClosestWaypoint()
    {
        if (patrolWaypoints.Count == 0) return transform.position;

        int closestIndex = 0;
        float closestDistance = Vector2.Distance(transform.position, patrolWaypoints[0].position);

        for (int i = 1; i < patrolWaypoints.Count; i++)
        {
            float distance = Vector2.Distance(transform.position, patrolWaypoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        currentWaypointIndex = closestIndex;
        return patrolWaypoints[closestIndex].position;
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > maxViewDistance)
            return false;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        if (Vector2.Angle(lookDirection, directionToPlayer) > 45f)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToPlayer,
            distanceToPlayer,
            obstacleLayer
        );

        return hit.collider == null || hit.collider.gameObject == player.gameObject;
    }

    public void TakeGun()
    {
        if (isMafia)
        {
            animator.SetBool("Gun", true);
        }
    }

    public void TakeDamage(int damage)
    {
        SoundManager.Instance.PlayEnemies(EnemiesSoundType.TakeDamage);
        health -= damage;
        Debug.Log("Damage = " + damage);
        StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    private void Death()
    {
        spriteRenderer.color = originalColor;
        animator.SetBool("isDead", true);
        animator.SetBool("GunTaking", false);
        animator.SetBool("Gun", false);
        isDead = true;
        if (isMafia)
        {
            gameObject.transform.localScale = Vector3.one * 2;
        }
        rb.simulated = false;
    }

    private void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = CanSeePlayer() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + lookDirection * 2);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, shootingDistance);

        Gizmos.color = Color.blue;
        for (int i = 0; i < patrolWaypoints.Count; i++)
        {
            if (patrolWaypoints[i] == null) continue;

            Gizmos.DrawSphere(patrolWaypoints[i].position, 0.1f);
            if (i < patrolWaypoints.Count - 1 && patrolWaypoints[i + 1] != null)
            {
                Gizmos.DrawLine(patrolWaypoints[i].position, patrolWaypoints[i + 1].position);
            }
            else if (i == patrolWaypoints.Count - 1 && patrolWaypoints[0] != null)
            {
                Gizmos.DrawLine(patrolWaypoints[i].position, patrolWaypoints[0].position);
            }
        }

        Gizmos.color = Color.cyan;
        foreach (Vector2 pos in playerLastPositions)
        {
            Gizmos.DrawWireSphere(pos, 0.2f);
        }

        if (isAvoidingObstacle)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(avoidanceTarget, 0.3f);
        }

        if (isWandering)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentTarget, 0.3f);
        }
    }
}