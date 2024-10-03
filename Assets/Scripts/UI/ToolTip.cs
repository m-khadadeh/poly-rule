using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ToolTip : MonoBehaviour
{
    public static ToolTip instance;

    [SerializeField]
    private TMPro.TextMeshProUGUI textMesh;
    [SerializeField]
    private TMPro.TextMeshProUGUI flippedTextMesh;

    [SerializeField]
    private Material tooltipMaterial;

    [SerializeField]
    private RawImage tooltipImage;

    [SerializeField]
    private float revealSpeed;

    [SerializeField]
    private float flipRatio;

    private float _revealProgress;

    private bool _showText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
        ImmediatelyHide();
        _revealProgress = 0;
        flippedTextMesh.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (tooltipImage.gameObject.activeSelf)
        {
            flippedTextMesh.text = textMesh.text;
            transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if(Input.mousePosition.x < Screen.width / 2)
            {
                transform.localScale = ((Vector3.right * -1) + Vector3.up + Vector3.forward);
                textMesh.gameObject.SetActive(false && _showText);
                flippedTextMesh.gameObject.SetActive(true && _showText);
            }
            else
            {
                transform.localScale = Vector3.right + Vector3.up + Vector3.forward;
                textMesh.gameObject.SetActive(true && _showText);
                flippedTextMesh.gameObject.SetActive(false && _showText);
            }
        }

    }

    public void BringToIndex(int i)
    {
        transform.SetSiblingIndex(i);
    }

    public void HideTooltip()
    {
        _showText = false;
        this.StopAllCoroutines();
        StartCoroutine(Reveal(-1, () =>
        {
            tooltipImage.gameObject.SetActive(false);
        }));
    }

    public void ImmediatelyHide()
    {
        tooltipImage.gameObject.SetActive(false);
        textMesh.gameObject.SetActive(false);
        flippedTextMesh.gameObject.SetActive(false);
        _revealProgress = 0;
        tooltipMaterial.SetFloat("_ColorFadeProgress", 0);
    }

    public void ShowTooltip(string text)
    {
        //tooltipImage.color = GameManager.instance.currentLevelColors.foregroundColor;
        tooltipImage.gameObject.SetActive(true);
        textMesh.text = text;
        tooltipMaterial.SetFloat("_NumberOfLetters", text.Length);
        this.StopAllCoroutines();
        StartCoroutine(Reveal(1, () =>
        {
            _showText = true;
        }));
    }

    private IEnumerator Reveal(int multiplier, Action callback)
    {
        do
        {
            tooltipMaterial.SetFloat("_ColorFadeProgress", Mathf.Lerp(0, 1, _revealProgress));
            _revealProgress += multiplier * Time.deltaTime * revealSpeed;
            yield return null;
        } while (_revealProgress > 0 && _revealProgress < 1);
        _revealProgress = Mathf.Clamp01(_revealProgress);
        tooltipMaterial.SetFloat("_ColorFadeProgress", Mathf.Lerp(0, 1, _revealProgress));
        callback.Invoke();
        yield return null;
    }
}
