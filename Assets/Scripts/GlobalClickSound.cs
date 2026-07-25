using UnityEngine;

public class GlobalClickSound : MonoBehaviour
{
    [SerializeField] private AudioClip clickSFX;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private bool includeRightClick;

    private void Update()
    {
        bool clicked = Input.GetMouseButtonDown(0) || (includeRightClick && Input.GetMouseButtonDown(1));

        if (clicked)
            AudioManager.Instance?.PlaySFX(clickSFX, volume);
    }
}