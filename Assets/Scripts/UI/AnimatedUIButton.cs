using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class AnimatedUIButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("Press Animation")]
    [SerializeField] private float pressDistance = 4f;
    [SerializeField] private float pressedScale = 0.96f;
    [SerializeField] private float animationDuration = 0.08f;

    [Header("Pressed Appearance")]
    [SerializeField] private Color pressedColor = new Color(
        0.7f,
        0.7f,
        0.7f,
        1f
    );

    private RectTransform rectTransform;
    private Image buttonImage;
    private Button button;

    private Vector2 originalPosition;
    private Vector3 originalScale;
    private Color originalColor;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();
        button = GetComponent<Button>();

        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;
        originalColor = buttonImage.color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable)
            return;

        rectTransform.DOKill();
        buttonImage.DOKill();

        rectTransform.DOAnchorPos(originalPosition + Vector2.down * pressDistance,animationDuration).SetEase(Ease.OutQuad).SetUpdate(true);
        rectTransform.DOScale(originalScale * pressedScale, animationDuration).SetEase(Ease.OutQuad).SetUpdate(true);

        buttonImage.DOColor(pressedColor, animationDuration).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ReturnToNormal();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ReturnToNormal();
    }

    private void ReturnToNormal()
    {
        rectTransform.DOKill();
        buttonImage.DOKill();

        rectTransform.DOAnchorPos(originalPosition, animationDuration).SetEase(Ease.OutBack).SetUpdate(true);

        rectTransform.DOScale(originalScale, animationDuration).SetEase(Ease.OutBack).SetUpdate(true);

        buttonImage.DOColor(originalColor, animationDuration).SetUpdate(true);
    }

    private void OnDisable()
    {
        rectTransform?.DOKill();
        buttonImage?.DOKill();

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
            rectTransform.localScale = originalScale;
        }

        if (buttonImage != null)
            buttonImage.color = originalColor;
    }
}