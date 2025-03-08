using System.Collections;
using System.Collections.Generic;
using SettingsSystem;
using UnityEngine;

public class LoadLevelManager : MonoBehaviour
{
    private float levelBufferTime = 2;
    private float currentBufferTime;

    private static Dictionary<PolygonController, bool> polygons;
    private static bool levelEnd;

    private static int rulesApplied;
    private static int longestChain;

    public static Color laserCrossColor;
    public static Color polygonDestroyColor;

    public GameObject hexPrefab;
    public GameObject triPrefab;
    public GameObject upsideDownTriPrefab;

    public LaserColorData[] laserColors;
    public RuleConnector[] ruleConnectors;

    public MeshRenderer backgroundMeshRenderer;
    public UnityEngine.UI.Image foregroundPanel;
    public FadeInText tutorialText;
    public IntSetting tutorialsEnabled;

    private static Queue<PolygonController> polysQueued;

    [HideInInspector]
    public Color polygonColor;
    [HideInInspector]
    public Color backgroundColor;
    [HideInInspector]
    public Color foregroundColor;
    [HideInInspector]
    public Color badIntersectingLineColor;
    [HideInInspector]
    public Color destructionColor;

    public LevelColorData colorData;

    private static Queue<PolygonController> currentPolygonApplying;
    private static float timeSinceRule;
    private static bool applyingRule;

    bool allPolysSet;

    private static int destroyedPolysSinceLastRuleApp;

    private static PolygonController lastSelectedPolygon;

    public static List<PolygonController> destroyList;

    private static bool doneCheckingForChains;

    public GameObject plusOneParticlesObject;
    static GameObject plusOneParticles;
    public GameObject queuedParticlesObject;
    static GameObject queuedParticles;
    static Color plusOneColor;

    static bool resetting;

    bool allPolysAdded;

    void LevelStart()
    {
        allPolysAdded = false;
        polygonColor = colorData.polygonColor;
        backgroundColor = colorData.backgroundColor;
        foregroundColor = colorData.foregroundColor;
        badIntersectingLineColor = colorData.badIntersectingLineColor;
        destructionColor = colorData.destructionColor;
        resetting = false;
        plusOneColor = polygonColor;

        currentBufferTime = levelBufferTime;

        if (polygons == null)
        {
            polygons = new Dictionary<PolygonController, bool>();
        }
        laserCrossColor = badIntersectingLineColor;
        polygonDestroyColor = destructionColor;
        if(backgroundMeshRenderer != null)
            backgroundMeshRenderer.material.SetColor("_BaseColor", backgroundColor);
        if(foregroundPanel != null)
            foregroundPanel.color = foregroundColor;
        allPolysSet = false;

        currentPolygonApplying = new Queue<PolygonController>();
        polysQueued = new Queue<PolygonController>();
        applyingRule = false;

        timeSinceRule = 0;

        levelEnd = false;

        destroyedPolysSinceLastRuleApp = 0;
        destroyList = new List<PolygonController>();

        doneCheckingForChains = true;

        SetUpFromJson();

        allPolysAdded = true;
    }

    [System.Serializable]
    class LevelData
    {
		public string tutorialText;
        public float[] sizeData;
        public PolyData[] levelObjects;
    }

    [System.Serializable]
    class PolyData
    {
        public int index;
        public int type;
        public int[] position;
        public string rule;
        public int[] laserData;
        public int[] ruleConnectors;
    }

    private void SetUpFromJson()
    {
        //Debug.Log(GameManager.instance.currentLevelJson.text);
        LevelData polygonDataArr = JsonUtility.FromJson<LevelData>(GameManager.instance.currentLevelJson.text);
        if(tutorialsEnabled.Value != 0)
        {
          tutorialText.SetText(polygonDataArr.tutorialText);
          tutorialText.FadeIn();
        }
        else
        {
          tutorialText.SetAlpha(0);
        }
        if(TriangularCoordinates.instance == null)
        {
            TriangularCoordinates.instance = FindObjectOfType<TriangularCoordinates>();
        }
        TriangularCoordinates.instance.section = (int)polygonDataArr.sizeData[0];
        TriangularCoordinates.instance.radius = (float)polygonDataArr.sizeData[1];
        PolyData[] polygonData = polygonDataArr.levelObjects;
        foreach(PolyData polygon in polygonData)
        {
            //Debug.Log(JsonUtility.ToJson(polygon));
            PolygonController newPoly = null;
            switch (polygon.type)
            {
                case 1:
                    // Hexagon
                    newPoly = GameObject.Instantiate(hexPrefab).GetComponent<PolygonController>();
                    break;
                case 2:
                    // Triangle
                    newPoly = GameObject.Instantiate(triPrefab).GetComponent<PolygonController>();
                    break;
                case 3:
                    // Upside down Triangle
                    newPoly = GameObject.Instantiate(upsideDownTriPrefab).GetComponent<PolygonController>();
                    break;
            }

            newPoly.transform.parent = this.transform;
            //Debug.Log(new Vector2(polygon.position[0], polygon.position[1]));
            newPoly.triangularPosition = new Vector2(polygon.position[0], polygon.position[1]);
            switch(polygon.rule)
            {
                case "none":
                    newPoly.rule = PolygonController.Rule.None;
                    break;
                case "rotateCCW":
                    newPoly.rule = PolygonController.Rule.RotateCCW;
                    break;
                case "rotateCW":
                    newPoly.rule = PolygonController.Rule.RotateCW;
                    break;
                case "flipVertical":
                    newPoly.rule = PolygonController.Rule.FlipVertical;
                    break;
                case "flipHorizontal":
                    newPoly.rule = PolygonController.Rule.FlipHorizontal;
                    break;
            }

            for (int i = 0; i < polygon.laserData.Length; i++)
            {
                if (polygon.laserData[i] != 0)
                {
                    PolygonController.LaserInfo newLaserInfo;
                    newLaserInfo.colorData = laserColors[polygon.laserData[i] - 1]; // values of laser colors start @ 1 bc 0 is no laser
                    newLaserInfo.pointIndex = i;
                    newPoly.laserInfo.Add(newLaserInfo);
                } 
            }

            foreach(int connectorID in polygon.ruleConnectors)
            {
                newPoly.ruleConnectors.Add(ruleConnectors[connectorID]);
            }

            newPoly.gameObject.name = "Poly_" + polygon.index;
            AddPolygon(newPoly);
            newPoly.InitPolygon();
            newPoly.polyRenderer.SetColor(polygonColor);
            //Debug.Log("Initialized polygon " + newPoly.gameObject.name);
        }
    }

    /*
    IEnumerator SnapPanelBack(int index)
    {
        HUD.instance.SnapPanel(index);
        yield return null;
    }
    */
    private void Start()
    {
        PanelController.instance.ChangePanelColor(colorData.foregroundColor);
        PanelController.instance.SetPanelProgress(1);
        LevelStart();

        PanelController.instance.SwipePanelOff();

        //HUD.instance.SnapPanel(1);
        //HUD.instance.MovePanel(2, SnapPanelBack(0));

        plusOneParticles = plusOneParticlesObject;
        queuedParticles = queuedParticlesObject;

        string sceneName = GameManager.instance.currentLevelJson.name.Substring(3); // level names should start with "lev"
        //Debug.Log(sceneName);
        int dashIndex = sceneName.IndexOf('-');
        GameManager.instance.currentWorld = int.Parse(sceneName.Substring(0, dashIndex)) - 1;
        GameManager.instance.currentLevel = int.Parse(sceneName.Substring(dashIndex + 1)) - 1;
        GameManager.instance.saveData.lastPlayedWorld = GameManager.instance.currentWorld;

        HUD.instance.SetLevelName(sceneName);

    }

    private bool AllPolygonsReady()
    {
        foreach(var polygon in polygons)
        {
            if(!polygon.Value)
            {
                return false;
            }
        }
        //Debug.Log("All Ready");
        return true;
    }

    public void ClickSound()
    {
        AudioManager.instance.PlayClick();
    }

    private void Update()
    {
        if(!allPolysSet && allPolysAdded && AllPolygonsReady() && levelBufferTime <= 0)
        {
            foreach (var polygon in polygons)
            {
                polygon.Key.UpdateLaserPhysicalCollisions();
            }
            foreach (var polygon in polygons)
            {
                //Debug.Log("Shooting Lasers from " + polygon.Key.gameObject);
                polygon.Key.ShootStartingLaser();
            }
            allPolysSet = true;
        }

        if (levelBufferTime > 0)
        {
            levelBufferTime--;
        }

        if (applyingRule)
        {
            timeSinceRule += Time.deltaTime;
        }

        if (LoadLevelManager.AllowNext() && polysQueued.Count > 0)
        {
            PolygonController nextPoly = polysQueued.Dequeue();
            if (nextPoly != null)
            {
                nextPoly.wasSelectedToApply = true;
                lastSelectedPolygon = nextPoly;

                if(tutorialText.CurrentAlpha > 0.9f)
                {
                  tutorialText.FadeOut();
                }

                //rulesApplied++;
                AudioManager.instance.PlayClick();
                // This is where rule-connected polys would be enqueued
                Queue<PolygonController> toApply = nextPoly.GetRuleConnectedPolygons();
                while (toApply.Count > 0)
                {
                    if (!toApply.Peek().isBeingDestroyed)
                    {
                        toApply.Peek().ApplyRule();
                        currentPolygonApplying.Enqueue(toApply.Dequeue());
                    }
                    else
                    {
                        toApply.Dequeue();
                    }
                }
                applyingRule = true;
                timeSinceRule = 0;
            }
        }
    }

    public static void EnsureNoPolygonsHovering()
    {
        if (polygons == null)
        {
            polygons = new Dictionary<PolygonController, bool>();
        }
        foreach (var polygon in polygons)
        {
            polygon.Key.EnsureNotHovering();
        }
    }

    public static void AddPolygon(PolygonController polygon)
    {
        //Debug.Log(polygon.gameObject.name + " is being added to the dictionary.");
        if (polygons == null)
        {
            polygons = new Dictionary<PolygonController, bool>();
        }
        polygons.Add(polygon, false);
    }

    public static void RemovePolygon(PolygonController polygon)
    {
        polygons.Remove(polygon);
        destroyList.Remove(polygon);

        destroyedPolysSinceLastRuleApp++;
        if (destroyedPolysSinceLastRuleApp > longestChain)
        {
            longestChain = destroyedPolysSinceLastRuleApp;
            HUD.instance.SetChainHUD(longestChain);
        }

        CheckLevelEnd();
    }

    public static void AddPolygonToDestroyList(PolygonController polygon)
    {
        if(destroyList.Count == 0 && doneCheckingForChains)
        {
            // This is the first polygon to be destroyed. Reset the counter
            destroyedPolysSinceLastRuleApp = 0;
        }
        if (!destroyList.Contains(polygon))
        {
            destroyList.Add(polygon);
        }
        doneCheckingForChains = false;
    }

    public static void PolygonSetupComplete(PolygonController polygon)
    {
        polygons[polygon] = true;
    }

    public static void QueueRuleApplication(PolygonController polygon)
    {
        if (polysQueued.Count < 1)
        {
            polysQueued.Enqueue(polygon);
            // Spawn queued particles
            GameObject newQueued = Instantiate(queuedParticles, polygon.gameObject.transform.position, Quaternion.identity);
            var pSystemMat = newQueued.GetComponent<ParticleSystemRenderer>().material;
            pSystemMat.renderQueue = 4500;
            pSystemMat.color = Color.white;
            newQueued.GetComponent<ParticleSystemRenderer>().material = pSystemMat;
            Destroy(newQueued, 0.5f);
        }
    }

    public static void RuleApplicationComplete(PolygonController completedPoly)
    {
        if (completedPoly == lastSelectedPolygon)
        {
            // Spawn +1 particles
            GameObject newPlusOne = Instantiate(plusOneParticles, completedPoly.gameObject.transform.position, Quaternion.identity);
            var pSystemMat = newPlusOne.GetComponent<ParticleSystemRenderer>().material;
            pSystemMat.renderQueue = 4500;
            pSystemMat.color = plusOneColor;
            newPlusOne.GetComponent<ParticleSystemRenderer>().material = pSystemMat;
            Destroy(newPlusOne, 0.5f);

            rulesApplied++;
            HUD.instance.SetRulesHUD(rulesApplied);
        }

        foreach (var polygon in polygons)
        {
            polygon.Key.UpdateLaserPhysicalCollisions(true);
            polygon.Key.IsFullyConnected();
        }
        while (currentPolygonApplying.Count > 0)
        {
            currentPolygonApplying.Dequeue().OnRuleApplied();
        }
        /*
        if (bufferedPolygon != null)
        {
            if (polygons.ContainsKey(bufferedPolygon))
            {
                QueueRuleApplication(bufferedPolygon);
            }
            bufferedPolygon = null;
        }
        */
        applyingRule = false;
    }

    private static bool AllowNext()
    {
        return (!applyingRule && (destroyList.Count == 0) && doneCheckingForChains);
    }

    public static void CheckPolygons()
    {
        foreach(var polygon in polygons)
        {
            if(polygon.Key.CheckForChainAndDestroy())
            {
                return;
            }
        }
        doneCheckingForChains = true;
    }

    public static void CheckLevelEnd()
    {
        if(polygons.Count == 0 && !levelEnd && !resetting)
        {
            // End level code
            levelEnd = true;
            HUD.instance.SetLongestChain(longestChain);
            HUD.instance.SetRulesApplied(rulesApplied);
            HUD.instance.RandomizeCongratsMessage();
            HUD.instance.EndLevel();

            // Set Score Data
            GameManager.instance.UpdateCurrentLevelScore(rulesApplied, longestChain);
            //GameManager.instance.currentLevel = -1;
            //GameManager.instance.currentWorld = -1;
        }
    }

    void SceneSwap(string name)
    {
        GameManager.instance.SaveData();
        GameManager.instance.ChangeScene(name);
        //yield return null;
    }

    public void PromptExitLevel()
    {
        HUD.instance.levelHUDLockGroup.SetInteractability(false);
        DialogBox.instance.Prompt("Exit Level?",
            () =>
            {
                ChangeScene("LevelMenu");
            },
            () =>
            {
                HUD.instance.levelHUDLockGroup.SetInteractability(true);
            });
    }

    public void PromptReset()
    {
        HUD.instance.levelHUDLockGroup.SetInteractability(false);
        DialogBox.instance.Prompt("Reset Level?",
            () =>
            {
                ResetScene();
            },
            () =>
            {
                HUD.instance.levelHUDLockGroup.SetInteractability(true);
            });
    }

    public void ResetScene()
    {
        ChangeScene("MainLevel");
    }

    public void ChangeScene(string sceneName)
    {
        // Reset Static Vars
        allPolysAdded = false;
        polygons = new Dictionary<PolygonController, bool>();
        currentPolygonApplying = new Queue<PolygonController>();
        destroyList = new List<PolygonController>();
        rulesApplied = 0;
        longestChain = 0;
        laserCrossColor = badIntersectingLineColor;
        polygonDestroyColor = destructionColor;
        if (backgroundMeshRenderer != null)
        {
            backgroundMeshRenderer.material.SetColor("_BaseColor", backgroundColor);
        }
        if (foregroundPanel != null)
            foregroundPanel.color = foregroundColor;
        allPolysSet = false;
        applyingRule = false;
        timeSinceRule = 0;
        levelEnd = false;
        destroyedPolysSinceLastRuleApp = 0;
        doneCheckingForChains = true;
        resetting = true;

        if (PanelController.instance.GetPanelProgress() < 0.1f || PanelController.instance.GetPanelProgress() > 1.9f)
        {
            PanelController.instance.SwipePanelOn(() => { SceneSwap(sceneName); });
        }
        else
        {
            SceneSwap(sceneName);
        }

        //HUD.instance.MovePanel(1, SceneSwap(sceneName));
    }

    public void GoToNextLevel()
    {
        GameManager.instance.currentLevelJson = GameManager.instance.GetLevelAssetFromData(GameManager.instance.currentWorld, GameManager.instance.currentLevel + 1);
        ResetScene();
    }

    public static bool IsLevelEnd()
    {
        return levelEnd;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(backgroundMeshRenderer != null)
            backgroundMeshRenderer.sharedMaterial.SetColor("_BaseColor", backgroundColor);
        if(foregroundPanel != null)
            foregroundPanel.color = foregroundColor;
    }
#endif
}