using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public static HUD instance;

    public List<string> congratsMessages;

    //public Transform panelTransform;
    //public Image panel;

    public float screenCoverTime;
    public float maskExitTime;

    public Canvas uiCanvas;

    //public Transform[] panelPositions; // 0 is left, 1 is center, 2 is right

    public GameObject endBrief;
    public GameObject levelHUD;

    public Image[] masks;
    public Transform[] maskExitPositions;

    public TMPro.TextMeshProUGUI longestChain;
    public TMPro.TextMeshProUGUI rulesApplied;
    public TMPro.TextMeshProUGUI congratsMessage;

    public TMPro.TextMeshProUGUI levelName;
    public TMPro.TextMeshProUGUI levelNameEnd;

    public TMPro.TextMeshProUGUI rulesAppliedLevelHUD;
    public TMPro.TextMeshProUGUI longestChainLevelHUD;

    public GameObject endLevelButtons;
    public GameObject nextLevelButton;

    public LoadLevelManager levelManager;

    [SerializeField]
    ButtonLockGroup endHUDLockGroup;

    [SerializeField]
    public ButtonLockGroup levelHUDLockGroup;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Get correct camera
        uiCanvas.worldCamera = PanelController.instance.canvasCamera;

        foreach (var mask in masks)
        {
            mask.color = PanelController.instance.GetPanelColor();
        }
        SetChainHUD(0);
        SetRulesHUD(0);
    }

    public void SetLevelHUDInteractability(bool unlocked)
    {
        levelHUDLockGroup.SetInteractability(unlocked);
    }

    public void SetEndHUDInteractability(bool unlocked)
    {
        endHUDLockGroup.SetInteractability(unlocked);
    }

    public void SetLevelName(string levelNameNew)
    {
        levelNameEnd.text = levelName.text = levelNameNew;
    }

    public IEnumerator ResetEndBrief()
    {
        endBrief.transform.SetParent(this.transform);
        endBrief.transform.position = new Vector3(endBrief.transform.position.x, endBrief.transform.position.y, this.transform.position.z);
        yield return null;
    }

    public void EndLevel()
    {
        endBrief.transform.SetParent(PanelController.instance.transform.parent);
        endBrief.transform.position = new Vector3(endBrief.transform.position.x, endBrief.transform.position.y, PanelController.instance.transform.position.z);
        IEnumerator finalMaskRemove = AnimatePanel(masks[2].transform.position, maskExitPositions[2].position, masks[2].transform, maskExitTime, RevealButtons());
        IEnumerator secondMaskRemove = AnimatePanel(masks[1].transform.position, maskExitPositions[1].position, masks[1].transform, maskExitTime, finalMaskRemove);
        IEnumerator firstMaskRemove = AnimatePanel(masks[0].transform.position, maskExitPositions[0].position, masks[0].transform, maskExitTime, secondMaskRemove);
        IEnumerator revealBrief = RevealEndBrief(0.5f, firstMaskRemove);
        PanelController.instance.SwipePanelOn(() => { HUD.instance.StartCoroutine(RevealEndBrief(0.5f, firstMaskRemove)); });
        //StartCoroutine(AnimatePanel(panelTransform.position, panelPositions[1].position, panelTransform, screenCoverTime, revealBrief));
    }

    public void HideBrief(string nextFunction)
    {
        levelHUD.SetActive(true);
        Vector3 mask2Start = new Vector3(maskExitPositions[2].position.x * -1, maskExitPositions[2].position.y, maskExitPositions[2].position.z);
        Vector3 mask1Start = new Vector3(maskExitPositions[1].position.x * -1, maskExitPositions[1].position.y, maskExitPositions[1].position.z);
        Vector3 mask0Start = new Vector3(maskExitPositions[0].position.x * -1, maskExitPositions[0].position.y, maskExitPositions[0].position.z);
        IEnumerator finalMaskHide = AnimatePanel(mask2Start, (maskExitPositions[2].position + mask2Start)/2, masks[2].transform, maskExitTime / 1.2f, ResetEndBrief());
        IEnumerator secondMaskHide = AnimatePanel(mask1Start, (maskExitPositions[1].position + mask1Start)/2, masks[1].transform, maskExitTime / 1.2f, null);
        IEnumerator firstMaskHide = AnimatePanel(mask0Start, (maskExitPositions[0].position + mask0Start)/2, masks[0].transform, maskExitTime / 1.2f, null);
        StartCoroutine(firstMaskHide);
        StartCoroutine(secondMaskHide);
        StartCoroutine(finalMaskHide);
        StartCoroutine(EndLevelCallback(nextFunction, maskExitTime));
    }

    IEnumerator EndLevelCallback(string nextFunction, float timeToExecute)
    {
        if(timeToExecute > 0)
        {
            yield return new WaitForSeconds(timeToExecute);
        }
        if(nextFunction == "NextLevel")
        {
            levelManager.GoToNextLevel();
        }
        else if (nextFunction == "LevelMenu")
        {
            levelManager.ChangeScene("LevelMenu");
        }
        else
        {
            levelManager.ResetScene();
        }
        yield return null;
    }

    IEnumerator RevealButtons()
    {
        levelHUD.SetActive(false);
        endLevelButtons.SetActive(true);
        //Debug.Log("Next Level: " + GameManager.instance.currentWorld.ToString() + "-" + (GameManager.instance.currentLevel + 1).ToString());
        nextLevelButton.SetActive(GameManager.instance.GetLevelAssetFromData(GameManager.instance.currentWorld, GameManager.instance.currentLevel + 1) != null);
        yield return null;
    }

    /*
    public void MovePanel(int index, IEnumerator callback)
    {
        StartCoroutine(AnimatePanel(panelTransform.position, panelPositions[index].position, panelTransform, screenCoverTime, callback));
    }
    */
    /*
    public void SnapPanel(int index)
    {
        panelTransform.position = panelPositions[index].position;
    }
    */
    IEnumerator RevealEndBrief(float pause, IEnumerator callback)
    {
        yield return new WaitForSeconds(pause);

        endBrief.SetActive(true);

        if (callback != null)
        {
            StartCoroutine(callback);
        }

        yield return null;
    }

    IEnumerator AnimatePanel(Vector3 startPos, Vector3 endPos, Transform transformToMove, float timeToStop, IEnumerator callbackCoroutine)
    {
        float timer = 0;
        while (timer < timeToStop)
        {
            timer = Mathf.Clamp(timer + Time.deltaTime, 0, timeToStop);
            transformToMove.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, timer / timeToStop));
            yield return null;
        }

        if(callbackCoroutine != null)
        {
            StartCoroutine(callbackCoroutine);
        }

        yield return null;
    }

    public void SetLongestChain(int val)
    {
        longestChain.text = "Longest Chain: " + val.ToString();
    }

    public void SetRulesApplied(int val)
    {
        rulesApplied.text = "Rules Applied: " + val.ToString();
    }

    public void RandomizeCongratsMessage()
    {
        if(levelName.text == "6-1")
        {
            congratsMessage.text = "Thanks for playing!!";
        } else
        {
            congratsMessage.text = congratsMessages[Random.Range(0, congratsMessages.Count)];
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public void SetRulesHUD(int num)
    {
        rulesAppliedLevelHUD.text = num.ToString();
    }

    public void SetChainHUD(int num)
    {
        longestChainLevelHUD.text = num.ToString();
    }

}
