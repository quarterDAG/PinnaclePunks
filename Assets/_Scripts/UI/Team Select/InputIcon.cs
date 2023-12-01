using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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

            //inputDevice = playerInput.devices[0];
            playerConfig = playerConfigData.AddPlayerConfig(playerInput);

            if (teamSelectionController != null)
            {
                SetIconColor(playerConfig.playerColor);
                SetPlayerStateChoosingTeam();
            }
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
            {
                SetPlayerStateChoosingTeam();
                if (teamSelectionController.AreAllPlayersSelecting())
                    teamSelectionController.PreviousScene();
            }
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

    public void SetIconColor ( Color _playerColor )
    {
        GetComponent<Image>().color = _playerColor;
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
        if (playerConfig.playerState != PlayerState.ChoosingTeam)
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
        else
        {
            LoadMainMenuScene();
        }
    }

    private void LoadMainMenuScene ()
    {
        PlayerManager.Instance.ResetPlayerConfigs();
        SceneManager.LoadScene("MainMenu");
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
