using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
  public static AudioManager instance;

  public SettingsSystem.VolumeProcessor VolumeProcessor => _volumeProcessor;

  [SerializeField] private SettingsSystem.VolumeProcessor _volumeProcessor;

  [SerializeField] private AudioSource _musicSource;
  [SerializeField] private AudioMixerGroup _SFXmixerGroup;

  [SerializeField] private AudioClip _clickSFX;
  [SerializeField] private AudioClip _deleteSFX;

  float _destroyBuffer;

  private void Awake()
  {
      if (instance == null)
      {
          instance = this;
          DontDestroyOnLoad(gameObject);
          _destroyBuffer = -1;
      }
      else
      {
          Destroy(gameObject);
      }
  }

  private void Update()
  {
      if(_destroyBuffer > -1)
      {
          _destroyBuffer += Time.deltaTime;
          if(_destroyBuffer >= 0.2f)
          {
              _destroyBuffer = -1;
          }
      }
  }

  public void PlayClick()
  {
      PlaySound(_clickSFX);
  }

  public void PlayDelete()
  {
      if (_destroyBuffer == -1)
      {
          PlaySound(_deleteSFX);
          _destroyBuffer = 0;
      }
  }

  void PlaySound(AudioClip clip)
  {
      GameObject newSound = new GameObject();
      DontDestroyOnLoad(newSound);
      newSound.transform.parent = this.transform;
      AudioSource newAudio = newSound.AddComponent<AudioSource>();
      newAudio.outputAudioMixerGroup = _SFXmixerGroup;
      newAudio.volume = 1.0f;
      newAudio.clip = clip;
      newAudio.playOnAwake = false;
      newAudio.loop = false;
      newAudio.Play();
      Destroy(newSound,newAudio.clip.length + 0.1f);
  }
}
