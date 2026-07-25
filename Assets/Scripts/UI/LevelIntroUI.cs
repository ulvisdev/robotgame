using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class LevelIntroUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform levelTitle;
    [SerializeField] private TMP_Text levelTitleText;
    [SerializeField] private CanvasGroup levelTitleCanvasGroup;

    [Header("Positions")]
    [SerializeField] private RectTransform centerTarget;
    [SerializeField] private RectTransform sideTarget;

    [Header("Level Number")]
    [SerializeField] private bool useSceneBuildIndex = true;
    [SerializeField] private int levelNumber = 1;

    [Header("Animation")]
    [SerializeField] private float popDuration = 0.4f;
    [SerializeField] private float stayInCenterDuration = 0.8f;
    [SerializeField] private float swipeDuration = 0.6f;

    [SerializeField] private float centerScale = 1f;
    [SerializeField] private float sideScale = 0.4f;

    private Sequence introSequence;

    private void Start()
    {
        PlayLevelIntro();
    }

    private void OnDestroy()
    {
        introSequence?.Kill();
    }

    private void PlayLevelIntro()
    {
        int displayedLevel = useSceneBuildIndex ? SceneManager.GetActiveScene().buildIndex : levelNumber;

        levelTitleText.text = $"LEVEL {displayedLevel}";

        levelTitle.position = centerTarget.position;
        levelTitle.localScale = Vector3.zero;
        levelTitleCanvasGroup.alpha = 0f;

        introSequence?.Kill();

        Time.timeScale = 0f;

        introSequence = DOTween.Sequence();

        introSequence.SetUpdate(true);

        introSequence.Append(levelTitleCanvasGroup.DOFade(1f, popDuration * 0.5f));
        introSequence.Join(levelTitle.DOScale(centerScale, popDuration).SetEase(Ease.OutBack));
        introSequence.AppendInterval(stayInCenterDuration);
        introSequence.AppendCallback(() => {Time.timeScale = 1f;});
        introSequence.Append(levelTitle.DOMove(sideTarget.position, swipeDuration).SetEase(Ease.InOutCubic));
        introSequence.Join(levelTitle.DOScale(sideScale, swipeDuration).SetEase(Ease.InOutCubic));
    }
}