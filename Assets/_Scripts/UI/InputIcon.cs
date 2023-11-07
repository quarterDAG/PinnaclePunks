using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class InputIcon : MonoBehaviour
{
    private TeamSelectionController teamSelectionController;
    [SerializeField] private PlayerConfig playerConfig;
    private Rigidbody2D rb;
    [SerializeField] private float speed = 10f;

    [SerializeField] private PlayerConfigData playerConfigData;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InputManager inputManager;

    private InputDevice inputDevice;

    [SerializeField] private float pushBackForce = 5f;

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

        if (inputManager.IsJumpPressed)
            SetPlayerStateReady();

        if (inputManager.IsRopeShootPressed)
            SetPlayerStateChoosingTeam();

    }

    // Call this method when a player joins the game.
    public void AddPlayerConfig ()
    {
        // Get the control scheme from the input manager, typically based on the last input received.
        PlayerConfigData.ControlScheme controlScheme = inputManager.GetCurrentControlScheme(inputDevice);
        int _playerIndex = teamSelectionController.GetUniquePlayerIndex();


        // Call a method to get a unique color for the player.
        Color playerColor = GetUniquePlayerColor(_playerIndex);

        // Create the new PlayerConfig with the unique color and control scheme.
        PlayerConfig newPlayerConfig = new PlayerConfig
        {
            playerIndex = _playerIndex,
            playerColor = playerColor,
            team = PlayerConfigData.Team.Spectator,
            controlScheme = controlScheme
        };

        GetComponent<SpriteRenderer>().color = playerColor;

        // Add the new PlayerConfig to the playerConfigs list.
        teamSelectionController.playerConfigs.Add(newPlayerConfig);

        playerConfig = newPlayerConfig;

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

    private void SetPlayerStateReady ()
    {
        if (playerConfig.playerState != PlayerConfigData.PlayerState.Playing)
        {
            playerConfig.playerState = PlayerConfigData.PlayerState.Ready;
            teamSelectionController.SetPlayerReady(playerConfig.playerIndex);

            // Handle any other functionality that should occur when the player is ready
        }
    }

    private void SetPlayerStateChoosingTeam ()
    {
        if (playerConfig.playerState != PlayerConfigData.PlayerState.Playing)
        {
            playerConfig.playerState = PlayerConfigData.PlayerState.ChoosingTeam;
            teamSelectionController.SetPlayerChoosingTeam(playerConfig.playerIndex);
        }
    }


    #region Colliders

    private void OnTriggerEnter2D ( Collider2D other )
    {
        // Attempt to join TeamA.
        if (other.CompareTag("TeamA"))
        {
            Debug.Log(teamSelectionController.CanJoinTeam(PlayerConfigData.Team.TeamA));
            if (teamSelectionController.CanJoinTeam(PlayerConfigData.Team.TeamA))
            {
                playerConfig.team = PlayerConfigData.Team.TeamA;
                teamSelectionController.SetPlayerTeam(playerConfig.playerIndex, PlayerConfigData.Team.TeamA);
                gameObject.layer = 0;
            }
        }
        // Attempt to join TeamB.
        else if (other.CompareTag("TeamB"))
        {
            teamSelectionController.CanJoinTeam(PlayerConfigData.Team.TeamB);
            if (teamSelectionController.CanJoinTeam(PlayerConfigData.Team.TeamB))
            {
                playerConfig.team = PlayerConfigData.Team.TeamB;
                teamSelectionController.SetPlayerTeam(playerConfig.playerIndex, PlayerConfigData.Team.TeamB);
                gameObject.layer = 0;
            }
        }
    }

    private void OnTriggerExit2D ( Collider2D other )
    {
        if ((other.CompareTag("TeamA") && playerConfig.team == PlayerConfigData.Team.TeamA) ||
            (other.CompareTag("TeamB") && playerConfig.team == PlayerConfigData.Team.TeamB))
        {
            playerConfig.team = PlayerConfigData.Team.Spectator;
            teamSelectionController.SetPlayerTeam(playerConfig.playerIndex, PlayerConfigData.Team.Spectator);
            gameObject.layer = 10;
        }
    }

    #endregion
}
