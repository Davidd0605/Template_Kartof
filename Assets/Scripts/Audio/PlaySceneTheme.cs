using UnityEngine;

public class PlaySceneTheme : MonoBehaviour
{
    [SerializeField] private ThemeConfiguration sceneTheme;
    [SerializeField] private float targetPitch = 1f;

    void Start()
    {
        if (AudioManager.Instance != null && sceneTheme != null)
        {
            AudioManager.Instance.PlayTheme(sceneTheme, targetPitch, true);

        }

    }
}