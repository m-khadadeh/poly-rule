using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipSpawner : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  [SerializeField] private FadeInText _text;

  public void OnPointerEnter(PointerEventData eventData)
  {
    _text.FadeIn();
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    _text.FadeOut();
  }
}
