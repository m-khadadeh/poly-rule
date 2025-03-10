using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    public LevelColorData levelColorData;

    public Canvas uiCanvas;
    
    //public UnityEngine.UI.Image panel;
    //public Transform panelTransform;
    public Transform[] panelPositions; // 0 is left, 1 is center, 2 is right
    public float panelMoveTime;

    public UIClickPlayer clickPlayer;
    

    public List<Transform> worldButtonSets;
    public Transform levelSelectContainer;
    public GameObject worldButtonPrefab;
    public GameObject worldContainerPrefab;
    public GameObject worldCirclePrefab;
    public Transform worldCirclesPivot;
    public float worldCircleSpacing;

    private List<UnityEngine.UI.Image> worldCircles;
    private List<LockableButton> worldCircleButtons;
    private Color disabledCircleColor;

    private List<LevelButtonData> levelButtons;
    public UnityEngine.UI.Button backButton;
    public MeshRenderer bkgRenderer;

    [SerializeField]
    private ButtonLockGroup worldButtons;

    [SerializeField]
    private ButtonLockGroup levelSelectButtons;

    [SerializeField]
    private LockableButton nextWorldButton;

    [SerializeField]
    private LockableButton prevWorldButton;

    private int currentWorld = 0;


    public ColorBlock worldCirclesBlock;
    public ColorBlock worldCirclesSelected;

    // Start is called before the first frame update
    void Start()
    {
        // Get correct camera
        uiCanvas.worldCamera = PanelController.instance.canvasCamera;

        PanelController.instance.ChangePanelColor(levelColorData.foregroundColor);
        PanelController.instance.SetPanelProgress(1);
        
        // Populate Sets
        levelButtons = new List<LevelButtonData>();

        int worldCount = GameManager.instance.levelsPerWorld.Length;
        for(int i = 0; i < worldCount; i++)
        {
            GameObject newContainer = Instantiate(worldContainerPrefab, levelSelectContainer);
            worldButtonSets.Add(newContainer.transform);
            for(int j = 0; j < GameManager.instance.levelsPerWorld[i]; j++)
            {
                LevelButtonData newButton = Instantiate(worldButtonPrefab, newContainer.transform.GetChild(0)).GetComponent<LevelButtonData>();
                UnityEngine.UI.Button unityButton = newButton.GetComponentInChildren<UnityEngine.UI.Button>();
                LevelColorData levelColors = GameManager.instance.worldColors[i];
                newButton.backgroundColor = levelColors.backgroundColor;
                newButton.badIntersectingLineColor = levelColors.badIntersectingLineColor;
                newButton.destructionColor = levelColors.destructionColor;
                newButton.foregroundColor = levelColors.foregroundColor;
                newButton.polygonColor = levelColors.polygonColor;
                int worldNum = i;
                int levelNum = j;
                newButton.levelIndex = levelNum;
                newButton.worldIndex = worldNum;
                unityButton.onClick.AddListener(() =>
                {
                    GoToLevel(newButton);
                    clickPlayer.Click();
                });
                levelButtons.Add(newButton);
            }
        }

        // Things to do after the buttons and sets are populated
        disabledCircleColor = new Color(1, 1, 1, 0.5f);

        PanelController.instance.SwipePanelOff();

        /*
        panel.color = levelColorData.foregroundColor;
        SnapPanel(1);
        StartCoroutine(AnimatePanel(panelPositions[1].position, panelPositions[2].position, panelTransform, panelMoveTime, null));
        */

        worldCircles = new List<UnityEngine.UI.Image>();
        worldCircleButtons = new List<LockableButton>();

        //Vector3 worldCircleStartingPt = worldCirclesPivot.position - ((((worldButtonSets.Length - 1) * worldCircleSpacing) / 2) * Vector3.right);

        for (int i = 0; i < worldButtonSets.Count; i++)
        {
            worldButtonSets[i].gameObject.SetActive(false);
            GameObject newCircle = Instantiate(worldCirclePrefab, worldCirclesPivot);
            worldCircles.Add(newCircle.GetComponent<UnityEngine.UI.Image>());
            var circleButton = newCircle.GetComponent<Button>();
            int worldNum = i;
            circleButton.onClick.AddListener(() =>
            {
                GoToWorld(worldNum);
                clickPlayer.Click();
            });
            worldCircleButtons.Add(circleButton.GetComponent<LockableButton>());
        }

        SetWorld(GameManager.instance.saveData.lastPlayedWorld);

        bkgRenderer.material.color = worldButtonSets[currentWorld].GetChild(0).GetChild(0).GetComponent<LevelButtonData>().backgroundColor;
        //panelController.ChangePanelColor(worldButtonSets[currentWorld].GetChild(0).GetChild(0).GetComponent<LevelButtonData>().foregroundColor);
        //panel.color = worldButtonSets[currentWorld].GetChild(0).GetChild(0).GetComponent<LevelButtonData>().foregroundColor;
    }
    
    public void SetButtonInteractability(bool falseOverride)
    {
        worldButtons.SetInteractability(falseOverride);
        for(int i = 0; i < worldCircleButtons.Count; i++)
        {
          if(currentWorld == i)
          {
            worldCircleButtons[i].GetComponent<Button>().colors = worldCirclesSelected;
          }
          else
          {
            worldCircleButtons[i].GetComponent<Button>().colors = worldCirclesBlock;
          }
          worldCircleButtons[i].SetInteractability(falseOverride && (currentWorld != i));
        }
    }

    public void GoToMainMenu()
    {
        StopAllCoroutines();
        levelSelectButtons.SetInteractability(false);
        backButton.interactable = false;
        SetButtonInteractability(false);
        GameManager.instance.mainMenuSlideRight = false;
        if (PanelController.instance.GetPanelColor() != levelColorData.foregroundColor)
        {
            PanelController.instance.ChangePanelColor(levelColorData.foregroundColor);
        }
        PanelController.instance.SwipePanelOn(() => { GameManager.instance.ChangeScene("MainMenu"); });
    }

    public void GoToLevel(LevelButtonData btnData)
    {
        backButton.interactable = false;
        levelSelectButtons.SetInteractability(false);
        SetButtonInteractability(false);
        levelColorData.backgroundColor = btnData.backgroundColor;
        levelColorData.badIntersectingLineColor = btnData.badIntersectingLineColor;
        levelColorData.foregroundColor = btnData.foregroundColor;
        levelColorData.destructionColor = btnData.destructionColor;
        levelColorData.polygonColor = btnData.polygonColor;

        PanelController.instance.ChangePanelColor(levelColorData.foregroundColor);
        //panel.color = levelColorData.foregroundColor;

        GameManager.instance.currentLevelJson = GameManager.instance.GetLevelAssetFromData(btnData.worldIndex, btnData.levelIndex);
        PanelController.instance.SwipePanelOn(() => { GameManager.instance.ChangeScene("MainLevel"); });
        //StartCoroutine(AnimatePanel(panelPositions[0].position, panelPositions[1].position, panelTransform, panelMoveTime, SceneSwap("MainLevel")));
    }

    /*
    public void SnapPanel(int index)
    {
        panelTransform.position = panelPositions[index].position;
    }
    */
    /*
    IEnumerator SceneSwap(string name)
    {
        SceneManager.LoadScene(name);
        yield return null;
    }
    */

    IEnumerator ChangeBkg(Color startColor, Color endColor, float timeToStop, IEnumerator callbackCoroutine)
    {
        float timer = 0;
        while(timer < timeToStop)
        {
            timer = Mathf.Clamp(timer + Time.deltaTime, 0, timeToStop);
            bkgRenderer.material.color = Color.Lerp(startColor, endColor, timer / timeToStop);
            yield return null;
        }

        if(callbackCoroutine != null)
        {
            StartCoroutine(callbackCoroutine);
        }

        yield return null;
    }

    IEnumerator ChangeColors(int worldNum)
    {
        bkgRenderer.material.color = worldButtonSets[worldNum].GetChild(0).GetChild(0).GetComponent<LevelButtonData>().backgroundColor;
        PanelController.instance.ChangePanelColor(worldButtonSets[worldNum].GetChild(0).GetChild(0).GetComponent<LevelButtonData>().foregroundColor);
        //panel.color = worldButtonSets[worldNum].GetChild(0).GetChild(0).GetComponent<LevelButtonData>().foregroundColor;
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

        if (callbackCoroutine != null)
        {
            StartCoroutine(callbackCoroutine);
        }

        yield return null;
    }

    public void GoToWorld(int worldNum)
    {
        int previousWorld = currentWorld;
        levelSelectButtons.SetInteractability(false);
        SetButtonInteractability(false);
        if (previousWorld < worldNum)
        {
            StartCoroutine(AnimatePanel(panelPositions[1].position, panelPositions[0].position, worldButtonSets[currentWorld], panelMoveTime, WorldChangeCallback(worldButtonSets[currentWorld], panelPositions[1].position, worldNum)));
            StartCoroutine(AnimatePanel(panelPositions[2].position, panelPositions[1].position, worldButtonSets[worldNum], panelMoveTime, null));
            StartCoroutine(ChangeBkg(worldButtonSets[currentWorld].GetChild(0).GetChild(0).GetComponent<LevelButtonData>().backgroundColor, worldButtonSets[worldNum].GetChild(0).GetChild(0).GetComponent<LevelButtonData>().backgroundColor, panelMoveTime, ChangeColors(worldNum)));
            worldButtonSets[worldNum].gameObject.SetActive(true);
        }
        else
        {
            StartCoroutine(AnimatePanel(panelPositions[1].position, panelPositions[2].position, worldButtonSets[currentWorld], panelMoveTime, WorldChangeCallback(worldButtonSets[currentWorld], panelPositions[1].position, worldNum)));
            StartCoroutine(AnimatePanel(panelPositions[0].position, panelPositions[1].position, worldButtonSets[worldNum], panelMoveTime, null));
            StartCoroutine(ChangeBkg(worldButtonSets[currentWorld].GetChild(0).GetChild(0).GetComponent<LevelButtonData>().backgroundColor, worldButtonSets[worldNum].GetChild(0).GetChild(0).GetComponent<LevelButtonData>().backgroundColor, panelMoveTime, ChangeColors(worldNum)));
            worldButtonSets[worldNum].gameObject.SetActive(true);
        }
    }

    public void NextWorld()
    {
        if ((currentWorld + 1) < worldButtonSets.Count) {
            levelSelectButtons.SetInteractability(false);
            SetButtonInteractability(false);
            StartCoroutine(AnimatePanel(panelPositions[1].position, panelPositions[0].position, worldButtonSets[currentWorld], panelMoveTime, WorldChangeCallback(worldButtonSets[currentWorld], panelPositions[1].position, currentWorld + 1)));
            StartCoroutine(AnimatePanel(panelPositions[2].position, panelPositions[1].position, worldButtonSets[currentWorld + 1], panelMoveTime, null));
            StartCoroutine(ChangeBkg(worldButtonSets[currentWorld].GetChild(0).GetChild(0).GetComponent<LevelButtonData>().backgroundColor, worldButtonSets[currentWorld + 1].GetChild(0).GetChild(0).GetComponent<LevelButtonData>().backgroundColor, panelMoveTime, ChangeColors(currentWorld + 1)));
            worldButtonSets[currentWorld + 1].gameObject.SetActive(true);
        }
    }

    public void PrevWorld()
    {
        if ((currentWorld - 1) >= 0)
        {
            levelSelectButtons.SetInteractability(false);
            SetButtonInteractability(false);
            StartCoroutine(AnimatePanel(panelPositions[1].position, panelPositions[2].position, worldButtonSets[currentWorld], panelMoveTime, WorldChangeCallback(worldButtonSets[currentWorld], panelPositions[1].position, currentWorld - 1)));
            StartCoroutine(AnimatePanel(panelPositions[0].position, panelPositions[1].position, worldButtonSets[currentWorld - 1], panelMoveTime, null));
            StartCoroutine(ChangeBkg(worldButtonSets[currentWorld].GetChild(0).GetChild(0).GetComponent<LevelButtonData>().backgroundColor, worldButtonSets[currentWorld - 1].GetChild(0).GetChild(0).GetComponent<LevelButtonData>().backgroundColor, panelMoveTime, ChangeColors(currentWorld - 1)));
            worldButtonSets[currentWorld - 1].gameObject.SetActive(true);
        }
    }

    IEnumerator WorldChangeCallback(Transform changedWorld, Vector3 positionToSnapBackTo, int worldToSetTo)
    {
        levelSelectButtons.SetInteractability(true);
        SetWorld(worldToSetTo);
        changedWorld.gameObject.SetActive(false);
        changedWorld.position = positionToSnapBackTo;
        yield return null;
    }

    void SetWorld(int i)
    {
        worldButtonSets[currentWorld].gameObject.SetActive(false);
        //worldCircles[currentWorld].color = disabledCircleColor;
        currentWorld = i;
        worldButtonSets[currentWorld].gameObject.SetActive(true);
        //worldCircles[currentWorld].color = Color.white;
        SetButtonInteractability(true);
        if (currentWorld == 0)
        {
            prevWorldButton.SetInteractability(false);
        }
        else if (currentWorld == worldButtonSets.Count - 1)
        {
            nextWorldButton.SetInteractability(false);
        }
    }
}
