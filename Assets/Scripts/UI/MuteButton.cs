using System.Collections;
using System.Collections.Generic;
using SettingsSystem;
using UnityEngine;

public class MuteButton : MonoBehaviour
{
  [SerializeField] private GameObject _muteButton;
  [SerializeField] private GameObject _unmuteButton;
  [SerializeField] private IntSetting _muteSetting;

  private bool _initialized;

  private void Awake()
  {
    _initialized = false;
  }

  public void Initialize()
  {
    if(_initialized)
      return;
    
    _muteSetting.SubscribeReset(ShowCorrectButton);
  }
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
