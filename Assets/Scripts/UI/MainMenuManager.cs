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
        DialogBox.instance.Prompt("Reset Save Data?",
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

        if(!inMainMenu)
        {
            muteButton.gameObject.SetActive(!AudioManager.instance.MuteVal);
            unmuteButton.gameObject.SetActive(AudioManager.instance.MuteVal);
        }

        PanelController.instance.SwipePanelOff();
    }

    public void Mute()
    {
        AudioManager.instance.Mute();
        muteButton.gameObject.SetActive(false);
        unmuteButton.gameObject.SetActive(true);
    }

    public void Unmute()
    {
        AudioManager.instance.Mute();
        muteButton.gameObject.SetActive(true);
        unmuteButton.gameObject.SetActive(false);
    }

    public void ToggleOptionsMainMenu()
    {
        inMainMenu = !inMainMenu;
        optionsMenuLockGroup.SetInteractability(!inMainMenu);
        mainMenuLockGroup.SetInteractability(inMainMenu);
        PanelController.instance.SwipePanelOn(() => { ToggleMenu();});
    }
}
