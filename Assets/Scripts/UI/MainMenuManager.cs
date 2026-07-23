using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour
{
    private const string HighestUnlockedKey = "HighestUnlockedLevel";
    private const string VolumeKey = "MasterVolume";

    [Header("Menu Panels")]
    [SerializeField] private CanvasGroup mainPanel;
    [SerializeField] private CanvasGroup levelsPanel;
    [SerializeField] private CanvasGroup optionsPanel;

    [Header("Transitions")]
    [SerializeField] private float fadeOutDuration = 0.35f;
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float transitionDelay = 0.1f;

    [Header("Level Buttons")]
    [SerializeField] private Button[] levelButtons;

    [Header("Options")]
    [SerializeField] private Slider volumeSlider;

    private CanvasGroup currentPanel;
    private bool isTransitioning;

    private void Awake()
    {
        SetupLevelButtons();
        SetupVolume();

        SetPanelInstant(mainPanel, true);
        SetPanelInstant(levelsPanel, false);
        SetPanelInstant(optionsPanel, false);

        currentPanel = mainPanel;
    }

    private void SetPanelInstant(CanvasGroup panel, bool visible)
    {
        if (panel == null)
            return;

        panel.DOKill();

        panel.gameObject.SetActive(visible);
        panel.alpha = visible ? 1f : 0f;
        panel.interactable = visible;
        panel.blocksRaycasts = visible;
    }

    private void TransitionTo(CanvasGroup newPanel)
    {
        if (isTransitioning || newPanel == null)
            return;

        if (currentPanel == newPanel)
            return;

        isTransitioning = true;

        CanvasGroup oldPanel = currentPanel;
        currentPanel = newPanel;

        oldPanel.DOKill();
        newPanel.DOKill();

        oldPanel.interactable = false;
        oldPanel.blocksRaycasts = false;

        newPanel.gameObject.SetActive(true);
        newPanel.alpha = 0f;
        newPanel.interactable = false;
        newPanel.blocksRaycasts = false;

        EventSystem.current?.SetSelectedGameObject(null);

        Sequence transition = DOTween.Sequence();

        transition.SetUpdate(true);

        transition.Append(oldPanel.DOFade(0f, fadeOutDuration).SetEase(Ease.InOutQuad));
        transition.AppendCallback(() =>{oldPanel.gameObject.SetActive(false);});
        transition.AppendInterval(transitionDelay);
        transition.Append(newPanel.DOFade(1f, fadeInDuration).SetEase(Ease.InOutQuad));

        transition.OnComplete(() =>
        {
            newPanel.interactable = true;
            newPanel.blocksRaycasts = true;

            isTransitioning = false;

            EventSystem.current?.SetSelectedGameObject(null);
        });
    }

    private void SetupLevelButtons()
    {
        int highestUnlockedLevel =
            PlayerPrefs.GetInt(HighestUnlockedKey, 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;

            levelButtons[i].gameObject.SetActive(levelNumber <= highestUnlockedLevel);

            int capturedLevelNumber = levelNumber;

            levelButtons[i].onClick.RemoveAllListeners();
            levelButtons[i].onClick.AddListener(() => LoadLevel(capturedLevelNumber));
        }
    }

    private void SetupVolume()
    {
        if (volumeSlider == null)
            return;

        float savedVolume =
            PlayerPrefs.GetFloat(VolumeKey, 1f);

        volumeSlider.SetValueWithoutNotify(savedVolume);
        AudioListener.volume = savedVolume;

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void OpenLevels()
    {
        RefreshLevelButtons();
        TransitionTo(levelsPanel);
    }

    public void OpenOptions()
    {
        TransitionTo(optionsPanel);
    }

    public void ShowMainMenu()
    {
        TransitionTo(mainPanel);
    }

    public void ShowMainMenuFromLevel()
    {
        ShowMainMenu();
    }

    public void ShowMainMenuFromOptions()
    {
        ShowMainMenu();
    }

    public void StartGame()
    {
        int highestUnlockedLevel =
            PlayerPrefs.GetInt(HighestUnlockedKey, 1);

        LoadLevel(highestUnlockedLevel);
    }

    public void LoadLevel(int levelNumber)
    {
        if (isTransitioning)
            return;

        int highestUnlockedLevel =
            PlayerPrefs.GetInt(HighestUnlockedKey, 1);

        if (levelNumber > highestUnlockedLevel)
        {
            Debug.LogWarning("That level is still locked.");
            return;
        }

        FadeOutAndRun(() =>{SceneManager.LoadScene("Level" + levelNumber);});
    }

    private void FadeOutAndRun(System.Action action)
    {
        if (currentPanel == null)
        {
            action?.Invoke();
            return;
        }

        isTransitioning = true;

        currentPanel.DOKill();
        currentPanel.interactable = false;
        currentPanel.blocksRaycasts = false;

        EventSystem.current?.SetSelectedGameObject(null);

        currentPanel
            .DOFade(0f, fadeOutDuration)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(true)
            .OnComplete(() =>{action?.Invoke();});
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;

        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    private void RefreshLevelButtons()
    {
        int highestUnlockedLevel =
            PlayerPrefs.GetInt(HighestUnlockedKey, 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;

            levelButtons[i].gameObject.SetActive(levelNumber <= highestUnlockedLevel);
        }
    }

    public void QuitGame()
    {
        if (isTransitioning)
            return;

        FadeOutAndRun(QuitImmediately);
    }

    private void QuitImmediately()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    [ContextMenu("Reset Level Progress")]
    public void ResetLevelProgress()
    {
        PlayerPrefs.SetInt(HighestUnlockedKey, 1);
        PlayerPrefs.Save();

        RefreshLevelButtons();

        Debug.Log("Level progress reset.");
    }

    private void OnDestroy()
    {
        mainPanel?.DOKill();
        levelsPanel?.DOKill();
        optionsPanel?.DOKill();
    }
}