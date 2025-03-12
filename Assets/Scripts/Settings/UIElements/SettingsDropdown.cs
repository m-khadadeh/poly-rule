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

    [SerializeField] private bool _askConfirmation;
    [SerializeField] private ButtonLockGroup _optionsGroup;

    private int _previousSetting;

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

    protected void UpdateSetting(int value)
    {
      if(!_askConfirmation)
      {
        _setting.Value = value;
      }
      else
      {
        _previousSetting = _setting.Value;
        _setting.Value = value;
        _optionsGroup.SetInteractability(false);
        DialogBox.instance.PromptCountdown(
            "Keep?",
            () => {_optionsGroup.SetInteractability(true);},
            () => {
              _setting.Value = _previousSetting;
              SyncDropdownToSetting();
              _optionsGroup.SetInteractability(true);
              },
            10);
      }
    }

    private void SyncDropdownToSetting()
    {
      _dropdown.onValueChanged.RemoveListener(UpdateSetting);
      _dropdown.value = _setting.Value;
      _dropdown.onValueChanged.AddListener(UpdateSetting);
    }
  }
}