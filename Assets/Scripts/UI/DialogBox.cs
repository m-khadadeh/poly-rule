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

    public bool CurrentlyPrompted
    {
        get
        {
            return _currentlyPrompted;
        }
    }

    private void Awake()
    {
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

    public void Prompt(string promptText, Action onYes, Action onNo)
    {
        promptTMP.text = promptText;
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
        yesAction?.Invoke();
    }

    public void DoNoAction()
    {
        _currentlyPrompted = false;
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
            revealMaterial.SetFloat("_Progress", Mathf.Lerp(show ? revealStart : revealEnd, show ? revealEnd : revealStart, _revealLerpTime));
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
