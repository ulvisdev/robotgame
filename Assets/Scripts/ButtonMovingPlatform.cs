using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ButtonMovingPlatform : MonoBehaviour
{
    public enum ButtonRequirement
    {
        AnyButton,
        AllButtons
    }

    [Header("Movement Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool startAtPointA = true;
    [SerializeField] private float movementThreshold = 0.01f;

    [Header("Button Logic")]
    [SerializeField]
    private ButtonRequirement buttonRequirement = ButtonRequirement.AnyButton;

    [Header("Audio")]
    [SerializeField] private AudioClip movementSFX;

    private Rigidbody2D rb;

    private readonly HashSet<RobotPlatformButton> registeredButtons = new();
    private readonly HashSet<RobotPlatformButton> pressedButtons = new();

    private bool? previousTargetWasPointB;
    private bool movementSoundPlayed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (pointA == null || pointB == null)
        {
            enabled = false;
            return;
        }

        rb.position = startAtPointA ? (Vector2)pointA.position : (Vector2)pointB.position;
    }

    private void FixedUpdate()
    {
        RemoveMissingButtons();

        bool moveToPointB = ShouldMoveToPointB();

        Vector2 targetPosition = moveToPointB ? pointB.position : pointA.position;

        if (!previousTargetWasPointB.HasValue || previousTargetWasPointB.Value != moveToPointB)
        {
            previousTargetWasPointB = moveToPointB;
            movementSoundPlayed = false;
        }

        bool isActuallyMoving = Vector2.Distance(rb.position, targetPosition) > movementThreshold;
        if (isActuallyMoving && !movementSoundPlayed)
        {
            movementSoundPlayed = true;

            if (movementSFX != null)
                AudioManager.Instance?.PlaySFX(movementSFX);
        }

        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);

        rb.MovePosition(newPosition);
    }

    public void RegisterButton(RobotPlatformButton button)
    {
        if (button == null)
            return;

        registeredButtons.Add(button);

        if (button.IsPressed)
            pressedButtons.Add(button);
    }

    public void UnregisterButton(RobotPlatformButton button)
    {
        registeredButtons.Remove(button);
        pressedButtons.Remove(button);
    }

    public void SetButtonState(RobotPlatformButton button, bool isPressed)
    {
        if (button == null)
            return;

        registeredButtons.Add(button);

        if (isPressed)
            pressedButtons.Add(button);
        else
            pressedButtons.Remove(button);
    }

    private bool ShouldMoveToPointB()
    {
        if (registeredButtons.Count == 0)
            return false;

        switch (buttonRequirement)
        {
            case ButtonRequirement.AllButtons:
                return pressedButtons.Count >= registeredButtons.Count;

            case ButtonRequirement.AnyButton:
            default:
                return pressedButtons.Count > 0;
        }
    }

    private void RemoveMissingButtons()
    {
        registeredButtons.RemoveWhere(button => button == null);
        pressedButtons.RemoveWhere(button => button == null);
    }
}