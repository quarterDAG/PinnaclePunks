using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class InputIcon : MonoBehaviour
{
    private TeamSelectionController teamSelectionController;
    [SerializeField] private PlayerConfig playerConfig;
    private Rigidbody2D rb;
    [SerializeField] private float speed = 1000f;

    [SerializeField] private PlayerConfigData playerConfigData;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InputManager inputManager;

    private InputDevice inputDevice;

    [SerializeField] private Image readyIcon;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody2D>();
        teamSelectionController = FindObjectOfType<TeamSelectionController>();
        transform.SetParent(teamSelectionController.transform, false);
        inputDevice = playerInput.devices[0];
        AddPlayerConfig();
    }


    private void Update ()
    {
        if (playerConfig.playerState != PlayerConfigData.PlayerState.Ready)
            rb.velocity = inputManager.InputVelocity * speed;

        if (inputManager.IsJumpPressed && playerConfig.team != PlayerConfigData.Team.Spectator)
            SetPlayerStateReady();

        if (inputManager.IsRopeShootPressed)
            SetPlayerStateChoosingTeam();

    }

    // Call this method when a player joins the game.
    public void AddPlayerConfig ()
    {
        // Get the control scheme from the input manager, typically based on the last input received.
        PlayerConfigData.ControlScheme controlScheme = inputManager.GetCurrentControlScheme(inputDevice);
        int _playerIndex = PlayerManager.Instance.GetUniquePlayerIndex();

        Color _playerColor = GetUniquePlayerColor(_playerIndex);
        string _playerName = GetPlayerNameByColor(_playerIndex);

        //Debug.Log("Player Index: " + _playerIndex + "Player Name: " + _playerName);

        // Create the new PlayerConfig with the unique color and control scheme.
        PlayerConfig newPlayerConfig = new PlayerConfig
        {
            playerIndex = _playerIndex,
            playerName = _playerName,
            playerColor = _playerColor,
            team = PlayerConfigData.Team.Spectator,
            controlScheme = controlScheme,
            inputDevice = inputDevice
        };


        GetComponent<Image>().color = _playerColor;

        SetNameAndSendPlayerStats(_playerName, _playerIndex);

        // Add the new PlayerConfig to the playerConfigs list.
        PlayerManager.Instance.playerConfigs.Add(newPlayerConfig);

        playerConfig = newPlayerConfig;

        SetPlayerStateChoosingTeam();
    }

    private void SetNameAndSendPlayerStats ( string _playerName, int _playerIndex )
    {
        PlayerStats _playerStats = new PlayerStats();
        _playerStats.playerName = _playerName;
        PlayerStatsManager.Instance.UpdatePlayerStats(_playerStats, _playerIndex);
    }

    private Color GetUniquePlayerColor ( int playerIndex )
    {
        // Ensure that the player index is within the range of available colors.
        if (playerConfigData.playerColors.Count > 0)
        {
            // Use modulo to loop back to the start of the color list if there are more players than colors.
            return playerConfigData.playerColors[playerIndex % playerConfigData.playerColors.Count];
        }
        else
            return Color.white;
    }

    private string GetPlayerNameByColor ( int playerIndex )
    {
        switch (playerIndex)
        {
            case 0:
                return "Red";
            case 1:
                return "Blue";
            case 2:
                return "Green";
            case 3:
                return "Yellow";
            default:
                return "Unknown";

        }
    }

    private void SetPlayerStateReady ()
    {
        if (playerConfig.playerState != PlayerConfigData.PlayerState.Ready)
        {
            playerConfig.playerState = PlayerConfigData.PlayerState.Ready;
            rb.velocity = Vector2.zero;
            teamSelectionController.SetPlayerTeam(playerConfig.playerIndex, playerConfig.team, transform);
            teamSelectionController.SetPlayerReady(playerConfig.playerIndex);
            readyIcon.enabled = true;

            // Handle any other functionality that should occur when the player is ready
        }
    }

    private void SetPlayerStateChoosingTeam ()
    {
        if (playerConfig.playerState != PlayerConfigData.PlayerState.Playing)
        {
            playerConfig.playerState = PlayerConfigData.PlayerState.ChoosingTeam;
            teamSelectionController.SetPlayerChoosingTeam(playerConfig.playerIndex);
            readyIcon.enabled = false;

        }
    }


    #region Colliders

    private void OnTriggerEnter2D ( Collider2D other )
    {
        // Attempt to join TeamA.
        if (other.CompareTag("TeamA"))
        {
            if (teamSelectionController.CanJoinTeam(PlayerConfigData.Team.TeamA))
            {
                playerConfig.team = PlayerConfigData.Team.TeamA;
            }
        }
        // Attempt to join TeamB.
        else if (other.CompareTag("TeamB"))
        {
            teamSelectionController.CanJoinTeam(PlayerConfigData.Team.TeamB);
            if (teamSelectionController.CanJoinTeam(PlayerConfigData.Team.TeamB))
            {
                playerConfig.team = PlayerConfigData.Team.TeamB;
            }
        }
    }

    private void OnTriggerExit2D ( Collider2D other )
    {
        if ((other.CompareTag("TeamA") && playerConfig.team == PlayerConfigData.Team.TeamA) ||
            (other.CompareTag("TeamB") && playerConfig.team == PlayerConfigData.Team.TeamB))
        {
            playerConfig.team = PlayerConfigData.Team.Spectator;
        }
    }

    #endregion
}
