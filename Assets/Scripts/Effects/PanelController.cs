using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    [SerializeField] private RawImage _mainPanel;
    [SerializeField] private RawImage _colorSwapPanel;

    [SerializeField] private float _speed;

    [SerializeField] private Canvas parentCanvas;
    public Camera canvasCamera;

    [Header("Tests")]
    [SerializeField] private bool _testSwipe;
    [SerializeField] private bool _testSwipeOff;
    [SerializeField] private bool _testColorSwipe;
    [SerializeField] private bool _testColorChange;
    [SerializeField] private Color _testColorToSwapTo;

    private Coroutine currentCoroutine;
    private float _lerpFraction;

    public static PanelController instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            canvasCamera = parentCanvas.worldCamera;
            Destroy(canvasCamera.gameObject);
            Destroy(this.parentCanvas.gameObject); // Destroying the whole canvas
        }
        else if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance.parentCanvas.gameObject); // Keeping the whole canvas
            canvasCamera = parentCanvas.worldCamera;
            DontDestroyOnLoad(canvasCamera.gameObject);

        }
    }

    public void Start()
    {
        _mainPanel.material.SetFloat("_Progress", 1);
        _colorSwapPanel.material.SetFloat("_Progress", 1);
    }

    private void Update()
    {
        if(_testSwipe)
        {
            _testSwipe = false;
            SwipePanelOn();
        }

        if (_testSwipeOff)
        {
            _testSwipeOff = false;
            SwipePanelOff();
        }
        /*
        if(_testColorSwipe)
        {
            _testColorSwipe = false;
            SwipePanelColorChange(_testColorToSwapTo);
        }
        */
        if (_testColorChange)
        {
            _testColorChange = false;
            ChangePanelColor(_testColorToSwapTo);
        }
    }

    public float GetPanelProgress()
    {
        return _mainPanel.material.GetFloat("_Progress");
    }

    public Color GetPanelColor()
    {
        return _mainPanel.color;
    }

    public void SetPanelProgress(float progress)
    {
        if(progress != _mainPanel.material.GetFloat("_Progress"))
            _mainPanel.material.SetFloat("_Progress", progress);
    }

    public void SwipePanelOn(System.Action callback = null)
    {
        _lerpFraction = 0;
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(SwipePanel(_mainPanel.material, 0, 1, () => {
            //ToolTip.instance.ImmediatelyHide();
            callback?.Invoke(); 
        }));
    }

    public void SwipePanelOff(System.Action callback = null)
    {
        //ToolTip.instance.ImmediatelyHide();
        _lerpFraction = 0;
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(SwipePanel(_mainPanel.material, 1, 2, callback));
    }

    /*
    public void SwipePanelColorChange(Color colorToChangeTo, System.Action callback = null)
    {
        _lerpFraction = 0;
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        _colorSwapPanel.gameObject.SetActive(true);
        _colorSwapPanel.material.SetFloat("_Progress", 0);
        _colorSwapPanel.color = colorToChangeTo;
        currentCoroutine = StartCoroutine(SwipePanel(_colorSwapPanel.material, 0, 1, () => {
            SetPanelColorAfterSwapChange(colorToChangeTo);
            if(callback != null)
            {
                callback.Invoke();
            }
        }));
    }
    */

    public void ChangePanelColor(Color colorToSet)
    {
        _mainPanel.color = colorToSet;
    }

    private void SetPanelColorAfterSwapChange(Color colorToSet)
    {
        _mainPanel.color = colorToSet;
        _colorSwapPanel.material.SetFloat("_Progress", 0);
        _colorSwapPanel.gameObject.SetActive(false);
    }

    private IEnumerator SwipePanel(Material panelMaterial, float from, float to, System.Action callback = null)
    {
        while(_lerpFraction < 1)
        {
            _lerpFraction += Time.deltaTime * _speed;
            _lerpFraction = Mathf.Clamp(_lerpFraction, 0, 1);
            panelMaterial.SetFloat("_Progress", Mathf.Lerp(from, to, _lerpFraction));
            yield return null;
        }
        if(_lerpFraction == 1 && callback != null)
        {
            callback.Invoke();
        }
    }
}
