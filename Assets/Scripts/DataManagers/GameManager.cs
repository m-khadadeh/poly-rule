using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [HideInInspector]
    public bool mainMenuSlideRight = true;
    public SaveData saveData;

    public int[] levelsPerWorld; // Used to know how many levels are in each world
    public List<LevelColorData> worldColors;

    public string saveFile;
    string saveFilePath;

    public int currentLevel;
    public int currentWorld;

    public List<TextAsset> levelJsons;

    public TextAsset currentLevelJson;

    public float bufferTime;

    public string gameVersion;

    public UnityEngine.Events.UnityEvent onDataLoad;
    public LevelColorData currentLevelColors;

    public List<ButtonLockGroup> lockGroups;

    // Start is called before the first frame update
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }    
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Calculate Levels per world
        levelJsons.Sort(delegate (TextAsset a, TextAsset b) {
            return string.Compare(a.name, b.name);
        });
        int currentWorld = 1;
        List<int> levelsPerWorldList = new List<int>();
        levelsPerWorldList.Add(0);
        for (int i = 0; i < levelJsons.Count; i++)
        {
            //Debug.Log(levelJsons[i].name + " starts with " + "lvl" + currentWorld.ToString() + "?" + levelJsons[i].name.StartsWith("lvl" + currentWorld.ToString()));
            while (!levelJsons[i].name.StartsWith("lev" + currentWorld.ToString()))
            {
                currentWorld++;
                levelsPerWorldList.Add(0);
            }
            levelsPerWorldList[levelsPerWorldList.Count - 1]++;
        }
        levelsPerWorld = levelsPerWorldList.ToArray();

        // Load Data
        saveFilePath = Application.persistentDataPath + "/" + saveFile;
        LoadData();
        DialogBox.instance.TurnOff();
    }

    public void UpdateCurrentLevelScore(int rulesApplied, int longestChain)
    {
        if(saveData.worldData[currentWorld].levelData[currentLevel].rulesApplied == -1 || saveData.worldData[currentWorld].levelData[currentLevel].rulesApplied > rulesApplied)
        {
            saveData.worldData[currentWorld].levelData[currentLevel].rulesApplied = rulesApplied;
        }

        if(saveData.worldData[currentWorld].levelData[currentLevel].longestChain == -1 || saveData.worldData[currentWorld].levelData[currentLevel].longestChain < longestChain)
        {
            saveData.worldData[currentWorld].levelData[currentLevel].longestChain = longestChain;
        }

        if(CheckWorldCompletion(currentWorld))
        {
            // Unlock next world
            if(currentWorld + 1 < saveData.worldData.Length)
            {
                saveData.worldData[currentWorld + 1].unlocked = true;
            }
        }

        SaveData();
    }

    public void SaveData()
    {
        string saveJson = JsonUtility.ToJson(saveData);
        File.WriteAllText(saveFilePath, saveJson);
    }

    public void LoadData()
    {
        if(File.Exists(saveFilePath))
        {
            string saveJson = File.ReadAllText(saveFilePath);
            try
            {
                saveData = JsonUtility.FromJson<SaveData>(saveJson);
                if(saveData.version != gameVersion)
                {
                    Debug.Log("Save Data incorrect version. Updating.");
                    UpdateSaveData();
                }
                if (saveData.worldData.Length != levelsPerWorld.Length)
                {
                    Debug.Log("Save Data has incorrect world amount");
                    UpdateSaveData();
                }
                else
                {
                    for(int i = 0; i < saveData.worldData.Length; i++)
                    {
                        if(saveData.worldData[i].levelData.Length != levelsPerWorld[i])
                        {
                            Debug.Log("Save Data has incorrect level amount.");
                            UpdateSaveData();
                            break;
                        }
                    }
                }
            }
            catch
            {
                Debug.Log("Save Data not formatted correctly. Resetting.");
                ResetSaveData(false);
            }
        }
        else
        {
            // Reset Data and Save new file;
            ResetSaveData(false);
        }

        currentLevelColors.backgroundColor = worldColors[saveData.lastPlayedWorld].backgroundColor;
        currentLevelColors.foregroundColor = worldColors[saveData.lastPlayedWorld].foregroundColor;
        currentLevelColors.badIntersectingLineColor = worldColors[saveData.lastPlayedWorld].badIntersectingLineColor;
        currentLevelColors.destructionColor = worldColors[saveData.lastPlayedWorld].destructionColor;
        currentLevelColors.polygonColor = worldColors[saveData.lastPlayedWorld].polygonColor;
        onDataLoad.Invoke();
    }

    public void UpdateSaveData()
    {
        SaveData oldData = saveData;
        ResetSaveData(false);
        for(int i = 0; i < oldData.worldData.Length; i++)
        {
            for(int j = 0; j < oldData.worldData[i].levelData.Length; j++)
            {
                saveData.worldData[i].levelData[j] = oldData.worldData[i].levelData[j];
            }
            if(i == 0)
            {
                saveData.worldData[i].unlocked = true;
            }
            else
            {
                saveData.worldData[i].unlocked = CheckWorldCompletion(i - 1);
            }
        }
        saveData.version = gameVersion;
    }

    public void ResetSaveData(bool loadData = true)
    {
        saveData = new SaveData();
        saveData.worldData = new WorldSaveData[levelsPerWorld.Length];
        saveData.lastPlayedWorld = 0;
        for(int i = 0; i < levelsPerWorld.Length; i++)
        {
            saveData.worldData[i].unlocked = false;
            saveData.worldData[i].levelData = new LevelSaveData[levelsPerWorld[i]];
            for(int j = 0; j < levelsPerWorld[i]; j++)
            {
                saveData.worldData[i].levelData[j].longestChain = -1;
                saveData.worldData[i].levelData[j].rulesApplied = -1;
            }
        }
        saveData.worldData[0].unlocked = true;
        saveData.version = gameVersion;

        SaveData();
        if(loadData)
            LoadData();
    }

    public bool CheckWorldCompletion(int worldIndex)
    {
        for(int i = 0; i < saveData.worldData[worldIndex].levelData.Length; i++)
        {
            if (saveData.worldData[worldIndex].levelData[i].longestChain == -1)
                return false;
        }
        return true;
    }

    public TextAsset GetLevelAssetFromData(int worldIndex, int levelIndex)
    {
        return levelJsons.Find(e => e.name == ("lev" + (worldIndex + 1).ToString() + "-" + (levelIndex + 1).ToString()));
    }

    private void ClearLockGroups()
    {
        foreach(var lockGroup in lockGroups)
        {
            lockGroup.ClearList();
        }
    }

    public void ChangeScene(string sceneName)
    {
        ClearLockGroups();
        SceneManager.LoadScene(sceneName);
    }
}
