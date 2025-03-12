using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipSpawner : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  [SerializeField] private FadeInText _text;
  private UnityEngine.UI.Button _button;

  private void Awake()
  {
    _button = GetComponent<UnityEngine.UI.Button>(); 
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if(_button != null && _button.interactable)
      _text.FadeIn();
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    _text.FadeOut();
  }
}
