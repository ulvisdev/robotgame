using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

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

    [Header("Victory Lighting")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private float normalLightIntensity = 1f;
    [SerializeField] private float victoryLightIntensity = 2f;
    [SerializeField] private float lightChangeDuration = 1f;

    [Header("Energy")]
    [SerializeField] private Bar energyBar;

    private void Awake()
    {
        if (bulbRenderer != null && disabledBulbSprite != null)
            bulbRenderer.sprite = disabledBulbSprite;

        if (globalLight != null)
            globalLight.intensity = normalLightIntensity;
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

        if (energyBar != null)
            energyBar.StopEnergyDrain();

        UnlockNextLevel();

        Robot[] allRobots = FindObjectsByType<Robot>(FindObjectsSortMode.None);

        foreach (Robot robot in allRobots)
        {
            robot.FreezeForLevelFinish(robot == winningRobot);
        }

        if (bulbRenderer != null && enabledBulbSprite != null)
            bulbRenderer.sprite = enabledBulbSprite;


        if (globalLight != null)
        {
            DOTween.To(() => globalLight.intensity,
                value => globalLight.intensity = value,
                victoryLightIntensity,
                lightChangeDuration).SetEase(Ease.OutQuad);
        }

        // Let the winning animation play.
        yield return new WaitForSecondsRealtime(victoryDuration);

        if (SceneFader.Instance != null)
        {
            if (currentLevelNumber < 10)
            {
                SceneFader.Instance.FadeToScene("Level" + (currentLevelNumber + 1));
            }
            else
            {
                SceneFader.Instance.FadeToScene("MainMenu");
            }
        }
        else
        {
            LoadNextSceneImmediately();
        }
    }

    private void UnlockNextLevel()
    {
        int nextLevelNumber = currentLevelNumber + 1;

        int highestUnlocked = PlayerPrefs.GetInt(HighestUnlockedKey, 1);

        if (nextLevelNumber > highestUnlocked && nextLevelNumber <= 10)
        {
            PlayerPrefs.SetInt(HighestUnlockedKey, nextLevelNumber);
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