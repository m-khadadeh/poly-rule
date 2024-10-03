using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource musicSource;

    public AudioClip click;
    public AudioClip delete;

    public float soundVolume;

    bool mute;

    public bool MuteVal
    {
        get
        {
            return mute;
        }
    }

    float destroyBuffer;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            mute = false;
            musicSource.mute = mute;
            destroyBuffer = -1;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if(destroyBuffer > -1)
        {
            destroyBuffer += Time.deltaTime;
            if(destroyBuffer >= 0.2f)
            {
                destroyBuffer = -1;
            }
        }
    }

    public void PlayClick()
    {
        PlaySound(click);
    }

    public void PlayDelete()
    {
        if (destroyBuffer == -1)
        {
            PlaySound(delete);
            destroyBuffer = 0;
        }
    }

    void PlaySound(AudioClip clip)
    {
        GameObject newSound = new GameObject();
        DontDestroyOnLoad(newSound);
        newSound.transform.parent = this.transform;
        AudioSource newAudio = newSound.AddComponent<AudioSource>();
        newAudio.volume = mute ? 0 : soundVolume;
        newAudio.clip = clip;
        newAudio.playOnAwake = false;
        newAudio.loop = false;
        newAudio.mute = mute;
        newAudio.Play();
        Destroy(newSound,newAudio.clip.length + 0.1f);
    }

    public void Mute()
    {
        mute = !mute;
        musicSource.mute = mute;
    }
}
