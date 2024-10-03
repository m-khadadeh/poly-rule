using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelButtonData : TextButton
{
    public Color polygonColor;
    public Color backgroundColor;
    public Color foregroundColor;
    public Color badIntersectingLineColor;
    public Color destructionColor;

    public int worldIndex;
    public int levelIndex;

    public UnityEngine.UI.Button button;

    public UnityEngine.UI.Image longestChainImage;
    public UnityEngine.UI.Image rulesAppliedImage;

    public TMPro.TextMeshProUGUI longestChainValue;
    public TMPro.TextMeshProUGUI rulesAppliedValue;

    private void Start()
    {
        textMesh.text = (worldIndex + 1).ToString() + "-" + (levelIndex + 1).ToString();
        SetInteractability(true);
        if (GameManager.instance.saveData.worldData[worldIndex].levelData[levelIndex].longestChain == -1)
        {
            longestChainValue.text = "0";
        }
        else
        {
            longestChainValue.text = GameManager.instance.saveData.worldData[worldIndex].levelData[levelIndex].longestChain.ToString();
        }

        if(GameManager.instance.saveData.worldData[worldIndex].levelData[levelIndex].rulesApplied == -1)
        {
            rulesAppliedValue.text = "∞";
        }
        else
        {
            rulesAppliedValue.text = GameManager.instance.saveData.worldData[worldIndex].levelData[levelIndex].rulesApplied.ToString();
        }
    }

    public override void SetInteractability(bool interactable)
    {
        base.SetInteractability(GameManager.instance.saveData.worldData[worldIndex].unlocked && interactable);
        longestChainValue.color = rulesAppliedValue.color = longestChainImage.color = rulesAppliedImage.color = (GameManager.instance.saveData.worldData[worldIndex].unlocked && interactable) ? Color.white : disabledColor;
    }
}
