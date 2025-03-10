using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LockableButton : MonoBehaviour
{
    [SerializeField]
    protected Selectable uiButton;

    protected EventTrigger eventTrigger;

    [SerializeField]
    protected ButtonLockGroup lockGroup;

    private void Awake()
    {
      lockGroup?.Subscribe(this);
      eventTrigger = GetComponent<EventTrigger>();
    }

    public virtual void SetInteractability(bool interactable)
    {
      uiButton.interactable = interactable;
      if(eventTrigger != null)
      {
        eventTrigger.enabled = interactable;
      }
    }
}
