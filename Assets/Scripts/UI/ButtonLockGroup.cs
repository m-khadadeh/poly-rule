using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Button Lock Group", menuName = "Button Lock Group")]
public class ButtonLockGroup : ScriptableObject
{
    private HashSet<LockableButton> buttons;

    public void Subscribe(LockableButton button)
    {
        if(buttons == null)
        {
            buttons = new HashSet<LockableButton>();
        }
        buttons.Add(button);
    }

    public void Unsubscribe(LockableButton button)
    {
        if (buttons == null)
        {
            buttons = new HashSet<LockableButton>();
        }
        if(buttons.Contains(button))
        {
            buttons.Remove(button);
        }
    }

    public void SetInteractability(bool interactable)
    {
        Debug.Log(name + " set to " + interactable);
        if (buttons == null)
        {
            buttons = new HashSet<LockableButton>();
        }
        foreach (var button in buttons)
        {
            button.SetInteractability(interactable);
        }
    }

    public void ClearList()
    {
        if (buttons != null)
            buttons.Clear();
    }
}
