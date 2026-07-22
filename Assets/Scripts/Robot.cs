using UnityEngine;

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
    public bool ismoving => moving;

    private Vector2 expectedPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        movingTowardsB = startMovingTowardsB;
        expectedPosition = rb.position;

        if (pointA != null && pointA.IsChildOf(transform))
            pointA.SetParent(null, true);

        if (pointB != null && pointB.IsChildOf(transform))
            pointB.SetParent(null, true);
    }

private void FixedUpdate()
{
    if (pointA == null || pointB == null)
        return;

    Vector2 externalMovement = rb.position - expectedPosition;

    if (externalMovement.sqrMagnitude > 0.000001f)
    {
        pointA.position += (Vector3)externalMovement;
        pointB.position += (Vector3)externalMovement;

        expectedPosition = rb.position;
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

    bool movedSuccessfully = TryMove(movement);

    if (!movedSuccessfully)
    {
        moving = false;
        movingTowardsB = !movingTowardsB;
        expectedPosition = rb.position;
        return;
    }

    expectedPosition = newPosition;

    if (movement.x < 0f)
        sr.flipX = true;
    else if (movement.x > 0f)
        sr.flipX = false;
    
    if (Vector2.Distance(newPosition, targetPosition) <= arrivalDistance)
    {
        moving = false;
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

        moving = !moving;
        expectedPosition = rb.position;

        if (!moving)
            rb.linearVelocity = Vector2.zero;
    }

    void OnDrawGizmos()
    {
        if (pointA == null || pointB == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(pointA.position, pointB.position);
    }

}
