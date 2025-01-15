using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SettingsSystem
{
  public class SettingsSlider : MonoBehaviour
  {
    [SerializeField] private FloatSetting _setting;
    [SerializeField] private Slider _slider;

    private void OnEnable()
    {
      _setting.SubscribeReset(SyncSliderToSetting);
      SyncSliderToSetting();
      _slider.onValueChanged.AddListener(UpdateSetting);
    }

    private void OnDisable()
    {
      _setting.SubscribeReset(SyncSliderToSetting);
      _slider.onValueChanged.RemoveListener(UpdateSetting);
    }

    private void UpdateSetting(float value)
    {
      _setting.Value = value;
    }

    private void SyncSliderToSetting()
    {
      _slider.value = _setting.Value;
    }
  }
}
