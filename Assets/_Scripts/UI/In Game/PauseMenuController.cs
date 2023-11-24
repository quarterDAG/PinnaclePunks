using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public Button rematchButton;
    public Button teamSelectButton;
    public Button mainMenuButton;

    [SerializeField] private Button[] buttons;
    [SerializeField] private int selectedIndex = 0;

    private InputManager inputManager;

    private Canvas canvas;

    private float lastInputTime;
    [SerializeField] private float inputDelay = 0.2f; // Delay between inputs
    [SerializeField] private float inputThreshold = 0.5f;

    void Start ()
    {
        GameManager.Instance.SetPauseMenu(this);
        canvas = GetComponent<Canvas>();
        buttons = new Button[] { rematchButton, teamSelectButton, mainMenuButton };
        UpdateButtonSelection();
    }

    void Update ()
    {
        if (inputManager != null && canvas.enabled)
        {
            if (Time.unscaledTime - lastInputTime > inputDelay)
            {
                Vector2 inputVelocity = inputManager.InputVelocity;
                if (Mathf.Abs(inputVelocity.y) > inputThreshold)
                {
                    if (inputVelocity.y > 0)
                    {
                        MoveSelection(-1);
                        lastInputTime = Time.unscaledTime;
                    }
                    else if (inputVelocity.y < 0)
                    {
                        MoveSelection(1);
                        lastInputTime = Time.unscaledTime;
                    }
                }
            }

            if (inputManager.IsJumpPressed)
            {
                PressSelectedButton();
            }
        }
    }




    public void ShowPauseMenu ( InputManager _inputManager )
    {
        MakeButtonsInteractables(true);

        canvas.enabled = true;
        selectedIndex = 0;
        UpdateButtonSelection();
    }

    public void HidePauseMenu ()
    {
        MakeButtonsInteractables(false);

        canvas.enabled = false;
        inputManager = null;
    }

    private void MakeButtonsInteractables ( bool _isInteractable )
    {
        foreach (var button in buttons)
        {
            button.interactable = _isInteractable;
        }
    }

    private void MoveSelection ( int direction )
    {
        int newIndex = selectedIndex + direction;

        newIndex = Mathf.Clamp(newIndex, 0, buttons.Length - 1);

        if (newIndex != selectedIndex)
        {
            selectedIndex = newIndex;
            UpdateButtonSelection();
        }
    }



    private void UpdateButtonSelection ()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == selectedIndex)
            {
                // Set the button as the current selected object in the Event System
                buttons[i].Select();
            }
        }
    }

    private void PressSelectedButton ()
    {
        canvas.enabled = false;
        buttons[selectedIndex].onClick.Invoke();
    }


    public void SetInputManager ( InputManager _inputManager )
    {
        inputManager = _inputManager;
    }

    #region Button Actions

    public void Rematch ()
    {
        Debug.Log("REMATCH!!");
        GameManager.Instance.Rematch();
        HidePauseMenu();
    }

    public void TeamSelect ()
    {
        PlayerManager.Instance.ResetPlayerConfigs();
        PlayerStatsManager.Instance.ClearAllStatsList();
        GameManager.Instance.ClearManagerAssignments();
        GameManager.Instance.StopTime(false);
        SceneManager.LoadScene("TeamSelect");
    }

    public void MainMenu ()
    {
        // Implement when there is a main menu
    }


    #endregion
}
