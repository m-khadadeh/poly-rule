using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SettingsSystem
{
  [CreateAssetMenu(fileName = "StringSetting", menuName = "Settings/StringSetting")]
  public class StringSetting : GameSetting
  {
    [SerializeField] private string _defaultValue;
    private string _value;

    public string Value {
      get => _value;
      set {
        _value = value;
        NotifyObserversChanged();
        SaveSetting();
      }
    }

    public override void LoadSetting()
    {
      Value = PlayerPrefs.GetString(_settingKey, _defaultValue);
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
      PlayerPrefs.SetString(_settingKey, _value);
    }
  }
}
