using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSystem : MonoBehaviour
{
    public static SoundSystem instance;

    public int musicVolume = 40;
    public int soundVolume = 50;

    public float startMusicSpeed = 1f;

    private AudioSource audioSource;
    private float targetVolume;
    private bool slowlyStartingMusic = false;

    public void PlaySound(string soundName)
    {
        transform.Find(soundName).GetComponent<AudioSource>().Play();
    }

    public void StopSound(string soundName)
    {
        transform.Find(soundName).GetComponent<AudioSource>().Stop();
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (PlayerPrefs.HasKey("MusicVolume")) musicVolume = PlayerPrefs.GetInt("MusicVolume");
        if (PlayerPrefs.HasKey("SoundVolume")) soundVolume = PlayerPrefs.GetInt("SoundVolume");
        SetMusicVolume(musicVolume);
        SetSoundVolume(soundVolume);

        targetVolume = (float)musicVolume / 100;
        audioSource.volume = 0;
        slowlyStartingMusic = true;
        audioSource.Play();
    }

    private void Update()
    {
        if (slowlyStartingMusic)
        {
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, startMusicSpeed * Time.deltaTime);
            if (Mathf.Abs(audioSource.volume - targetVolume) < 0.01f)
            {
                audioSource.volume = targetVolume;
                slowlyStartingMusic = false;
            }
        }
    }

    public void SetMusicVolume(int musicVolume)
    {
        this.musicVolume = musicVolume;
        PlayerPrefs.SetInt("MusicVolume", musicVolume);

        slowlyStartingMusic = false;
        audioSource.volume = (float)musicVolume / 100;
    }

    public void SetSoundVolume(int soundVolume)
    {
        this.soundVolume = soundVolume;
        PlayerPrefs.SetInt("SoundVolume", soundVolume);

        foreach (Transform child in transform)
        {
            // 0-1 local y position of sound components is used as a volume multiplier
            child.GetComponent<AudioSource>().volume = ((float)soundVolume / 100) * child.localPosition.y;
        }
    }

    public void RestartMusic()
    {
        audioSource.Stop();
        audioSource.volume = 0;
        slowlyStartingMusic = true;
        targetVolume = (float)musicVolume / 100;
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}
