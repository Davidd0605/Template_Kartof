using UnityEngine;

[CreateAssetMenu(fileName = "Sound", menuName = "Audio/SFX Configuration")]
public class SFXConfiguration : ScriptableObject
{
    public AudioClip clip;

    [Range(0f, 1f)] public float volume = 1f;
    [Range(.1f, 3f)] public float pitch = 1f;


    [HideInInspector] public AudioSource source;

    public void Play()
    {
        if (source == null) return;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.Play();
    }
}