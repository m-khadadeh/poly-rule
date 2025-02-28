using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Audio;

namespace SettingsSystem
{
  public class VolumeProcessor : MonoBehaviour
  {
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private List<VolumeSettingSubcategory> _subcategories;

    bool _initialized;

    void Awake()
    {
      _initialized = false;
    }

    public void Initialize()
    {
      if(_initialized)
        return;

      foreach(var subcategory in _subcategories)
      {
        subcategory.Initialize(_mixer);
      }
    }

    [System.Serializable]
    private class VolumeSettingSubcategory
    {
      [SerializeField] private string _mixerPropertyName;
      [SerializeField] private FloatSetting _rawVolume;
      [SerializeField] private IntSetting _mute;
      

      private AudioMixer _mixer;

      public void Initialize(AudioMixer mixer)
      {
        _mixer = mixer;
        _rawVolume.SubscribeChanged(ProcessValues);
        _rawVolume.SubscribeReset(ProcessValues);
        _mute.SubscribeChanged(ProcessValues);
        _mute.SubscribeReset(ProcessValues);
        ProcessValues();
      }

      public void ProcessValues()
      {
        float muteMultiplier = (_mute.Value == 0 ? 1.0f : 0.0f); // Invert the mute value
        _mixer.SetFloat(_mixerPropertyName, Mathf.Log10(
          Mathf.Clamp(
            muteMultiplier *_rawVolume.Value, // Apply mute
            0.0001f, 1.0f) // Clamp to valid range for logarithmic conversion
          ) * 20); // Convert to decibels
      }
    }
  }
}