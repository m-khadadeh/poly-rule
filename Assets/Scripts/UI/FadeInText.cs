using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FadeInText : MonoBehaviour
{
  [SerializeField] private TextMeshProUGUI _text;
  [SerializeField] private float _initAlpha;
  [SerializeField] private float _lerpTime;

  public delegate void FadeEventComplete();

  public event FadeEventComplete onFadeInComplete;
  public event FadeEventComplete onFadeOutComplete;

  private float _currentAlpha;
  private bool _lerping;
  private float _targetAlpha;
  private float _startingAlpha;
  private float _currentTime;

  private void OnEnable()
  {
    _currentAlpha = _initAlpha;
    _text.alpha = _currentAlpha;
    _lerping = false;
  }

  private void Update()
  {
    if(_lerping)
    {
      if(_currentTime < _lerpTime)
      {
        _currentAlpha = Mathf.Lerp(_startingAlpha, _targetAlpha, _currentTime / _lerpTime);
        _currentTime += Time.deltaTime;
      }
      else
      {
        _lerping = false;
        _currentAlpha = _targetAlpha;
        _currentTime = 0;
        if(_currentAlpha <= 0.1f)
        {
          // Faded out
          onFadeOutComplete?.Invoke();
        }
        else
        {
          // Faded in
          onFadeInComplete?.Invoke();
        }
      }
      _text.alpha = _currentAlpha;
    }
  }

  public void FadeIn()
  {
    _startingAlpha = _currentAlpha;
    _targetAlpha = 1.0f;
    _lerping = true;
  }

  public void FadeOut()
  {
    _startingAlpha = _currentAlpha;
    _targetAlpha = 0.0f;
    _lerping = true;
  }

  public void SetText(string newText)
  {
    _text.text = newText;
  }
}
