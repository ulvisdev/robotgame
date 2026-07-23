using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RobotDirectionSwitch : MonoBehaviour
{
    [Header("New Movement Route")]
    [SerializeField] private Transform newPointA;
    [SerializeField] private Transform newPointB;

    [Header("Direction")]
    [SerializeField] private bool moveTowardsB = true;

    // Robots remain in this list permanently.
    // The switch activates only once for each robot.
    private readonly HashSet<Robot> robotsAlreadySwitched = new();

    private void Awake()
    {
        Collider2D switchCollider = GetComponent<Collider2D>();
        switchCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Robot robot = other.GetComponentInParent<Robot>();

        if (robot == null)
            return;

        if (!robotsAlreadySwitched.Add(robot))
            return;

        robot.SetMovementPoints(newPointA, newPointB, moveTowardsB);
    }
}