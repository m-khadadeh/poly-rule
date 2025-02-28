using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SettingsSystem
{
  [CreateAssetMenu(fileName = "FloatSetting", menuName = "Settings/FloatSetting")]
  public class FloatSetting : GameSetting
  {
    [SerializeField] private float _defaultValue;
    private float _value;

    public float Value {
      get => _value;
      set {
        _value = value;
        NotifyObserversChanged();
        SaveSetting();
      }
    }

    public override void LoadSetting()
    {
      Value = PlayerPrefs.GetFloat(_settingKey, _defaultValue);
    }

    public override void ResetToDefault()
    {
      PlayerPrefs.DeleteKey(_settingKey);
      // Skip the Change event
      _value = _defaultValue;
      NotifyObserversReset();
    }

    public override void SaveSetting()
    {
      PlayerPrefs.SetFloat(_settingKey, _value);
    }
  }
}
