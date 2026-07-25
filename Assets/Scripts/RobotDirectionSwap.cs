// using System.Collections.Generic;
// using UnityEngine;

// [RequireComponent(typeof(Collider2D))]
// public class RobotDirectionSwitch : MonoBehaviour
// {
//     private enum EntrySide
//     {
//         Left,
//         Right,
//         Top,
//         Bottom
//     }

//     [Header("New Movement Route")]
//     [SerializeField] private Transform newPointA;
//     [SerializeField] private Transform newPointB;

//     [Header("Direction")]
//     [SerializeField] private bool moveTowardsB = true;

//     [Header("Allowed Entry Side")]
//     [SerializeField] private EntrySide allowedEntrySide = EntrySide.Left;

//     // Prevents multiple trigger calls while the same robot is still inside.
//     private readonly HashSet<Robot> robotsInside = new();

//     private void Awake()
//     {
//         GetComponent<Collider2D>().isTrigger = true;
//     }

//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         Robot robot = other.GetComponentInParent<Robot>();

//         if (robot == null)
//             return;

//         if (!robotsInside.Add(robot))
//             return;

//         if (!EnteredFromAllowedSide(robot.transform.position))
//             return;

//         robot.SetMovementPoints(newPointA, newPointB, moveTowardsB);
//     }

//     private void OnTriggerExit2D(Collider2D other)
//     {
//         Robot robot = other.GetComponentInParent<Robot>();

//         if (robot == null)
//             return;

//         // The robot can activate this switch again after leaving.
//         robotsInside.Remove(robot);
//     }

//     private bool EnteredFromAllowedSide(Vector3 robotWorldPosition)
//     {
//         // Converts the robot position into the plate's local coordinates.
//         // This means rotating the plate also rotates its allowed sides.
//         Vector3 localPosition =
//             transform.InverseTransformPoint(robotWorldPosition);

//         return allowedEntrySide switch
//         {
//             EntrySide.Left   => localPosition.x < 0f,
//             EntrySide.Right  => localPosition.x > 0f,
//             EntrySide.Top    => localPosition.y > 0f,
//             EntrySide.Bottom => localPosition.y < 0f,
//             _ => false
//         };
//     }
// }

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RobotDirectionSwitch : MonoBehaviour
{
    [Header("New Movement Route")]
    [SerializeField] private Transform newPointA;
    [SerializeField] private Transform newPointB;

    [Header("Starting Direction On New Route")]
    [SerializeField] private bool moveTowardsB = true;

    [Header("Entry Check")]
    [Range(0f, 1f)] [SerializeField] private float requiredDirectionAmount = 0.5f;

    private readonly HashSet<Robot> robotsInside = new();

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Robot robot = other.GetComponentInParent<Robot>();

        if (robot == null)
            return;

        Vector2 worldMovementDirection =
            robot.GetMovementDirection();

        Vector2 localMovementDirection = transform.InverseTransformDirection(worldMovementDirection);

        if (localMovementDirection.y < requiredDirectionAmount)
            return;

        if (!robotsInside.Add(robot))
            return;

        robot.SetMovementPoints( newPointA, newPointB, moveTowardsB);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Robot robot = other.GetComponentInParent<Robot>();

        if (robot == null)
            return;

        robotsInside.Remove(robot);
    }
}