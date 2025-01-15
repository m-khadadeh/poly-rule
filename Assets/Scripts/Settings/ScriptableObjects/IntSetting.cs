using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SettingsSystem
{
  [CreateAssetMenu(fileName = "IntSetting", menuName = "Settings/IntSetting")]
  public class IntSetting : GameSetting
  {
    [SerializeField] private int _defaultValue;
    private int _value;

    public int Value {
      get => _value;
      set {
        _value = value;
        NotifyObserversChanged();
      }
    }

    public override void LoadSetting()
    {
      Value = PlayerPrefs.GetInt(_settingKey, _defaultValue);
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
      PlayerPrefs.SetInt(_settingKey, _value);
    }
  }
}
