using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    private const string HighestUnlockedKey = "HighestUnlockedLevel";

    [Header("Level Information")]
    [SerializeField] private int currentLevelNumber = 1;

    [Header("Goal Detection")]
    [SerializeField] private string playerTag = "Robot";

    [Header("Light Bulb")]
    [SerializeField] private SpriteRenderer bulbRenderer;
    [SerializeField] private Sprite disabledBulbSprite;
    [SerializeField] private Sprite enabledBulbSprite;

    [Header("Victory Timing")]
    [SerializeField] private float victoryDuration = 1f;

    private bool levelCompleted;

    private void Awake()
    {
        if (bulbRenderer != null && disabledBulbSprite != null)
            bulbRenderer.sprite = disabledBulbSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (levelCompleted)
            return;

        if (!other.CompareTag(playerTag))
            return;

        Robot winningRobot = other.GetComponentInParent<Robot>();

        if (winningRobot == null)
            return;

        StartCoroutine(CompleteLevelRoutine(winningRobot));
    }

    private IEnumerator CompleteLevelRoutine(Robot winningRobot)
    {
        if (levelCompleted)
            yield break;

        levelCompleted = true;

        UnlockNextLevel();

        // Freeze every robot. Only the robot at the goal celebrates.
        Robot[] allRobots = FindObjectsByType<Robot>(
            FindObjectsSortMode.None
        );

        foreach (Robot robot in allRobots)
        {
            robot.FreezeForLevelFinish(robot == winningRobot);
        }

        // Turn on the bulb.
        if (bulbRenderer != null && enabledBulbSprite != null)
            bulbRenderer.sprite = enabledBulbSprite;

        // Let the winning animation play.
        yield return new WaitForSecondsRealtime(victoryDuration);

        if (SceneFader.Instance != null)
        {
            if (currentLevelNumber < 10)
            {
                SceneFader.Instance.FadeToScene(
                    "Level" + (currentLevelNumber + 1)
                );
            }
            else
            {
                SceneFader.Instance.FadeToScene("MainMenu");
            }
        }
        else
        {
            Debug.LogWarning(
                "SceneFader not found. Loading scene without fading."
            );

            LoadNextSceneImmediately();
        }
    }

    private void UnlockNextLevel()
    {
        int nextLevelNumber = currentLevelNumber + 1;

        int highestUnlocked =
            PlayerPrefs.GetInt(HighestUnlockedKey, 1);

        if (nextLevelNumber > highestUnlocked &&
            nextLevelNumber <= 10)
        {
            PlayerPrefs.SetInt(
                HighestUnlockedKey,
                nextLevelNumber
            );

            PlayerPrefs.Save();
        }
    }

    private void LoadNextSceneImmediately()
    {
        int nextLevelNumber = currentLevelNumber + 1;

        if (nextLevelNumber <= 10)
            SceneManager.LoadScene("Level" + nextLevelNumber);
        else
            SceneManager.LoadScene("MainMenu");
    }
}