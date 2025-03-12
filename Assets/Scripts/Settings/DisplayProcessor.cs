using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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

    public void SetDefaultToNativeResolution()
    {
      Debug.Log(Display.main.systemWidth + " " + Display.main.systemHeight);
      int height = _possibleResolutions[0].y;
      for(int i = 1; i < _possibleResolutions.Count && height > Display.main.systemHeight; i++)
      {
        height = _possibleResolutions[1].y;
      }
      int width = _possibleResolutions[0].x;
      int resolutionIndex = 0;
      while(resolutionIndex < _possibleResolutions.Count)
      {
        if(height == _possibleResolutions[resolutionIndex].y)
        {
          width = _possibleResolutions[resolutionIndex].x;
          if(width <= Display.main.systemWidth)
          {
            break;
          }
        }
        resolutionIndex++;
      }
      for(int i = 0; i < _possibleResolutions.Count; i++)
      {
        if(_possibleResolutions[i].x == width && _possibleResolutions[i].y == height)
        {
          resolutionIndex = i;
          break;
        }
      }
      _resolution.SetDefaultValue(resolutionIndex);
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