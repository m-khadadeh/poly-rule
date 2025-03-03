using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FadeInText))]
public class Credits : MonoBehaviour
{
  [SerializeField] private List<string> _creditLines;
  [SerializeField] private int _millisecondsPerLine;
  private FadeInText _fadeInText;
  private CallbackTimer _callbackTimer;
  private int _lineCounter;

  private void Start()
  {
    _fadeInText = GetComponent<FadeInText>();
    _fadeInText.onFadeOutComplete += SwapTextAndFadeIn;
    _fadeInText.onFadeInComplete += WaitToFadeOut;

    _lineCounter = 0;
    _fadeInText.SetText(_creditLines[_lineCounter]);
    _fadeInText.FadeIn();
  }

  private void Update()
  {
    _callbackTimer?.ProcessTimer();
  }

  private void SwapTextAndFadeIn()
  {
    _lineCounter = (_lineCounter + 1) % _creditLines.Count;
    _fadeInText.SetText(_creditLines[_lineCounter]);
    _fadeInText.FadeIn();
  }

  private void WaitToFadeOut()
  {
    _callbackTimer = new CallbackTimer(_millisecondsPerLine, ResetTimerAndFadeOut);
  }

  private void ResetTimerAndFadeOut()
  {
    _callbackTimer = null;
    _fadeInText.FadeOut();
  }
}
