using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DialogBox : MonoBehaviour
{
    public static DialogBox instance;

    [SerializeField]
    private GameObject box;
    [SerializeField]
    private GameObject content;

    [SerializeField]
    private UnityEngine.UI.RawImage panelImage;

    [SerializeField]
    private float revealStart = 8.0f;
    [SerializeField]
    private float revealEnd = -0.7f;
    [SerializeField]
    private float revealSpeed;
    [SerializeField]
    private Material revealMaterial;

    [SerializeField]
    private TextButton yesButton;
    [SerializeField]
    private TextButton noButton;
    [SerializeField]
    private TMPro.TextMeshProUGUI promptTMP;

    private Coroutine _revealCoroutine;
    private float _revealLerpTime;

    private Action yesAction;
    private Action noAction;

    private bool _currentlyPrompted;

    float countdownTimer;

    string preCountdownText;

    public bool CurrentlyPrompted
    {
        get
        {
            return _currentlyPrompted;
        }
    }

    private void Awake()
    {
      countdownTimer = -2;
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(this.gameObject);
        }
        _currentlyPrompted = false;
    }

  void Update()
  {
    if(_currentlyPrompted)
    {
      if(countdownTimer > 0)
      {
        countdownTimer -= Time.deltaTime;
        promptTMP.text = preCountdownText + Mathf.Ceil(countdownTimer).ToString();
      }
      else if(countdownTimer > -1)
      {
        countdownTimer = -2;
        DialogBox.instance.StartReveal(false);
        DoNoAction();
      }
    }
    
  }

  public void Prompt(string promptText, Action onYes, Action onNo)
    {
      promptTMP.text = promptText;
      yesAction = onYes;
      noAction = onNo;
      _currentlyPrompted = true;
      StartReveal(true);
    }

    

    public void PromptCountdown(string promptText, Action onYes, Action onNo, float countdownSeconds)
    {
      countdownTimer = countdownSeconds;
      preCountdownText = promptText + " Reset in ";
      promptTMP.text = preCountdownText + Mathf.Ceil(countdownTimer).ToString();
      yesAction = onYes;
      noAction = onNo;
      _currentlyPrompted = true;
      StartReveal(true);
    }

    public void StartReveal(bool show)
    {
        box.SetActive(true);
        content.SetActive(false);
        if(_revealCoroutine != null)
        {
            StopCoroutine(_revealCoroutine);
        }
        StartCoroutine(Reveal(show));
    }

    public void DoYesAction()
    {
        _currentlyPrompted = false;
        countdownTimer = -2;
        yesAction?.Invoke();
    }

    public void DoNoAction()
    {
        _currentlyPrompted = false;
        countdownTimer = -2;
        noAction?.Invoke();
    }

    public void ChangeColor(Color panelColor)
    {
        panelImage.color = panelColor;
    }

    public IEnumerator Reveal(bool show)
    {
        _revealLerpTime = 0;
        while(_revealLerpTime < 1)
        {
            revealMaterial.SetFloat("_Progress", Mathf.Lerp(show ? revealStart : revealEnd, show ? revealEnd : revealStart, Mathf.SmoothStep(0.0f, 1.0f, _revealLerpTime)));
            _revealLerpTime += Time.deltaTime * revealSpeed;
            yield return null;
        }
        revealMaterial.SetFloat("_Progress", Mathf.Lerp(show ? revealStart : revealEnd, show ? revealEnd : revealStart, 1.0f));
        content.SetActive(show);
        box.SetActive(show);
        yield return null;
    }

    public void TurnOff()
    {
        box.SetActive(false);
        content.SetActive(false);
    }
}
