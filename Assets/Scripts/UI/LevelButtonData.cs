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

    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    string mainText;
    float fontSize;

    private void Start()
    {
        mainText = (worldIndex + 1).ToString() + "-" + (levelIndex + 1).ToString();
        textMesh.text = mainText;
        fontSize = 60;
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
        SetLevelLock(!GameManager.instance.saveData.worldData[worldIndex].unlocked);
        //longestChainValue.color = rulesAppliedValue.color = longestChainImage.color = rulesAppliedImage.color = (GameManager.instance.saveData.worldData[worldIndex].unlocked && interactable) ? Color.white : disabledColor;
    }

    public void SetLevelLock(bool locked)
    {
      if(locked)
      {
        textMesh.text = "Locked";
        textMesh.fontSize = 30;
      }
      else
      {
        textMesh.text = mainText;
        textMesh.fontSize = fontSize;
      }
    }
}
