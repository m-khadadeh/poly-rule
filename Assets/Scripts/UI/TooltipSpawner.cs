using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipSpawner : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    public string tooltipText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!DialogBox.instance.CurrentlyPrompted)
        {
            ToolTip.instance.ShowTooltip(tooltipText);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTip.instance.HideTooltip();
    }
}
