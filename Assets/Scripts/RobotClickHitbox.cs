using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RobotClickHitbox : MonoBehaviour
{
    [SerializeField] private Robot robot;

    [Header("Click Area")]
    [SerializeField] private Vector2 stoppedSize = new Vector2(1f, 1f);
    [SerializeField] private Vector2 movingSize = new Vector2(1.8f, 1.8f);

    private BoxCollider2D clickCollider;

    private void Awake()
    {
        clickCollider = GetComponent<BoxCollider2D>();

        if (robot == null)
            robot = GetComponentInParent<Robot>();

        clickCollider.isTrigger = true;
    }

    private void Update()
    {
        if (robot == null)
            return;

        clickCollider.size = robot.IsPoweredOn ? movingSize : stoppedSize;
    }

    private void OnMouseDown()
    {
        robot?.HandleClick();
    }
}