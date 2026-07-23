using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RobotPlatformButton : MonoBehaviour
{
    [Header("Platforms Controlled By This Button")]
    [SerializeField] private List<ButtonMovingPlatform> controlledPlatforms = new();

    [Header("Optional Button Visual")]
    [SerializeField] private Transform buttonVisual;
    [SerializeField] private Vector3 pressedLocalOffset = new Vector3(0f, -0.08f, 0f);

    private readonly HashSet<Collider2D> robotCollidersOnButton = new();

    private Vector3 releasedVisualPosition;
    private bool currentPressedState;

    public bool IsPressed => robotCollidersOnButton.Count > 0;

    private void Awake()
    {
        Collider2D buttonCollider = GetComponent<Collider2D>();
        buttonCollider.isTrigger = true;

        if (buttonVisual != null)
            releasedVisualPosition = buttonVisual.localPosition;
    }

    private void OnEnable()
    {
        foreach (ButtonMovingPlatform platform in controlledPlatforms)
        {
            if (platform != null)
                platform.RegisterButton(this);
        }

        RefreshButtonState();
    }

    private void OnDisable()
    {
        robotCollidersOnButton.Clear();
        SetPressed(false);

        foreach (ButtonMovingPlatform platform in controlledPlatforms)
        {
            if (platform != null)
                platform.UnregisterButton(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Robot robot = other.GetComponentInParent<Robot>();

        if (robot == null)
            return;

        robotCollidersOnButton.Add(other);
        RefreshButtonState();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        robotCollidersOnButton.Remove(other);
        RefreshButtonState();
    }

    private void RefreshButtonState()
    {
        robotCollidersOnButton.RemoveWhere(collider => collider == null || !collider.enabled);

        SetPressed(IsPressed);
    }

    private void SetPressed(bool pressed)
    {
        if (currentPressedState == pressed)
            return;

        currentPressedState = pressed;

        if (buttonVisual != null)
        {
            buttonVisual.localPosition = pressed ? releasedVisualPosition + pressedLocalOffset : releasedVisualPosition;
        }

        foreach (ButtonMovingPlatform platform in controlledPlatforms)
        {
            if (platform != null)
                platform.SetButtonState(this, pressed);
        }
    }
}