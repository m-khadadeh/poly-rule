using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextButton : LockableButton
{
    [SerializeField]
    protected Color disabledColor;
    [SerializeField]
    protected TMPro.TextMeshProUGUI textMesh;

    public override void SetInteractability(bool interactable)
    {
        //textMesh.color = interactable ? Color.white : disabledColor;
        base.SetInteractability(interactable);
    }
}
