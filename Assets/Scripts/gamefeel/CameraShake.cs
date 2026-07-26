using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 originalPosition;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        originalPosition = transform.localPosition;
    }

    public void ShakeCamera(float duration, float severity, bool vertical = true, bool horizontal = true)
    {

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            transform.localPosition = originalPosition;
        }

        originalPosition = transform.localPosition;
        shakeRoutine = StartCoroutine(Shake(duration, severity, vertical, horizontal));
    }

    private IEnumerator Shake(float duration, float severity, bool vertical, bool horizontal)
    {
        float remainingTime = duration;

        while (remainingTime > 0f)
        {
            Vector3 shakeOffset = Vector3.zero;

            if (horizontal)
                shakeOffset.x = Random.Range(-1f, 1f) * severity;

            if (vertical)
                shakeOffset.y = Random.Range(-1f, 1f) * severity;

            transform.localPosition = originalPosition + shakeOffset;
            remainingTime -= Time.unscaledDeltaTime;

            yield return null;
        }

        transform.localPosition = originalPosition;
        shakeRoutine = null;
    }

    private void OnDisable()
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        transform.localPosition = originalPosition;
        shakeRoutine = null;
    }
}