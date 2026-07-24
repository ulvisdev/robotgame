using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [SerializeField] private AudioClip sceneMusic;

    private void Start()
    {
        AudioManager.Instance?.PlayMusic(sceneMusic);
    }
}