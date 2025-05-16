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
    private bool isInvestigatingSound = false;


    [Header("Visual Effects")]
    [SerializeField] private float flashDuration = 0.2f; // ������������ ����������� ��� �����

    [Header("References")]
    private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private List<Transform> patrolWaypoints = new List<Transform>();
    [SerializeField] private LayerMask playerLayer;
    private SpriteRenderer spriteRenderer; // ��� ������� �����������

    private bool isSpotted=false;
    private Color originalColor;
    private bool isDead = false;
    private bool isChasing = false;
    private bool isInCombat = false;
    private int currentWaypointIndex = 0;
    private Vector2 currentTarget;
    private Vector2 lookDirection = Vector2.right;
    private float lastShotTime;
    private Coroutine shootingCoroutine;
    private Vector2 m_Velocity = Vector2.zero;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>(); // ������������� SpriteRenderer
        originalColor = spriteRenderer.color;
        if (patrolWaypoints.Count == 0)
        {
            Debug.LogError("No patrol waypoints assigned!");
            return;
        }
        currentTarget = patrolWaypoints[currentWaypointIndex].position;
        rb.drag = 5f;
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
        }
        else if (isChasing)
        {
            ReturnToPatrol();
        }

        MoveToTarget();
        UpdateAnimation();
        UpdateRotation();

        if (!isChasing && Vector2.Distance(transform.position, currentTarget) < waypointReachedThreshold)
        {
            GetNextWaypoint();
        }

        if (health <= 0)
        {
            Death();
        }
    }

    private void DetectNearbyBullets()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, soundDetectionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Bullet")) // �������, ��� � ���� ����� ��� "Bullet"
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
                isInvestigatingSound = false; // ��������� �������������
                HandlePlayerDetection();      // ����� �������� ������������
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (!CanSeePlayer())
        {
            isInvestigatingSound = false;
            currentTarget = GetClosestWaypoint();
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

    private void ReturnToPatrol()
    {
        isChasing = false;
        isInCombat = false;
        if (isMafia)
        {
            animator.SetBool("Gun", false);
        }
        else
        {
            animator.SetBool("Gun", false);
            animator.SetBool("GunTaking", false);
        }
        currentTarget = GetClosestWaypoint();

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
            Debug.Log($"���� ��������!");
        }
    }

    private void MoveToTarget()
    {
        if (isInvestigatingSound) return; // <<< ��������������� �� ����� �������������
        float speed = isChasing ? chaseSpeed : patrolSpeed;
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
    }

    private void UpdateAnimation()
    {
        animator.SetFloat("Speed", rb.velocity.magnitude);
        if(isInvestigatingSound)
        {
            animator.SetFloat("Speed", 0);
        }
    }

    private void UpdateRotation()
    {
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
        StartCoroutine(FlashRed()); // ��������� ������ �����������
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red; // ������������� ������� ����
        yield return new WaitForSeconds(flashDuration); // ����
        spriteRenderer.color = originalColor; // ���������� �������� ����
    }

    private void Death()
    {
        GameAnalytics.Instance.RegisterEnemyKill();
        
        spriteRenderer.color = originalColor;
        animator.SetBool("isDead", true);
        animator.SetBool("GunTaking", false);
        animator.SetBool("Gun", false);
        isDead = true;
        ScoreManager.Instance.AddScore(30);
        
        if (isMafia)
        {
            gameObject.transform.localScale = Vector3.one * 2;
        }
        rb.simulated = false;

        ManagerLevel2 mgr2 = FindObjectOfType<ManagerLevel2>();
        if (mgr2 != null)
        {
            mgr2.RegisterEnemyKilled();
        }
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
    }
}