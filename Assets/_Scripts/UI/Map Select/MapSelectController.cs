using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static PlayerConfigData;

public class MapSelectController : MonoBehaviour
{
    private Dictionary<int, int> mapVotes = new Dictionary<int, int>(); // Stores votes for each map
    private List<int> playerIndices = new List<int>(); // List of player indices for vote tracking
    private int totalPlayers; // Total number of players participating
    public List<Transform> mapUIList;
    [SerializeField] private List<string> mapNameList;
    [SerializeField] private GameObject selectorPrefab;

    [SerializeField] private Transform selectorParentGO;

    [SerializeField] private List<Transform> iconPositionsMap1;
    [SerializeField] private List<Transform> iconPositionsMap2;

    [SerializeField] private CountdownUI countdownUI;



    private void Start ()
    {
        PlayerManager.Instance.SetMapSelectController(this);
        PlayerManager.Instance.InitializeMapSelectors();
    }

    public PlayerInput InstantiateSelector ( PlayerConfig config, int playerCount )
    {
        // Instantiate player at the spawn point
        PlayerInput instantiatedPlayer = PlayerInput.Instantiate(
            selectorPrefab,
            controlScheme: config.controlScheme.ToString(),
            pairWithDevice: config.inputDevice,
            playerIndex: playerCount
        );


        instantiatedPlayer.transform.SetParent(selectorParentGO, false);
        instantiatedPlayer.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);

        SetupMapSelector(config, instantiatedPlayer);

        return instantiatedPlayer;
    }

    private void SetupMapSelector ( PlayerConfig config, PlayerInput instantiatedPlayer )
    {
        InputIcon inputIcon = instantiatedPlayer.GetComponent<InputIcon>();
        inputIcon.SetIconConfig(config);
        inputIcon.SetIconColor(config.playerColor);
        inputIcon.SetPlayerStateChoosingMap();

        AddPlayer();
    }


    public void VoteForMap ( int mapIndex, int playerIndex, InputIcon inputIcon )
    {
        var playerConfig = PlayerManager.Instance.GetPlayerConfig(playerIndex);
        if (playerConfig.playerState == PlayerState.Ready) return;

        if (mapIndex == 0)
            inputIcon.SetIconPosition(iconPositionsMap1[playerIndex].position);

        if (mapIndex == 1)
            inputIcon.SetIconPosition(iconPositionsMap2[playerIndex].position);


        // Check if this is a new vote or changing an existing vote
        if (playerIndices.Contains(playerIndex))
        {
            // Find and decrement the old vote
            foreach (var entry in mapVotes)
            {
                if (entry.Key == playerIndex)
                {
                    mapVotes[entry.Key]--;
                    break;
                }
            }
        }
        else
        {
            playerIndices.Add(playerIndex);
        }

        // Add or update the vote
        if (mapVotes.ContainsKey(mapIndex))
        {
            mapVotes[mapIndex]++;
        }
        else
        {
            mapVotes[mapIndex] = 1;
        }

        CheckIfAllPlayersVoted();
    }


    public void SetPlayerChoosingMap ( int playerIndex )
    {
        //Debug.Log($"Player {playerIndex} is choosing a team.");

        PlayerManager.Instance.SetPlayerState(playerIndex, PlayerState.ChoosingMap);

        StopGameCoundown();

    }

    private void StopGameCoundown ()
    {
        countdownUI.StopTimer();
    }

    // Check if all players have voted and then determine the map
    private void CheckIfAllPlayersVoted ()
    {
        if (playerIndices.Count == totalPlayers)
        {
            DetermineMap();
        }
    }

    // Determine the winning map
    private void DetermineMap ()
    {
        int maxVotes = 0;
        List<int> topMaps = new List<int>();

        foreach (var map in mapVotes)
        {
            if (map.Value > maxVotes)
            {
                topMaps.Clear();
                topMaps.Add(map.Key);
                maxVotes = map.Value;
            }
            else if (map.Value == maxVotes)
            {
                topMaps.Add(map.Key);
            }
        }

        // Randomly select a map if there's a tie
        int selectedMapIndex = topMaps[Random.Range(0, topMaps.Count)];
        LoadSelectedMap(selectedMapIndex);
    }

    // Method to load the selected map
    private void LoadSelectedMap ( int mapIndex )
    {
        SceneManager.LoadScene(mapNameList[mapIndex]);
    }

    // Method to set the total number of players
    public void SetTotalPlayers ( int numPlayers )
    {
        totalPlayers = numPlayers;
    }

    private void AddPlayer ()
    {
        totalPlayers++;
    }



}
