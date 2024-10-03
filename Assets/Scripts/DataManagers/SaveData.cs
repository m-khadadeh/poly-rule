using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SaveData
{
    [SerializeField]
    public string version;
    [SerializeField]
    public WorldSaveData[] worldData;    
    [SerializeField]
    public int lastPlayedWorld;
}

[System.Serializable]
public struct WorldSaveData
{
    public bool unlocked;
    [SerializeField]
    public LevelSaveData[] levelData;
}

[System.Serializable]
public struct LevelSaveData
{
    public int rulesApplied;
    public int longestChain;
}