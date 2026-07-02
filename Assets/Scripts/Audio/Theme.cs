using UnityEngine;

[CreateAssetMenu(fileName = "Theme", menuName = "Audio/Theme Configuration")]
public class ThemeConfiguration : ScriptableObject
{
    public AudioClip clip;

    [Range(0f, 1f)] public float volume = 1f;
    [Range(.1f, 3f)] public float pitch = 1f;
    public bool loop = true;


    [HideInInspector] public AudioSource source;

    public void Play()
    {
        if (source == null) return;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.Play();
    }
}