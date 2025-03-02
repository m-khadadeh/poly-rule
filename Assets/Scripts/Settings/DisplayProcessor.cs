using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SettingsSystem
{
  public class DisplayProcessor : MonoBehaviour
  {
    [SerializeField] private IntSetting _displayMode;
    [SerializeField] private IntSetting _resolution;

    [SerializeField] private List<FullScreenMode> _possibleDisplayModes;
    [SerializeField] private List<Vector2Int> _possibleResolutions;

    private bool _initialized;
    void Awake()
    {
      _initialized = false;
    }

    public void Initialize()
    {
      if(_initialized)
        return;

      _displayMode.SubscribeChanged(UpdateDisplay);
      _displayMode.SubscribeReset(UpdateDisplay);
      _resolution.SubscribeChanged(UpdateDisplay);
      _resolution.SubscribeReset(UpdateDisplay);

      UpdateDisplay();
    }

    private void UpdateDisplay()
    {
      Screen.SetResolution(
        _possibleResolutions[_resolution.Value].x,
        _possibleResolutions[_resolution.Value].y,
        _possibleDisplayModes[_displayMode.Value]
      );

      // TODO: Prompt to make sure.
    }
  }
}