using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SettingsSystem
{
  public class SettingsDropdown : MonoBehaviour
  {
    [SerializeField] private IntSetting _setting;
    [SerializeField] private TMP_Dropdown _dropdown;

    private void OnEnable()
    {
      _setting.SubscribeReset(SyncDropdownToSetting);
      SyncDropdownToSetting();
      _dropdown.onValueChanged.AddListener(UpdateSetting);
    }

    private void OnDisable()
    {
      _setting.SubscribeReset(SyncDropdownToSetting);
      _dropdown.onValueChanged.RemoveListener(UpdateSetting);
    }

    private void UpdateSetting(int value)
    {
      _setting.Value = value;
    }

    private void SyncDropdownToSetting()
    {
      _dropdown.value = _setting.Value;
    }
  }
}