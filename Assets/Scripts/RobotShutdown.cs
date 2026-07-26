using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RobotShutdown : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text usesText;

    [Header("Shutdown Effects")]
    [SerializeField] private AudioClip shutdownSFX;
    [SerializeField] private float cameraShakeDuration = 0.3f;
    [SerializeField] private float cameraShakeStrength = 0.06f;

    private bool hasBeenUsed;

    private void Start()
    {
        UpdateText();
    }

    private void Update()
    {
        if (hasBeenUsed)
            return;

        if (Mouse.current == null)
            return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
            ShutdownAllRobots();
    }

    private void ShutdownAllRobots()
    {
        Robot[] robots = FindObjectsByType<Robot>(FindObjectsSortMode.None);

        foreach (Robot robot in robots)
        {
            if (robot != null)
                robot.ForceShutdown();
        }

        AudioManager.Instance?.PlaySFX(shutdownSFX);
        CameraShake.Instance?.ShakeCamera
            (cameraShakeDuration,
            cameraShakeStrength,
            true,
            true);

        hasBeenUsed = true;
        UpdateText();
    }

    private void UpdateText()
    {
        if (usesText == null)
            return;

        usesText.text = hasBeenUsed
            ? "SHUTDOWN: 0"
            : "SHUTDOWN: 1";
    }
}