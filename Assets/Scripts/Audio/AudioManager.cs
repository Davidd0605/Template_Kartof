using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private SFXConfiguration[] sounds;
    [SerializeField] private ThemeConfiguration[] songs;

    private bool _musicMuted;
    private bool _sfxMuted;
    private float currentMusicVolume = 1f;
    private float currentSFXVolume = 1f;
    private float masterVolume = 1f;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (var sound in sounds)
        {
            if (sound != null)
                sound.source = gameObject.AddComponent<AudioSource>();
        }

        foreach (var song in songs)
        {
            if (song != null)
                song.source = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlaySFX(SFXConfiguration sound, float dynamicPitch)
    {
        if (sound == null)
        {
            Debug.LogWarning("[AudioManager] Tried to play a null AudioConfiguration asset!");
            return;
        }

        sound.pitch = dynamicPitch;
        sound.Play();
    }

    public void PlayTheme(ThemeConfiguration song, float dynamicPitch, bool loop)
    {
        if (song == null)
        {
            Debug.LogWarning("[AudioManager] Tried to play a null AudioConfiguration asset!");
            return;
        }
        if (song.source.isPlaying) return;
        StopAllThemes();


        song.loop = loop;
        song.pitch = dynamicPitch;
        song.Play();
    }

    public void Stop(SFXConfiguration sfxSound)
    {
        if (sfxSound == null || sfxSound.source == null) return;

        sfxSound.source.Stop();
    }

    public void Stop(ThemeConfiguration themeSound)
    {
        if (themeSound == null || themeSound.source == null) return;

        themeSound.source.Stop();
    }

    public void StopAllThemes()
    {
        foreach (ThemeConfiguration song in songs)
        {
            Stop(song);
        }
    }

    public void SetVolume(float volumeMultiplier)
    {
        masterVolume = volumeMultiplier;
        SetVolumeMusic(currentMusicVolume);
        SetVolumeSFX(currentSFXVolume);
    }

    public void SetVolumeMusic(float volumeMultiplier)
    {
        currentMusicVolume = volumeMultiplier;
        foreach (ThemeConfiguration song in songs)
        {
            if (song == null || song.source == null) continue;

            song.source.volume = _musicMuted ? 0f : song.volume * currentMusicVolume * masterVolume;
        }
    }

    public void SetVolumeSFX(float volumeMultiplier)
    {
        currentSFXVolume = volumeMultiplier;
        foreach (SFXConfiguration sound in sounds)
        {
            if (sound == null || sound.source == null) continue;

            sound.source.volume = _sfxMuted ? 0f : sound.volume * currentSFXVolume * masterVolume;
        }
    }

    public void SetMusicMuted(bool muted)
    {
        _musicMuted = muted;
        foreach (var sound in songs)
        {
            sound.source.volume = muted ? 0f : sound.volume * currentMusicVolume;
        }
    }

    public void SetSFXMuted(bool muted)
    {
        _sfxMuted = muted;
        foreach (var sound in sounds)
        {
            sound.source.volume = muted ? 0f : sound.volume * currentSFXVolume;
        }
    }
}