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

    public void ResetToDefaults()
    {
      foreach(var setting in _gameSettings)
      {
        setting.ResetToDefault();
      }
    }
  }
}
