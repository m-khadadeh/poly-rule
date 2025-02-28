using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MainMenuManager : MonoBehaviour
{
    public MeshRenderer background;

    public Canvas uiCanvas;

    //public UnityEngine.UI.Image panel;
    //public Transform panelTransform;
    //public Transform[] panelPositions; // 0 is left, 1 is center, 2 is right
    //public float panelMoveTime;
    public LevelColorData currentLevelColors;

    bool inMainMenu;
    public GameObject mainMenuObject;
    public GameObject optionsMenuObject;

    public Renderer vignetteDotsRenderer;

    [SerializeField]
    private ButtonLockGroup mainMenuLockGroup;
    [SerializeField]
    private ButtonLockGroup optionsMenuLockGroup;

    [SerializeField]
    private Button muteButton;
    [SerializeField]
    private Button unmuteButton;

  // Start is called before the first frame update
  void Start()
    {
      uiCanvas.worldCamera = PanelController.instance.canvasCamera;        
      PanelController.instance.SetPanelProgress(1);
      SetColors();
      PanelController.instance.SwipePanelOff();
      inMainMenu = true;
      optionsMenuObject.SetActive(false);
    }

    public void SetColors()
    {
        PanelController.instance.ChangePanelColor(currentLevelColors.foregroundColor);
        DialogBox.instance.ChangeColor(currentLevelColors.foregroundColor);
        background.material.color = currentLevelColors.backgroundColor;
    }

    public void PromptQuit()
    {
        mainMenuLockGroup.SetInteractability(false);
        DialogBox.instance.Prompt(
            "Quit Game?",
            () => {
                QuitGame();
            },
            () => {
                mainMenuLockGroup.SetInteractability(true);
            });
    }

    private void QuitGame()
    {
        mainMenuLockGroup.SetInteractability(false);
        PanelController.instance.SwipePanelOn(() => { Application.Quit(); });
    }

    public void PromptReset()
    {
        optionsMenuLockGroup.SetInteractability(false);
        DialogBox.instance.Prompt("Delete Save Data?",
            () =>
            {
                GameManager.instance.ResetSaveData();
                optionsMenuLockGroup.SetInteractability(true);
            },
            () =>
            {
                optionsMenuLockGroup.SetInteractability(true);
            });
    }

    public void GoToLevelSelect()
    {
        mainMenuLockGroup.SetInteractability(false);
        if (PanelController.instance.GetPanelColor() != currentLevelColors.foregroundColor) {
            PanelController.instance.ChangePanelColor(currentLevelColors.foregroundColor);
        }
        PanelController.instance.SwipePanelOn(() => {
            GameManager.instance.ChangeScene("LevelMenu");
        });
    }

    void ToggleMenu()
    {
        mainMenuObject.SetActive(inMainMenu);
        optionsMenuObject.SetActive(!inMainMenu);

        PanelController.instance.SwipePanelOff();
    }


    public void ToggleOptionsMainMenu()
    {
        inMainMenu = !inMainMenu;
        optionsMenuLockGroup.SetInteractability(!inMainMenu);
        mainMenuLockGroup.SetInteractability(inMainMenu);
        PanelController.instance.SwipePanelOn(() => { ToggleMenu();});
    }

  public void ResetOptionsToDefaults()
  {
    GameManager.instance.settingsManager.ResetToDefaults();
  } 
}
