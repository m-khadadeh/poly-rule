using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SettingsSystem
{
  public class SettingsToggle : MonoBehaviour
  {
    [SerializeField] private IntSetting _setting;
    [SerializeField] private Toggle _toggle;
    // Start is called before the first frame update
    private void OnEnable()
    {
      _setting.SubscribeReset(SyncToggleToSetting);
      SyncToggleToSetting();
      _toggle.onValueChanged.AddListener(UpdateSetting);
    }

    private void OnDisable()
    {
      _setting.UnsubscribeReset(SyncToggleToSetting);
      _toggle.onValueChanged.RemoveListener(UpdateSetting);
    }

    private void UpdateSetting(bool value)
    {
      _setting.Value = value ? 1 : 0;
    }

    private void SyncToggleToSetting()
    {
      _toggle.isOn = _setting.Value == 1;
    }
  }
}


