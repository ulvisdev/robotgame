using UnityEngine;
using DG.Tweening;

public class Robot : MonoBehaviour
{

    [Header("Movement Points")]
    public Transform pointA;
    public Transform pointB;
    public bool startMovingTowardsB = true;

    [Header("Movement")]
    public float MoveSpeed = 5f;
    public float arrivalDistance = 0.02f;
    private Rigidbody2D rb;
    private Collider2D robotCollider;
    private SpriteRenderer sr;
    private bool movingTowardsB;
    private bool moving = false;
    public bool ismoving => moving && !isReacting;

    private Vector2 expectedPosition;

    [Header("Visual Reaction")]
    [SerializeField] private Transform visual;
    [SerializeField] private float wallPause = 0.2f;
    [SerializeField] private float togglePause = 0.15f;
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeStrength = 0.1f;
    [SerializeField] private int shakeVibrato = 12;

    private bool isReacting;
    private Sequence reactionSequence;
    private Vector3 visualRestLocalPosition;

    [Header("Robot Collision")]
    [SerializeField] private float robotCollisionCooldown = 0.1f;
    [SerializeField] private float robotSeparationDistance = 0.05f;

    private bool waitForOtherRobotThisStep;
    private float collisionDecisionUntil;

    [Header("Front Ray Detection")]
    [SerializeField] private LayerMask movementBlockerLayers;
    [SerializeField, Range(0.1f, 0.8f)] private float raySpread = 0.5f;
    [SerializeField] private float rayStartPadding = 0.002f;
    [SerializeField] private float rayExtraDistance = 0.01f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DirectionHash = Animator.StringToHash("Direction");
    private static readonly int VictoryHash = Animator.StringToHash("Victory");

    private const float SideDirection = 0f;
    private const float UpDirection = 0.5f;
    private const float DownDirection = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        robotCollider = GetComponent<Collider2D>();
        sr = visual.GetComponentInChildren<SpriteRenderer>();
        animator = visual.GetComponentInChildren<Animator>();

        visualRestLocalPosition = visual.localPosition;

        movingTowardsB = startMovingTowardsB;
        expectedPosition = rb.position;

        if (pointA != null && pointA.IsChildOf(transform))
            pointA.SetParent(null, true);

        if (pointB != null && pointB.IsChildOf(transform))
            pointB.SetParent(null, true);
    }

    private void Start()
    {
        FaceCurrentTarget();
        UpdateAnimationSpeed();
    }

    private void FixedUpdate()
    {
        // animator.SetBool("moving", moving);
        UpdateAnimationSpeed();

        if (pointA == null || pointB == null)
            return;

        Vector2 externalMovement = rb.position - expectedPosition;

        if (externalMovement.sqrMagnitude > 0.000001f)
        {
            pointA.position += (Vector3)externalMovement;
            pointB.position += (Vector3)externalMovement;

            expectedPosition = rb.position;
        }

        if (isReacting)
        {
            expectedPosition = rb.position;
            return;
        }

        if (!moving)
        {
            expectedPosition = rb.position;
            return;
        }

        Vector2 targetPosition = movingTowardsB ? pointB.position : pointA.position;

        Vector2 newPosition = Vector2.MoveTowards(
            rb.position,
            targetPosition,
            MoveSpeed * Time.fixedDeltaTime);

        Vector2 movement = newPosition - rb.position;

        UpdateFacingAnimation(movement);

        waitForOtherRobotThisStep = false;

        bool movedSuccessfully = TryMove(movement);

        if (!movedSuccessfully)
        {
            if (!waitForOtherRobotThisStep)
                PlayWallBounce();

            expectedPosition = rb.position;
            return;
        }

        expectedPosition = newPosition;

        // if (movement.x < 0f)
        //     sr.flipX = true;
        // else if (movement.x > 0f)
        //     sr.flipX = false;

        if (Vector2.Distance(newPosition, targetPosition) <= arrivalDistance)
        {
            // SetMoving(false);
            movingTowardsB = !movingTowardsB;
            expectedPosition = newPosition;
            FaceCurrentTarget();
        }
    }

    private bool TryMove(Vector2 movement, Robot pushingRobot = null)
    {
        float distance = movement.magnitude;

        if (distance <= 0.0001f)
            return true;

        Vector2 direction = movement.normalized;

        if (TryGetObstacleAhead(direction, distance, out RaycastHit2D hit))
        {
            Robot otherRobot = hit.collider.GetComponentInParent<Robot>();

            if (otherRobot != null && otherRobot != this && otherRobot != pushingRobot)
            {
                bool thisRobotIsMoving = moving && !isReacting;

                bool otherRobotIsMoving = otherRobot.moving && !otherRobot.isReacting;

                if (thisRobotIsMoving &&
                    otherRobotIsMoving)
                {
                    HandleMovingRobotCollision(otherRobot);

                    waitForOtherRobotThisStep = true;
                    return false;
                }

                if (!otherRobot.TryMove(movement, this))
                    return false;
            }
            else
                return false;
        }

        rb.MovePosition(rb.position + movement);
        expectedPosition = rb.position;

        return true;
    }

    private bool TryGetObstacleAhead(Vector2 direction, float movementDistance, out RaycastHit2D closestHit)
    {
        closestHit = default;

        if (robotCollider == null)
            return false;

        Bounds bounds = robotCollider.bounds;

        Vector2 sideways = new Vector2(-direction.y, direction.x);

        float forwardExtent = Mathf.Abs(direction.x) * bounds.extents.x + Mathf.Abs(direction.y) * bounds.extents.y;

        float sidewaysExtent = Mathf.Abs(sideways.x) * bounds.extents.x + Mathf.Abs(sideways.y) * bounds.extents.y;

        Vector2 frontCentre = (Vector2)bounds.center + direction * (forwardExtent + rayStartPadding);

        float rayDistance = movementDistance + rayExtraDistance;

        bool foundObstacle = false;

        for (int i = 0; i < 3; i++)
        {
            float spreadMultiplier = i switch
            {
                1 => -raySpread,
                2 => raySpread,
                _ => 0f
            };

            Vector2 origin = frontCentre + sideways * (sidewaysExtent * spreadMultiplier);

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayDistance, movementBlockerLayers);

            Debug.DrawRay(origin, direction * rayDistance, hit.collider != null ? Color.red : Color.green);

            if (hit.collider == null)
                continue;

            if (!foundObstacle || hit.distance < closestHit.distance)
            {
                closestHit = hit;
                foundObstacle = true;
            }
        }

        return foundObstacle;
    }

    private static Robot ChooseReactingRobot(Robot robotA, Robot robotB)
    {
        bool robotAMoving = robotA.moving && !robotA.isReacting;

        bool robotBMoving = robotB.moving && !robotB.isReacting;

        if (robotAMoving && !robotBMoving)
            return robotA;

        if (robotBMoving && !robotAMoving)
            return robotB;

        Vector2 directionAToB = (robotB.rb.position - robotA.rb.position).normalized;

        float approachScoreA = robotAMoving ? Vector2.Dot(robotA.GetDesiredMovementDirection(), directionAToB) : -2f;

        float approachScoreB = robotBMoving ? Vector2.Dot(robotB.GetDesiredMovementDirection(), -directionAToB) : -2f;

        if (approachScoreA > approachScoreB + 0.01f)
            return robotA;

        if (approachScoreB > approachScoreA + 0.01f)
            return robotB;

        return robotA.GetInstanceID() < robotB.GetInstanceID() ? robotA : robotB;
    }

    private Vector2 GetDesiredMovementDirection()
    {
        if (pointA == null || pointB == null)
            return Vector2.zero;

        Vector2 targetPosition = movingTowardsB ? pointB.position : pointA.position;

        return (targetPosition - rb.position).normalized;
    }

    public Vector2 GetCurrentMovementDirection()
    {
        return GetDesiredMovementDirection();
    }

    private void FaceCurrentTarget()
    {
        Vector2 direction = GetDesiredMovementDirection();

        if (direction.sqrMagnitude < 0.000001f)
            return;

        UpdateFacingAnimation(direction);
    }

    private bool IsHeadOnCollision(Robot otherRobot)
    {
        if (otherRobot == null)
            return false;

        if (!moving || isReacting)
            return false;

        if (!otherRobot.moving || otherRobot.isReacting)
            return false;

        Vector2 directionToOther = (otherRobot.rb.position - rb.position).normalized;

        Vector2 myDirection = GetDesiredMovementDirection();

        Vector2 otherDirection = otherRobot.GetDesiredMovementDirection();

        bool iAmApproaching = Vector2.Dot(myDirection, directionToOther) > 0.8f;

        bool otherIsApproaching = Vector2.Dot(otherDirection, -directionToOther) > 0.8f;

        bool directionsAreOpposite = Vector2.Dot(myDirection, otherDirection) < -0.8f;

        return iAmApproaching && otherIsApproaching && directionsAreOpposite;
    }

    private void HandleMovingRobotCollision(Robot otherRobot)
    {
        if (otherRobot == null)
            return;

        if (Time.time < collisionDecisionUntil || Time.time < otherRobot.collisionDecisionUntil)
        {
            return;
        }

        float cooldownEnd = Time.time + wallPause + shakeDuration + robotCollisionCooldown;

        collisionDecisionUntil = cooldownEnd;
        otherRobot.collisionDecisionUntil = cooldownEnd;

        if (IsHeadOnCollision(otherRobot))
        {
            SeparateRobots(otherRobot);
            PlayWallBounce();
            otherRobot.PlayWallBounce();
            return;
        }

        Robot collidingRobot = ChooseReactingRobot(this, otherRobot);

        collidingRobot.PlayWallBounce();
    }

    private void SeparateRobots(Robot otherRobot)
    {
        Vector2 separationDirection = otherRobot.rb.position - rb.position;

        if (separationDirection.sqrMagnitude < 0.000001f)
        {
            separationDirection = GetDesiredMovementDirection();

            if (separationDirection.sqrMagnitude < 0.000001f)
                separationDirection = Vector2.right;
        }

        separationDirection.Normalize();

        Vector2 separationOffset = separationDirection * (robotSeparationDistance * 0.5f);

        rb.position -= separationOffset;
        otherRobot.rb.position += separationOffset;

        expectedPosition = rb.position;
        otherRobot.expectedPosition = otherRobot.rb.position;
    }

    void OnMouseDown()
    {
        Debug.Log("Robot clicked!");

        if (isReacting)
            return;

        PlayPowerToggle();

        // SetMoving(!moving);

        // moving = !moving;
        // expectedPosition = rb.position;

        // if (!moving)
        //     rb.linearVelocity = Vector2.zero;
    }

    void OnDrawGizmos()
    {
        if (pointA == null || pointB == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(pointA.position, pointB.position);
    }

    private void SetMoving(bool value)
    {
        if (moving == value)
            return;

        moving = value;

        // if (animator != null)
        //     animator.SetBool("moving", moving);
    }

    private void PlayWallBounce()
    {
        if (isReacting || visual == null)
            return;

        isReacting = true;

        reactionSequence?.Kill();
        visual.localPosition = visualRestLocalPosition;

        reactionSequence = DOTween.Sequence();
        reactionSequence.AppendInterval(wallPause).Append(visual.DOShakePosition(
                    shakeDuration,
                    new Vector3(shakeStrength, shakeStrength, 0f),
                    shakeVibrato,
                    90f,
                    false,
                    true
                )).OnComplete(() =>
            {
                visual.localPosition = visualRestLocalPosition;
                movingTowardsB = !movingTowardsB;
                expectedPosition = rb.position;
                FaceCurrentTarget();
                isReacting = false;
            }).SetLink(gameObject);
    }

    private void PlayPowerToggle()
    {
        if (visual == null)
            return;

        bool turningOn = !moving;

        isReacting = true;

        reactionSequence?.Kill();
        visual.localPosition = visualRestLocalPosition;

        SetMoving(turningOn);

        reactionSequence = DOTween.Sequence();
        reactionSequence.Append(visual.DOShakePosition(
                    shakeDuration,
                    new Vector3(shakeStrength, shakeStrength, 0f),
                    shakeVibrato,
                    90f,
                    false,
                    true
                )).AppendInterval(togglePause).OnComplete(() =>
            {
                visual.localPosition = visualRestLocalPosition;
                expectedPosition = rb.position;
                isReacting = false;
            }).SetLink(gameObject);
    }

    public void SetMovementPoints(Transform newPointA, Transform newPointB, bool startMovingTowardsB = true)
    {
        if (newPointA == null || newPointB == null)
            return;

        pointA = newPointA;
        pointB = newPointB;

        movingTowardsB = startMovingTowardsB;
        expectedPosition = rb != null ? rb.position : (Vector2)transform.position;

        moving = true;

        FaceCurrentTarget();

    }

    private void UpdateAnimationSpeed()
    {
        if (animator == null)
            return;

        animator.SetFloat(SpeedHash, moving ? 1f : 0f);
    }

    private void UpdateFacingAnimation(Vector2 movement)
    {
        if (animator == null || sr == null)
            return;

        if (movement.sqrMagnitude < 0.000001f)
            return;

        bool movingVertically = Mathf.Abs(movement.y) > Mathf.Abs(movement.x);

        if (movingVertically)
        {
            sr.flipX = false;

            if (movement.y > 0f)
            {
                animator.SetFloat(DirectionHash, UpDirection);
            }
            else
            {
                animator.SetFloat(DirectionHash, DownDirection);
            }
        }
        else
        {
            animator.SetFloat(DirectionHash, SideDirection);

            sr.flipX = movement.x < 0f;
        }
    }

    public void FreezeForLevelFinish(bool playVictoryAnimation)
    {
        moving = false;
        isReacting = false;

        reactionSequence?.Kill();
        reactionSequence = null;

        if (visual != null)
            visual.localPosition = visualRestLocalPosition;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (animator != null)
        {
            animator.SetFloat(SpeedHash, 0f);

            if (playVictoryAnimation)
                animator.SetTrigger(VictoryHash);
        }

        enabled = false;
    }

    private void OnDestroy()
    {
        reactionSequence?.Kill();
    }
}
