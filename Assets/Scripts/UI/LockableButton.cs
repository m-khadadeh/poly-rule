using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockableButton : MonoBehaviour
{
    [SerializeField]
    protected Button uiButton;

    [SerializeField]
    protected ButtonLockGroup lockGroup;

    public Button UiButton {
        get {
            return uiButton;
        }
    }

    private void Awake()
    {
        lockGroup?.Subscribe(this);
        uiButton.onClick.AddListener(() =>
        {
            AudioManager.instance.PlayClick();
        });
    }

    public virtual void SetInteractability(bool interactable)
    {
        uiButton.interactable = interactable;
    }
}
