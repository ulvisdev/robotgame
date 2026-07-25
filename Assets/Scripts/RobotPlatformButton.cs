using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RobotPlatformButton : MonoBehaviour
{
    [Header("Button Sprites")]
    [SerializeField] private SpriteRenderer buttonSpriteRenderer;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite pressedSprite;

    [Header("Platforms Controlled By This Button")]
    [SerializeField] private List<ButtonMovingPlatform> controlledPlatforms = new();

    [Header("Optional Button Visual")]
    [SerializeField] private Transform buttonVisual;
    [SerializeField] private Vector3 pressedLocalOffset = Vector3.zero;

    private readonly HashSet<Collider2D> activatorCollidersOnButton = new();

    private Vector3 releasedVisualPosition;
    private bool currentPressedState;

    public bool IsPressed => activatorCollidersOnButton.Count > 0;

    [Header("Button Audio")]
    [SerializeField] private AudioClip pressSFX;
    [SerializeField] private AudioClip onbuttonSFX;
    [SerializeField] private AudioClip offbuttonSFX;

    private void Awake()
    {
        Collider2D buttonCollider = GetComponent<Collider2D>();
        buttonCollider.isTrigger = true;

        if (buttonVisual != null)
            releasedVisualPosition = buttonVisual.localPosition;

        if (buttonSpriteRenderer != null)
            buttonSpriteRenderer.sprite = idleSprite;
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
        activatorCollidersOnButton.Clear();
        SetPressed(false, false);

        foreach (ButtonMovingPlatform platform in controlledPlatforms)
        {
            if (platform != null)
                platform.UnregisterButton(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CanPressButton(other))
            return;

        activatorCollidersOnButton.Add(other);
        RefreshButtonState();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        activatorCollidersOnButton.Remove(other);
        RefreshButtonState();
    }

    private bool CanPressButton(Collider2D other)
    {
        Robot robot = other.GetComponentInParent<Robot>();

        if (robot != null)
            return true;

        return other.GetComponentInParent<PressurePlateActivator>() != null;

        // return platform != null;
    }

    private void RefreshButtonState()
    {
        activatorCollidersOnButton.RemoveWhere(collider => collider == null || !collider.enabled);

        SetPressed(IsPressed);
    }

    private void SetPressed(bool pressed, bool playAudio = true)
    {
        if (currentPressedState == pressed)
            return;

        currentPressedState = pressed;

        if (playAudio)
        {
            if (pressed)
            {
                //AudioManager.Instance?.PlaySFX(pressSFX);
                AudioManager.Instance?.PlaySFX(onbuttonSFX);
            }
            else
                AudioManager.Instance?.PlaySFX(offbuttonSFX);
        }

        if (buttonSpriteRenderer != null)
            buttonSpriteRenderer.sprite = pressed ? pressedSprite : idleSprite;

        if (buttonVisual != null)
            buttonVisual.localPosition = pressed ? releasedVisualPosition + pressedLocalOffset : releasedVisualPosition;

        foreach (ButtonMovingPlatform platform in controlledPlatforms)
        {
            if (platform != null)
                platform.SetButtonState(this, pressed);
        }
    }
}