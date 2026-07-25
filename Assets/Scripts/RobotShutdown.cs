using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RobotShutdown : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text usesText;

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
        {
            ShutdownAllRobots();
        }
    }

    private void ShutdownAllRobots()
    {
        Robot[] robots = FindObjectsByType<Robot>(FindObjectsSortMode.None);

        foreach (Robot robot in robots)
        {
            robot.ForceShutdown();
        }

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