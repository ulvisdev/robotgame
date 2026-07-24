using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private bool transitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.blocksRaycasts = true;
    }

    private void Start()
    {
        FadeIn();
    }

    private void FadeIn()
    {
        fadeCanvasGroup.DOKill();

        fadeCanvasGroup.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
            {fadeCanvasGroup.blocksRaycasts = false;});
    }

    public void FadeToScene(string sceneName)
    {
        if (transitioning)
            return;

        StartCoroutine(FadeRoutine(sceneName));
    }

    private IEnumerator FadeRoutine(string sceneName)
    {
        transitioning = true;
        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.DOKill();

        yield return fadeCanvasGroup.DOFade(1f, fadeDuration).SetUpdate(true).WaitForCompletion();

        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneName);

        while (!loading.isDone)
            yield return null;

        yield return null;

        yield return fadeCanvasGroup.DOFade(0f, fadeDuration).SetUpdate(true).WaitForCompletion();

        fadeCanvasGroup.blocksRaycasts = false;
        transitioning = false;
    }
}