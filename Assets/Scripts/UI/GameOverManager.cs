using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    public static bool IsGameOver { get; private set; }

    [Header("Game Over UI")]
    [SerializeField] private CanvasGroup gameOverMenu;
    [SerializeField] private RectTransform menuContent;
    [SerializeField] private Button restartButton;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float startScale = 0.8f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;

    private bool isTransitioning;

    private void Awake()
    {
        Instance = this;

        IsGameOver = false;
        Time.timeScale = 1f;

        gameOverMenu.DOKill();
        menuContent.DOKill();

        gameOverMenu.alpha = 0f;
        gameOverMenu.interactable = false;
        gameOverMenu.blocksRaycasts = false;
        gameOverMenu.gameObject.SetActive(false);

        menuContent.localScale = Vector3.one;
    }

    public void ShowGameOver()
    {
        if (IsGameOver || isTransitioning)
            return;

        IsGameOver = true;
        isTransitioning = true;

        Time.timeScale = 0f;

        gameOverMenu.DOKill();
        menuContent.DOKill();

        gameOverMenu.gameObject.SetActive(true);
        gameOverMenu.alpha = 0f;
        gameOverMenu.interactable = false;
        gameOverMenu.blocksRaycasts = true;

        menuContent.localScale = Vector3.one * startScale;

        EventSystem.current?.SetSelectedGameObject(null);
        Sequence sequence = DOTween.Sequence();

        sequence.SetUpdate(true);

        sequence.Join(gameOverMenu.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad));
        sequence.Join(menuContent.DOScale(1f, fadeDuration).SetEase(openEase));
        sequence.OnComplete(() =>
        {
            gameOverMenu.interactable = true;
            gameOverMenu.blocksRaycasts = true;
            isTransitioning = false;

            if (restartButton != null)
            {
                EventSystem.current?.SetSelectedGameObject(
                    restartButton.gameObject
                );
            }
        });
    }

    public void RestartLevel()
    {
        if (isTransitioning)
            return;

        FadeOutAndLoad(() =>
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        });
    }

    public void GoToMainMenu()
    {
        if (isTransitioning)
            return;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        if (isTransitioning)
            return;

        QuitImmediately();
    }

    private void FadeOutAndLoad(System.Action action)
    {
        isTransitioning = true;

        gameOverMenu.interactable = false;
        gameOverMenu.blocksRaycasts = false;

        EventSystem.current?.SetSelectedGameObject(null);

        gameOverMenu.DOKill();
        menuContent.DOKill();

        Sequence sequence = DOTween.Sequence();
        sequence.SetUpdate(true);

        sequence.Join(gameOverMenu.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));
        sequence.Join(menuContent.DOScale(startScale, fadeDuration).SetEase(closeEase));
        sequence.OnComplete(() =>{Time.timeScale = 1f; action?.Invoke();});
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
        gameOverMenu?.DOKill();
        menuContent?.DOKill();

        IsGameOver = false;
        Time.timeScale = 1f;

        if (Instance == this)
            Instance = null;
    }
}