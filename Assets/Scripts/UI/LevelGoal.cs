using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    private const string HighestUnlockedKey = "HighestUnlockedLevel";

    [Header("Level Information")]
    [SerializeField] private int currentLevelNumber = 1;

    [Header("Goal Detection")]
    [SerializeField] private string playerTag = "Robot";

    private bool levelCompleted;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (levelCompleted)
            return;

        if (!other.CompareTag(playerTag))
            return;

        CompleteLevel();
    }

    public void CompleteLevel()
    {
        if (levelCompleted)
            return;

        levelCompleted = true;

        int nextLevelNumber = currentLevelNumber + 1;
        int highestUnlocked =
            PlayerPrefs.GetInt(HighestUnlockedKey, 1);

        if (nextLevelNumber > highestUnlocked &&
            nextLevelNumber <= 10)
        {
            PlayerPrefs.SetInt(HighestUnlockedKey, nextLevelNumber);

            PlayerPrefs.Save();
        }

        if (nextLevelNumber <= 10)
        {
            SceneManager.LoadScene("Level" + nextLevelNumber);
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}