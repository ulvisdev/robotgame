using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class DraggablePlatform : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float snapBackDuration = 0.35f;

    [Header("Visual")]
    [Tooltip("Use a child object containing the platform sprite.")]
    [SerializeField] private Transform visual;

    [SerializeField] private float draggedScaleMultiplier = 1.08f;
    [SerializeField] private float scaleDuration = 0.12f;

    [Header("Return Bounce")]
    [SerializeField] private float punchStrength = 0.12f;
    [SerializeField] private float punchDuration = 0.2f;
    [SerializeField] private int punchVibrato = 5;

    private Rigidbody2D rb;

    [Header("Camera")]
    [SerializeField] private Camera worldCamera;

    private Vector2 dragOffset;
    private Vector2 desiredPosition;
    private Vector3 normalVisualScale;

    private bool isDragging;
    private Tween movementTween;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (visual == null)
            visual = transform;

        normalVisualScale = visual.localScale;

        if (pointA != null)
        {
            rb.position = pointA.position;
            desiredPosition = rb.position;
        }
    }

    private void FixedUpdate()
    {
        if (isDragging)
        {
            rb.MovePosition(desiredPosition);
        }
    }

    private void OnMouseDown()
    {
        if (pointA == null || pointB == null)
            return;

        isDragging = true;

        movementTween?.Kill();
        rb.DOKill();
        visual.DOKill();

        Vector2 mousePosition = GetMouseWorldPosition();
        dragOffset = rb.position - mousePosition;

        visual.DOScale(normalVisualScale * draggedScaleMultiplier, scaleDuration).SetEase(Ease.OutQuad);
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
            return;

        Vector2 mousePosition = GetMouseWorldPosition() + dragOffset;

        desiredPosition = GetClosestPointOnLine(mousePosition, pointA.position, pointB.position);
    }

    private void OnMouseUp()
    {
        if (!isDragging)
            return;

        isDragging = false;
        SnapBack();
    }

    private void SnapBack()
    {
        movementTween?.Kill();
        rb.DOKill();
        visual.DOKill();

        visual.DOScale(normalVisualScale, snapBackDuration * 0.75f)
            .SetEase(Ease.OutQuad);

        movementTween = rb.DOMove(pointA.position, snapBackDuration).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                desiredPosition = pointA.position;

                visual.DOPunchScale(Vector3.one * punchStrength, punchDuration, punchVibrato, 0.5f);
            });
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;

        mouseScreenPosition.z = Mathf.Abs(worldCamera.transform.position.z);

        Vector3 worldPosition =
            worldCamera.ScreenToWorldPoint(mouseScreenPosition);

        return new Vector2(worldPosition.x, worldPosition.y);
    }

    private Vector2 GetClosestPointOnLine(
        Vector2 position,
        Vector2 lineStart,
        Vector2 lineEnd)
    {
        Vector2 line = lineEnd - lineStart;

        if (line.sqrMagnitude <= Mathf.Epsilon)
            return lineStart;

        float progress = Vector2.Dot(position - lineStart, line) / line.sqrMagnitude;

        progress = Mathf.Clamp01(progress);

        return lineStart + line * progress;
    }

    private void OnDisable()
    {
        movementTween?.Kill();
        rb?.DOKill();
        visual?.DOKill();
    }
}