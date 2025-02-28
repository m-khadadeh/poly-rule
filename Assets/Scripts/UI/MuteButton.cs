using System.Collections;
using System.Collections.Generic;
using SettingsSystem;
using UnityEngine;

public class MuteButton : MonoBehaviour
{
  [SerializeField] private GameObject _muteButton;
  [SerializeField] private GameObject _unmuteButton;
  [SerializeField] private IntSetting _muteSetting;
  private void OnEnable()
  {
    ShowCorrectButton();
  }

  private void ShowCorrectButton()
  {
    _muteButton.SetActive(_muteSetting.Value == 0);
    _unmuteButton.SetActive(_muteSetting.Value != 0);
  }

  public void SetMute(bool value)
  {
    _muteSetting.Value = value ? 1 : 0;
    ShowCorrectButton();
  }
}
