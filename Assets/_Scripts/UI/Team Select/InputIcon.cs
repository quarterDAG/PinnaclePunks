using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static PlayerConfigData;

[RequireComponent(typeof(Collider2D))]
public class InputIcon : MonoBehaviour
{
    private TeamSelectionController teamSelectionController;
    private MapSelectController mapSelectController;
    [SerializeField] private PlayerConfig playerConfig;
    private Rigidbody2D rb;
    [SerializeField] private float speed = 1000f;

    [SerializeField] private PlayerConfigData playerConfigData;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InputManager inputManager;

    private InputDevice inputDevice;

    [SerializeField] private Image readyIcon;
    private int selectedMap;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody2D>();
        teamSelectionController = FindObjectOfType<TeamSelectionController>();
        mapSelectController = FindObjectOfType<MapSelectController>();

        if (teamSelectionController != null)
        {
            transform.SetParent(teamSelectionController.transform, false);

            inputDevice = playerInput.devices[0];
            AddPlayerConfig();

        }
    }


    private void Update ()
    {
        if (playerConfig.playerState != PlayerState.Ready)
            rb.velocity = inputManager.InputVelocity * speed;

        if (teamSelectionController != null)
        {
            if (inputManager.IsJumpPressed && playerConfig.team != Team.Spectator)
            {
                SetPlayerStateReady();
                teamSelectionController.SetPlayerTeam(playerConfig.playerIndex, playerConfig.team, transform);
                teamSelectionController.SetPlayerReady(playerConfig.playerIndex);
            }

            if (inputManager.IsSecondaryPressed)
                SetPlayerStateChoosingTeam();
        }

        if (mapSelectController != null)
        {
            if (inputManager.IsJumpPressed && selectedMap >= 0)
            {
                mapSelectController.VoteForMap(selectedMap, playerConfig.playerIndex, this);
                SetPlayerStateReady();
            }

            if (inputManager.IsSecondaryPressed)
                SetPlayerStateChoosingMap();
        }

    }

    // Call this method when a player joins the game.
    public void AddPlayerConfig ()
    {
        // Get the control scheme from the input manager, typically based on the last input received.
        int _playerIndex = PlayerManager.Instance.GetUniquePlayerIndex();

        Color _playerColor = GetUniquePlayerColor(_playerIndex);
        string _playerName = GetPlayerNameByColor(_playerIndex);


        // Create the new PlayerConfig with the unique color and control scheme.
        PlayerConfig newPlayerConfig = new PlayerConfig
        {
            playerIndex = _playerIndex,
            playerName = _playerName,
            playerColor = _playerColor,
            team = Team.Spectator,
            controlScheme = ControlScheme.Gamepad,
            inputDevice = inputDevice
        };

        SetIconColor(_playerColor);

        SetNameAndSendPlayerStats(_playerName, _playerIndex);

        // Add the new PlayerConfig to the playerConfigs list.
        PlayerManager.Instance.playerConfigs.Add(newPlayerConfig);

        playerConfig = newPlayerConfig;

        if (teamSelectionController != null)
            SetPlayerStateChoosingTeam();
    }

    public void SetIconColor ( Color _playerColor )
    {
        GetComponent<Image>().color = _playerColor;
    }

    private void SetNameAndSendPlayerStats ( string _playerName, int _playerIndex )
    {
        PlayerStats _playerStats = ScriptableObject.CreateInstance<PlayerStats>();
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
        if (playerConfig.playerState != PlayerState.Ready)
        {
            playerConfig.playerState = PlayerState.Ready;
            rb.velocity = Vector2.zero;
            readyIcon.enabled = true;

            // Handle any other functionality that should occur when the player is ready
        }
    }

    private void SetPlayerStateChoosingTeam ()
    {
        if (playerConfig.playerState != PlayerState.Playing)
        {
            playerConfig.playerState = PlayerState.ChoosingTeam;
            teamSelectionController.SetPlayerChoosingTeam(playerConfig.playerIndex);
            readyIcon.enabled = false;

        }
    }

    public void SetPlayerStateChoosingMap ()
    {
        if (playerConfig.playerState != PlayerState.ChoosingMap)
        {
            playerConfig.playerState = PlayerState.ChoosingMap;
            PlayerManager.Instance.SetPlayerState(playerConfig.playerIndex, PlayerState.ChoosingMap);
            readyIcon.enabled = false;

        }
    }

    public void SetSelectedMap ( int _selectedMap )
    {

        selectedMap = _selectedMap;
    }

    public void SetIconPosition ( Vector3 _position )
    {
        transform.position = _position;
    }

    public void SetIconConfig ( PlayerConfig _playerConfig )
    {
        playerConfig = _playerConfig;
    }

    public void SetIconSpeed ( float _speed )
    {
        speed = _speed;
    }


    public int GetSelectedMap ()
    {
        return selectedMap;
    }

    #region Colliders

    private void OnTriggerEnter2D ( Collider2D other )
    {
        if (teamSelectionController != null)
        {
            // Attempt to join TeamA.
            if (other.CompareTag("TeamA"))
            {
                if (teamSelectionController.CanJoinTeam(Team.TeamA))
                {
                    playerConfig.team = Team.TeamA;
                }
            }
            // Attempt to join TeamB.
            else if (other.CompareTag("TeamB"))
            {
                teamSelectionController.CanJoinTeam(Team.TeamB);
                if (teamSelectionController.CanJoinTeam(Team.TeamB))
                {
                    playerConfig.team = Team.TeamB;
                }
            }
        }


    }

    private void OnTriggerExit2D ( Collider2D other )
    {
        if (teamSelectionController != null)
        {
            if ((other.CompareTag("TeamA") && playerConfig.team == Team.TeamA) ||
                (other.CompareTag("TeamB") && playerConfig.team == Team.TeamB))
            {
                playerConfig.team = Team.Spectator;
            }
        }
    }

    #endregion
}
