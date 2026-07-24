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
    [SerializeField] private float shakeStrength = 0.08f;
    [SerializeField] private int shakeVibrato = 12;

    private bool isReacting;
    private Sequence reactionSequence;
    private Vector3 visualRestLocalPosition;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DirectionHash = Animator.StringToHash("Direction");

    private const float SideDirection = 0f;
    private const float UpDirection = 0.5f;
    private const float DownDirection = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

        Vector2 targetPosition = movingTowardsB
            ? pointB.position
            : pointA.position;

        Vector2 newPosition = Vector2.MoveTowards(
            rb.position,
            targetPosition,
            MoveSpeed * Time.fixedDeltaTime
        );

        Vector2 movement = newPosition - rb.position;

        UpdateFacingAnimation(movement);

        bool movedSuccessfully = TryMove(movement);

        if (!movedSuccessfully)
        {
            // SetMoving(false);
            // movingTowardsB = !movingTowardsB;
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
        }
    }

    private readonly RaycastHit2D[] pushHits = new RaycastHit2D[8];
    private bool TryMove(Vector2 movement)
    {
        float distance = movement.magnitude;

        if (distance <= 0.0001f)
            return true;

        Vector2 direction = movement.normalized;

        int hitCount = rb.Cast(direction, pushHits, distance + 0.01f);

        for (int i = 0; i < hitCount; i++)
        {
            Rigidbody2D hitBody = pushHits[i].rigidbody;

            if (hitBody == null || hitBody == rb)
                continue;

            if (hitBody.TryGetComponent(out Robot pushedRobot))
            {
                if (!pushedRobot.TryMove(movement))
                    return false;
            }
            else
            {
                return false;
            }
        }

        rb.MovePosition(rb.position + movement);
        return true;
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

        expectedPosition = rb != null
            ? rb.position
            : (Vector2)transform.position;

        moving = true;
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

    private void OnDestroy()
    {
        reactionSequence?.Kill();
    }
}
