using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace SettingsSystem
{
  [System.Serializable]
  public abstract class GameSetting : ScriptableObject
  {
    [SerializeField] protected string _settingKey;
    protected List<Action> _settingChangedObservers;
    protected List<Action> _settingResetObservers;
    public abstract void LoadSetting();
    public abstract void SaveSetting();
    public abstract void ResetToDefault();

    public void Initialize()
    {
      _settingChangedObservers = new List<Action>();
      _settingResetObservers = new List<Action>();
    }

    public void SubscribeChanged(Action eventHandler)
    {
      _settingChangedObservers.Add(eventHandler);
    }

    public void UnsubscribeChanged(Action eventHandler)
    {
      _settingChangedObservers.Remove(eventHandler);
    }

    public void SubscribeReset(Action eventHandler)
    {
      _settingResetObservers.Add(eventHandler);
    }

    public void UnsubscribeReset(Action eventHandler)
    {
      _settingResetObservers.Remove(eventHandler);
    }

    public virtual void NotifyObserversChanged()
    {
      foreach(var observer in _settingChangedObservers)
      {
        observer.Invoke();
      }
    }
    public void FlushObserversChanged()
    {
      _settingChangedObservers.Clear();
    }

    public virtual void NotifyObserversReset()
    {
      foreach(var observer in _settingResetObservers)
      {
        observer.Invoke();
      }
    }

    public void FlushObserversReset()
    {
      _settingResetObservers.Clear();
    }
  }
}

