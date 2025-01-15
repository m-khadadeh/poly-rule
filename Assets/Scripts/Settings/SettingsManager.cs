using System.Collections;
using System.Collections.Generic;
using SettingsSystem;
using UnityEngine;

namespace SettingsSystem
{
  public class SettingsManager : MonoBehaviour
  {
    [SerializeField] private List<GameSetting> _gameSettings;

    public void Initialize()
    {
      foreach(var setting in _gameSettings)
      {
        setting.Initialize();
      }
    }

    public void LoadAllSettings()
    {
      foreach(var setting in _gameSettings)
      {
        setting.LoadSetting();
      }
    }

    public void SaveAllSettings()
    {
      foreach(var setting in _gameSettings)
      {
        setting.SaveSetting();
      }
    }

    public void ResetToDefaults()
    {
      foreach(var setting in _gameSettings)
      {
        setting.ResetToDefault();
      }
    }
  }
}
