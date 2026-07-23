using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private CanvasGroup pauseMenu;
    [SerializeField] private RectTransform pauseMenuContent;
    [SerializeField] private Button resumeButton;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float startScale = 0.9f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;

    private bool isPaused;
    private bool isTransitioning;

    private void Awake()
    {
        Time.timeScale = 1f;

        pauseMenu.DOKill();
        pauseMenuContent.DOKill();

        pauseMenu.alpha = 0f;
        pauseMenu.interactable = false;
        pauseMenu.blocksRaycasts = false;
        pauseMenu.gameObject.SetActive(false);

        pauseMenuContent.localScale = Vector3.one;
    }

    private void Update()
    {
        if (GameOverManager.IsGameOver)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        if (!isPaused && !isTransitioning && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartLevel();
        }
    }

    public void TogglePause()
    {
        if (isTransitioning)
            return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused || isTransitioning)
            return;

        isPaused = true;
        isTransitioning = true;

        Time.timeScale = 0f;

        pauseMenu.DOKill();
        pauseMenuContent.DOKill();

        pauseMenu.gameObject.SetActive(true);
        pauseMenu.alpha = 0f;
        pauseMenu.interactable = false;
        pauseMenu.blocksRaycasts = true;

        pauseMenuContent.localScale = Vector3.one * startScale;

        EventSystem.current?.SetSelectedGameObject(null);

        Sequence openSequence = DOTween.Sequence();
    
        openSequence.SetUpdate(true);

        openSequence.Join(pauseMenu.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad));

        openSequence.Join(pauseMenuContent.DOScale(1f, fadeDuration).SetEase(openEase));

        openSequence.OnComplete(() =>
        {
            pauseMenu.interactable = true;
            pauseMenu.blocksRaycasts = true;
            isTransitioning = false;

            if (resumeButton != null)
            {
                EventSystem.current?.SetSelectedGameObject(
                    resumeButton.gameObject
                );
            }
        });
    }

    public void ResumeGame()
    {
        if (!isPaused || isTransitioning)
            return;

        isTransitioning = true;

        pauseMenu.interactable = false;
        pauseMenu.blocksRaycasts = false;

        pauseMenu.DOKill();
        pauseMenuContent.DOKill();

        EventSystem.current?.SetSelectedGameObject(null);

        Sequence closeSequence = DOTween.Sequence();
        closeSequence.SetUpdate(true);

        closeSequence.Join(pauseMenu.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));

        closeSequence.Join(pauseMenuContent.DOScale(startScale, fadeDuration).SetEase(closeEase));

        closeSequence.OnComplete(() =>
        {
            pauseMenu.gameObject.SetActive(false);
            pauseMenuContent.localScale = Vector3.one;

            isPaused = false;
            isTransitioning = false;

            Time.timeScale = 1f;
        });
    }

    public void RestartLevel()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void GoToMainMenu()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        pauseMenu.interactable = false;
        pauseMenu.blocksRaycasts = false;

        pauseMenu.DOKill();
        pauseMenuContent.DOKill();

        EventSystem.current?.SetSelectedGameObject(null);

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        pauseMenu.interactable = false;
        pauseMenu.blocksRaycasts = false;

        pauseMenu.DOKill();
        pauseMenuContent.DOKill();

        QuitImmediately();
    }

    private void QuitImmediately()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        pauseMenu?.DOKill();
        pauseMenuContent?.DOKill();

        Time.timeScale = 1f;
    }
}