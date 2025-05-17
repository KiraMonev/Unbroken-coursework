using UnityEngine;
using System;
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

    [Header("Perceptron Settings")]
    [SerializeField] private float[] perceptronWeights = new float[] { 0.5f, 0.3f, 0.8f, -0.4f };
    [SerializeField] private float perceptronBias = 0.1f;
    [SerializeField] private int candidatePointsCount = 5;
    [SerializeField] private float learningRate = 0.1f;
    [SerializeField] private int maxTrainingSamples = 100;

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
    private Vector2? nextWanderPoint;
    private Vector2? lastBadWanderPoint;
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
    private Perceptron perceptron;
    private BehaviorTree behaviorTree;
    private float[] lastWanderInputs;

    private abstract class Node
    {
        public abstract NodeStatus Tick(Mafia mafia);
    }

    private enum NodeStatus
    {
        Success,
        Failure,
        Running
    }

    private class Selector : Node
    {
        private List<Node> children = new List<Node>();

        public Selector(params Node[] nodes)
        {
            children.AddRange(nodes);
        }

        public override NodeStatus Tick(Mafia mafia)
        {
            foreach (var child in children)
            {
                NodeStatus status = child.Tick(mafia);
                if (status != NodeStatus.Failure)
                    return status;
            }
            return NodeStatus.Failure;
        }
    }

    private class Sequence : Node
    {
        private List<Node> children = new List<Node>();

        public Sequence(params Node[] nodes)
        {
            children.AddRange(nodes);
        }

        public override NodeStatus Tick(Mafia mafia)
        {
            foreach (var child in children)
            {
                NodeStatus status = child.Tick(mafia);
                if (status != NodeStatus.Success)
                    return status;
            }
            return NodeStatus.Success;
        }
    }

    private class ActionNode : Node
    {
        private Func<Mafia, NodeStatus> action;

        public ActionNode(Func<Mafia, NodeStatus> action)
        {
            this.action = action;
        }

        public override NodeStatus Tick(Mafia mafia)
        {
            return action(mafia);
        }
    }

    private class ConditionNode : Node
    {
        private Func<Mafia, bool> condition;

        public ConditionNode(Func<Mafia, bool> condition)
        {
            this.condition = condition;
        }

        public override NodeStatus Tick(Mafia mafia)
        {
            return condition(mafia) ? NodeStatus.Success : NodeStatus.Failure;
        }
    }

    [System.Serializable]
    private class Perceptron
    {
        public float[] weights;
        public float bias;
        private List<float[]> trainingInputs = new List<float[]>();
        private List<float> trainingTargets = new List<float>();
        private int maxTrainingSamples;
        private float[] initialWeights;
        private float initialBias;

        public Perceptron(float[] weights, float bias, int maxTrainingSamples)
        {
            this.initialWeights = weights;
            this.weights = (float[])weights.Clone();
            this.initialBias = bias;
            this.bias = bias;
            this.maxTrainingSamples = maxTrainingSamples;
        }

        public float Predict(float[] inputs)
        {
            if (inputs.Length != weights.Length)
            {
                Debug.LogError($"Input size ({inputs.Length}) does not match weights size ({weights.Length})");
                return 0f;
            }

            float sum = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                sum += inputs[i] * weights[i];
            }
            sum += bias;
            return 1f / (1f + Mathf.Exp(-sum));
        }

        public void AddTrainingSample(float[] inputs, float target)
        {
            trainingInputs.Add(inputs);
            trainingTargets.Add(target);
            if (trainingInputs.Count > maxTrainingSamples)
            {
                trainingInputs.RemoveAt(0);
                trainingTargets.RemoveAt(0);
            }
        }

        public void Train(float learningRate)
        {
            if (trainingInputs.Count == 0)
            {
                Debug.LogWarning("No training data available");
                return;
            }

            // Нормализация learning rate и защита от деления на 0
            float adjustedLearningRate = Mathf.Clamp(learningRate / Mathf.Max(1, trainingInputs.Count), 0.001f, 0.1f);

            float totalError = 0f;
            int successfulUpdates = 0;

            for (int i = 0; i < trainingInputs.Count; i++)
            {
                try
                {
                    float[] inputs = trainingInputs[i];
                    if (inputs == null || inputs.Length != weights.Length)
                    {
                        Debug.LogError($"Invalid input data at index {i}");
                        continue;
                    }

                    float target = Mathf.Clamp01(trainingTargets[i]);
                    float prediction = Mathf.Clamp01(Predict(inputs));
                    float error = target - prediction;
                    totalError += Mathf.Abs(error);

                    // Вычисляем производную сигмоиды с защитой от NaN
                    float sigmoidDerivative = Mathf.Max(0.0001f, prediction * (1f - prediction));

                    // Ограниченное и нормализованное изменение весов
                    float scaledError = Mathf.Clamp(error * sigmoidDerivative, -0.5f, 0.5f);

                    // Обновление весов с L2 регуляризацией
                    for (int j = 0; j < weights.Length; j++)
                    {
                        float inputValue = Mathf.Clamp(inputs[j], -1f, 1f);
                        float weightChange = adjustedLearningRate * scaledError * inputValue;

                        // Применяем регуляризацию
                        weights[j] = weights[j] * 0.99f + weightChange;

                        // Ограничиваем абсолютное значение весов
                        weights[j] = Mathf.Clamp(weights[j], -2f, 2f);
                    }

                    // Обновление смещения
                    bias += adjustedLearningRate * scaledError;
                    bias = Mathf.Clamp(bias, -1f, 1f);

                    successfulUpdates++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error during perceptron training: {e.Message}");
                }
            }

            if (successfulUpdates > 0)
            {
                Debug.Log($"Perceptron trained with {successfulUpdates} samples. " +
                         $"Avg error: {totalError / successfulUpdates:F4}\n" +
                         $"Weights: [{string.Join(", ", weights)}]\n" +
                         $"Bias: {bias:F4}\n" +
                         $"Learning rate: {adjustedLearningRate:F4}");
            }
        }

        public void SaveWeights(string prefix)
        {
            Debug.Log($"{prefix} saving weights..."); // Добавлено
            for (int i = 0; i < weights.Length; i++)
            {
                PlayerPrefs.SetFloat($"{prefix}_Weight_{i}", weights[i]);
                Debug.Log($"Saved {prefix}_Weight_{i}: {weights[i]}"); // Добавлено
            }
            PlayerPrefs.SetFloat($"{prefix}_Bias", bias);
            Debug.Log($"Saved {prefix}_Bias: {bias}"); // Добавлено
            PlayerPrefs.Save();
            Debug.Log($"Perceptron weights saved: [{string.Join(", ", weights)}], bias={bias}");
        }

        public void LoadWeights(string prefix)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = PlayerPrefs.GetFloat($"{prefix}_Weight_{i}", initialWeights[i]);
            }
            bias = PlayerPrefs.GetFloat($"{prefix}_Bias", initialBias);
            Debug.Log($"Perceptron weights loaded: [{string.Join(", ", weights)}], bias={bias}");
        }
    }

    private class BehaviorTree
    {
        private Node root;

        public BehaviorTree(Mafia mafia)
        {
            root = new Selector(
                new Sequence(
                    new ConditionNode(m => m.CanSeePlayer()),
                    new ActionNode(m =>
                    {
                        m.HandlePlayerDetection();
                        if (m.isChasing && Time.time - m.lastPositionRecordTime >= m.positionRecordInterval)
                        {
                            m.RecordPlayerPosition();
                        }
                        return NodeStatus.Success;
                    })
                ),
                new Sequence(
                    new ConditionNode(m => m.isChasing),
                    new ActionNode(m =>
                    {
                        m.FollowLastKnownPositionsOrWander();
                        return NodeStatus.Success;
                    })
                ),
                new Sequence(
                    new ConditionNode(m => m.isFollowingLastPositions && m.playerLastPositions.Count == 0),
                    new ActionNode(m =>
                    {
                        Debug.Log($"{m.gameObject.name} no player positions, starting look around");
                        m.StartCoroutine(m.MoveForwardAndRotate());
                        m.isFollowingLastPositions = false;
                        return NodeStatus.Success;
                    })
                ),
                new Sequence(
                    new ConditionNode(m => !m.isChasing && !m.isInvestigatingSound && !m.isFollowingLastPositions),
                    new ConditionNode(m => m.DetectNearbyBulletsForTree()),
                    new ActionNode(m =>
                    {
                        m.StartCoroutine(m.InvestigateSound());
                        return NodeStatus.Success;
                    })
                ),
                new Sequence(
                    new ConditionNode(m => !m.isChasing && !m.isFollowingLastPositions && !m.isWandering && !m.isLookingAround),
                    new ConditionNode(m => Vector2.Distance(m.transform.position, m.currentTarget) < m.waypointReachedThreshold),
                    new ActionNode(m =>
                    {
                        m.GetNextWaypoint();
                        return NodeStatus.Success;
                    })
                ),
                new ActionNode(m => NodeStatus.Success)
            );
        }

        public void Tick(Mafia mafia)
        {
            root.Tick(mafia);
        }
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) Debug.LogError("Player not found!");
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) Debug.LogError("SpriteRenderer not found!");
        originalColor = spriteRenderer.color;
        if (patrolWaypoints.Count == 0)
        {
            Debug.LogError("No patrol waypoints assigned!");
            return;
        }
        if (animator == null) Debug.LogError("Animator not found!");
        if (rb == null) Debug.LogError("Rigidbody2D not found!");

        perceptron = new Perceptron(perceptronWeights, perceptronBias, maxTrainingSamples);
        perceptron.LoadWeights(gameObject.name);
        behaviorTree = new BehaviorTree(this);
        Debug.Log($"{gameObject.name} Behavior Tree initialized");

        currentTarget = patrolWaypoints[currentWaypointIndex].position;
        rb.drag = 5f;
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (isDead) return;

        behaviorTree.Tick(this);

        if (!isLookingAround)
        {
            MoveToTarget();
        }

        // Улучшенная проверка на застревание
        if (!isLookingAround && Vector2.Distance(transform.position, lastPosition) < 0.05f &&
            rb.velocity.magnitude < 0.1f && !isInCombat)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckTimeThreshold)
            {
                Debug.Log($"{gameObject.name} is stuck, handling obstacle avoidance");
                HandleObstacleAvoidance();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
        UpdateAnimation();
        UpdateRotation();

        if (health <= 0)
        {
            Death();
        }
    }

    private bool DetectNearbyBulletsForTree()
    {
        if (isChasing || isFollowingLastPositions) return false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, soundDetectionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Bullet"))
            {
                return true;
            }
        }
        return false;
    }

    public IEnumerator InvestigateSound()
    {
        isInvestigatingSound = true;
        rb.velocity = Vector2.zero;

        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
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
                nextWanderPoint = null;
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
        nextWanderPoint = null;
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
        nextWanderPoint = null;
        lastBadWanderPoint = null;
        ChooseNewWanderPoint();
        Debug.Log($"{gameObject.name} entered wandering mode, initial target: {currentTarget}"); // Добавлено
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
        Vector2 directionToEnemy = (player.position - attackPoint.position).normalized;
        float angleToEnemy = Vector2.Angle(attackPoint.right, directionToEnemy);
        RaycastHit2D hit = Physics2D.Raycast(attackPoint.position, directionToEnemy, Vector2.Distance(player.position, attackPoint.position), obstacleLayer);
        if (angleToEnemy <= attackAngle / 2f && hit.collider == null && Vector2.Distance(player.position, attackPoint.position) < attackRange)
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
                nextWanderPoint = null;
                Debug.Log($"{gameObject.name} following last player position: {currentTarget}");
            }
            else
            {
                Debug.Log($"{gameObject.name} no player positions, starting look around");
                StartCoroutine(MoveForwardAndRotate());
                isFollowingLastPositions = false;
            }
        }
    }

    private IEnumerator MoveForwardAndRotate()
    {
        isLookingAround = true;
        isFollowingLastPositions = false;
        rb.velocity = Vector2.zero;

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

        StartWandering();
    }

    private void ChooseNewWanderPoint(bool forceUpdateCurrent = false)
    {
        bool reachedTarget = Vector2.Distance(transform.position, currentTarget) < waypointReachedThreshold * 2f;

        if (!forceUpdateCurrent && !reachedTarget && Time.time - lastWanderTime < wanderInterval && !isAvoidingObstacle)
        {
            Debug.Log($"{gameObject.name} waiting for wanderInterval, time remaining: {wanderInterval - (Time.time - lastWanderTime)}");
            return;
        }

        // Увеличиваем радиус для поиска точек
        float currentWanderRadius = wanderRadius * 1.5f;

        if (nextWanderPoint.HasValue && !forceUpdateCurrent)
        {
            currentTarget = nextWanderPoint.Value;
            lastWanderInputs = GetPerceptronInputs(currentTarget);
            Debug.Log($"{gameObject.name} moving to next wander point: {currentTarget}");
            nextWanderPoint = null;
            lastWanderTime = Time.time;
            return;
        }

        List<(Vector2 point, float score)> candidates = new List<(Vector2, float)>();
        for (int i = 0; i < candidatePointsCount; i++)
        {
            Vector2 candidatePoint;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
                candidatePoint = (Vector2)transform.position + randomDirection * currentWanderRadius;
                attempts++;
                if (lastBadWanderPoint.HasValue && Vector2.Distance(candidatePoint, lastBadWanderPoint.Value) < 0.5f)
                {
                    candidatePoint = Vector2.zero;
                }
            } while ((!CanMoveDirectlyTo(candidatePoint) || candidatePoint == Vector2.zero) && attempts < maxAttempts);

            if (CanMoveDirectlyTo(candidatePoint))
            {
                float score = EvaluatePointWithPerceptron(candidatePoint);
                candidates.Add((candidatePoint, score));
                Debug.Log($"{gameObject.name} Candidate point {candidatePoint}, score: {score}");
            }
        }

        if (candidates.Count > 0)
        {
            candidates.Sort((a, b) => b.score.CompareTo(a.score));
            nextWanderPoint = candidates[0].point;
            Debug.Log($"{gameObject.name} Selected next wander point: {nextWanderPoint}, score: {candidates[0].score}");
        }
        else
        {
            nextWanderPoint = GetClosestWaypoint();
            lastWanderInputs = GetPerceptronInputs(nextWanderPoint.Value);
            Debug.LogWarning($"{gameObject.name} No valid candidate points, using closest patrol waypoint: {nextWanderPoint}");
        }

        if (currentTarget == Vector2.zero || forceUpdateCurrent || reachedTarget)
        {
            if (candidates.Count > 0)
            {
                currentTarget = candidates[0].point;
                lastWanderInputs = GetPerceptronInputs(currentTarget);
                Debug.Log($"{gameObject.name} Selected initial wander point: {currentTarget}, score: {candidates[0].score}");
            }
            else
            {
                currentTarget = GetClosestWaypoint();
                lastWanderInputs = GetPerceptronInputs(currentTarget);
                Debug.LogWarning($"{gameObject.name} No valid initial candidate points, using closest patrol waypoint: {currentTarget}");
            }
        }

        lastWanderTime = Time.time;
    }

    private float EvaluatePointWithPerceptron(Vector2 point)
    {
        float[] inputs = GetPerceptronInputs(point);
        float score = perceptron.Predict(inputs);
        return score;
    }

    private float[] GetPerceptronInputs(Vector2 point)
    {
        float[] inputs = new float[4];
        float distance = Vector2.Distance(transform.position, point);
        inputs[0] = distance / wanderRadius;
        Vector2 directionToPoint = (point - (Vector2)transform.position).normalized;
        float angle = Vector2.Angle(lookDirection, directionToPoint);
        inputs[1] = angle / 180f;
        inputs[2] = CanMoveDirectlyTo(point) ? 1f : 0f;
        inputs[3] = 0f;
        if (player != null && CanSeePlayer())
        {
            float distanceToPlayer = Vector2.Distance(point, player.position);
            inputs[3] = 1f - Mathf.Clamp01(distanceToPlayer / maxViewDistance);
        }
        return inputs;
    }

    private float EvaluateWanderPointSuccess(Vector2 point)
    {
        // 1. Базовые проверки
        if (point == Vector2.zero)
        {
            Debug.LogWarning("Evaluating zero point");
            return 0f;
        }

        // 2. Параметры для настройки поведения
        float explorationFactor = 0.15f; // Сила исследования
        float minScore = 0.01f;          // Минимальная оценка
        float maxScore = 1.0f;           // Максимальная оценка
        float badPointRadius = 0.75f;    // Радиус "плохой" точки

        // 3. Компоненты оценки
        float baseScore = 0.5f;
        float explorationBonus = UnityEngine.Random.Range(-explorationFactor, explorationFactor);
        float successScore = 0f;

        // 4. Проверка видимости игрока
        if (CanSeePlayer())
        {
            float playerDistanceScore = 1f - Mathf.Clamp01(Vector2.Distance(point, player.position) / maxViewDistance);
            successScore = Mathf.Lerp(0.7f, 1.2f, playerDistanceScore);
            Debug.Log($"Player visible - boosting score to {successScore}");
            return Mathf.Clamp(successScore + explorationBonus, minScore, maxScore);
        }

        // 5. Проверка "плохих" точек
        if (lastBadWanderPoint.HasValue && Vector2.Distance(point, lastBadWanderPoint.Value) < badPointRadius)
        {
            Debug.Log($"Near bad point {lastBadWanderPoint.Value} - reducing score");
            return Mathf.Clamp(baseScore * 0.3f + explorationBonus, minScore, maxScore);
        }

        // 6. Проверка доступности точки
        if (!CanMoveDirectlyTo(point))
        {
            Debug.Log($"Point {point} not reachable - lowest score");
            return minScore;
        }

        // 7. Оценка расстояния
        float distanceToCurrent = Vector2.Distance(transform.position, point);
        float normalizedDistance = Mathf.Clamp01(distanceToCurrent / wanderRadius);

        // 8. Оценка направления
        Vector2 directionToPoint = (point - (Vector2)transform.position).normalized;
        float directionScore = Vector2.Dot(lookDirection, directionToPoint) * 0.5f + 0.5f;

        // 9. Комбинированная оценка
        successScore = baseScore +
                      (normalizedDistance * 0.3f) +
                      (directionScore * 0.2f) +
                      explorationBonus;

        // 10. Учет последних позиций игрока
        if (playerLastPositions.Count > 0)
        {
            Vector2 lastPlayerPos = playerLastPositions.Peek();
            float playerPosScore = 1f - Mathf.Clamp01(Vector2.Distance(point, lastPlayerPos) / wanderRadius);
            successScore += playerPosScore * 0.4f;
        }

        // 11. Финальная корректировка
        successScore = Mathf.Clamp(successScore, minScore, maxScore);

        Debug.Log($"Evaluated point {point} with score: {successScore:F2}\n" +
                  $"Components: base={baseScore:F2}, distance={normalizedDistance:F2}, " +
                  $"direction={directionScore:F2}, exploration={explorationBonus:F2}");

        return successScore;
    }
    private bool CanMoveDirectlyTo(Vector2 target)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, target);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);
        bool canMove = hit.collider == null;
        Debug.Log($"{gameObject.name} CanMoveDirectlyTo {target}: {canMove}, hit: {(hit.collider != null ? hit.collider.name : "none")}");
        return canMove;
    }

    private void HandleObstacleAvoidance()
    {
        if (isAvoidingObstacle || isLookingAround) return;

        isAvoidingObstacle = true;
        stuckTimer = 0f;

        Debug.Log($"{gameObject.name} starting obstacle avoidance");

        if (isWandering)
        {
            lastBadWanderPoint = currentTarget;
            Debug.Log($"{gameObject.name} stuck while wandering to {currentTarget}, choosing new wander point");
            ChooseNewWanderPoint(true);
            isAvoidingObstacle = false;
            return;
        }

        Vector2 directionToTarget = (currentTarget - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, collisionCheckDistance, obstacleLayer);

        if (hit.collider != null)
        {
            // Пробуем несколько направлений для обхода
            Vector2[] avoidanceDirections = {
            new Vector2(-directionToTarget.y, directionToTarget.x).normalized,
            new Vector2(directionToTarget.y, -directionToTarget.x).normalized,
            (Quaternion.Euler(0, 0, 30) * directionToTarget).normalized,
            (Quaternion.Euler(0, 0, -30) * directionToTarget).normalized,
            (Quaternion.Euler(0, 0, 45) * directionToTarget).normalized,
            (Quaternion.Euler(0, 0, -45) * directionToTarget).normalized
        };

            foreach (Vector2 avoidanceDir in avoidanceDirections)
            {
                if (!Physics2D.Raycast(transform.position, avoidanceDir, collisionCheckDistance, obstacleLayer))
                {
                    avoidanceTarget = (Vector2)transform.position + avoidanceDir * obstacleAvoidanceDistance;
                    if (CanMoveDirectlyTo(avoidanceTarget))
                    {
                        currentTarget = avoidanceTarget;
                        Debug.Log($"{gameObject.name} avoiding obstacle in direction {avoidanceDir}, new target: {currentTarget}");
                        isAvoidingObstacle = true;
                        return;
                    }
                }
            }

            // Если все направления заблокированы, пробуем отойти назад
            Vector2 oppositeDir = -directionToTarget;
            if (!Physics2D.Raycast(transform.position, oppositeDir, collisionCheckDistance, obstacleLayer))
            {
                avoidanceTarget = (Vector2)transform.position + oppositeDir * obstacleAvoidanceDistance;
                if (CanMoveDirectlyTo(avoidanceTarget))
                {
                    currentTarget = avoidanceTarget;
                    Debug.Log($"{gameObject.name} avoiding obstacle (opposite), new target: {currentTarget}");
                    isAvoidingObstacle = true;
                    return;
                }
            }
        }

        // Если ничего не помогло, выбираем новую точку
        if (isFollowingLastPositions && playerLastPositions.Count > 0)
        {
            Debug.Log($"{gameObject.name} cannot reach last player position {currentTarget}, removing it");
            playerLastPositions.Dequeue();
            if (playerLastPositions.Count > 0)
            {
                currentTarget = playerLastPositions.Peek();
                nextWanderPoint = null;
                Debug.Log($"{gameObject.name} moving to next player position: {currentTarget}");
            }
            else
            {
                Debug.Log($"{gameObject.name} no more player positions, starting look around");
                StartCoroutine(MoveForwardAndRotate());
                isFollowingLastPositions = false;
            }
        }
        else if (!isChasing && !isFollowingLastPositions && !isWandering)
        {
            Debug.Log($"{gameObject.name} cannot reach patrol waypoint {currentTarget}, skipping to next");
            GetNextWaypoint();
        }

        isAvoidingObstacle = false;
    }

    private void MoveToTarget()
    {
        if (isInvestigatingSound || isLookingAround) return;

        float speed = isChasing || isFollowingLastPositions || isWandering ? chaseSpeed : patrolSpeed;
        Vector2 direction = (currentTarget - (Vector2)transform.position).normalized;

        // Увеличиваем threshold для блуждания
        float currentThreshold = isWandering ? waypointReachedThreshold * 2f : waypointReachedThreshold;

        if (direction.magnitude > 0.1f && !isInCombat)
        {
            lookDirection = direction;
        }

        // Если уже достаточно близко к цели, останавливаемся
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget);
        if (distanceToTarget <= currentThreshold)
        {
            rb.velocity = Vector2.zero;
            HandleTargetReached();
            return;
        }

        Vector2 targetVelocity = direction * speed;
        rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, movementSmoothing);

        // Дополнительная проверка для плавного торможения
        if (distanceToTarget <= currentThreshold * 2f)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.fixedDeltaTime * 5f);
        }

        // Обновляем направление взгляда только если движемся
        if (rb.velocity.magnitude > 0.1f)
        {
            lookDirection = rb.velocity.normalized;
        }
    }

    private void HandleTargetReached()
    {
        Debug.Log($"{gameObject.name} reached target: {currentTarget}");

        if (isFollowingLastPositions)
        {
            if (playerLastPositions.Count > 0)
            {
                playerLastPositions.Dequeue();
                if (playerLastPositions.Count > 0)
                {
                    currentTarget = playerLastPositions.Peek();
                    nextWanderPoint = null;
                    Debug.Log($"{gameObject.name} moving to next player position: {currentTarget}");
                }
                else
                {
                    Debug.Log($"{gameObject.name} no more player positions, starting look around");
                    StartCoroutine(MoveForwardAndRotate());
                    isFollowingLastPositions = false;
                }
            }
            else
            {
                Debug.Log($"{gameObject.name} queue empty, starting look around");
                StartCoroutine(MoveForwardAndRotate());
                isFollowingLastPositions = false;
            }
        }
        else if (isWandering)
        {
            Debug.Log($"{gameObject.name} reached wander point: {currentTarget}");

            if (lastWanderInputs != null)
            {
                float targetScore = EvaluateWanderPointSuccess(currentTarget);
                perceptron.AddTrainingSample(lastWanderInputs, targetScore);
                perceptron.Train(learningRate);
                perceptron.SaveWeights(gameObject.name);
                Debug.Log($"{gameObject.name} trained perceptron with score: {targetScore}");
            }

            lastBadWanderPoint = null;
            ChooseNewWanderPoint();
        }
        else if (!isChasing && !isFollowingLastPositions && !isWandering)
        {
            Debug.Log($"{gameObject.name} reached patrol waypoint, getting next");
            GetNextWaypoint();
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

        if (isWandering && Vector2.Distance(transform.position, currentTarget) < waypointReachedThreshold)
        {
            Debug.Log($"{gameObject.name} target reached, stopping rotation");
            return;
        }

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
            Debug.Log($"{gameObject.name} no velocity, skipping rotation");
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
        nextWanderPoint = null;
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
        ScoreManager.Instance.AddScore(30);
        AchievementManager.instance.AddAchievementProgress("killer", 1);
        
        if (GameAnalytics.Instance != null)
        {
            GameAnalytics.Instance.RegisterEnemyKill();
        }
        if (isMafia)
            {
                transform.localScale = Vector3.one * 2;
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
            if (nextWanderPoint.HasValue)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(nextWanderPoint.Value, 0.3f);
                Gizmos.DrawLine(currentTarget, nextWanderPoint.Value);
            }
        }

        if (lastBadWanderPoint.HasValue)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lastBadWanderPoint.Value, 0.3f);
        }
    }
}