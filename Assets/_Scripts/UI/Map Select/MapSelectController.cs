using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapSelectController : MonoBehaviour
{
    private Dictionary<int, int> mapVotes = new Dictionary<int, int>(); // Stores votes for each map
    private List<int> playerIndices = new List<int>(); // List of player indices for vote tracking
    private int totalPlayers; // Total number of players participating
    public List<Transform> mapsList;
    [SerializeField] private GameObject selectorPrefab;

    [SerializeField] private Transform selectorParentGO;

    private Dictionary<SelectorUI, int> uiSelectors = new Dictionary<SelectorUI, int>();


    private void Start ()
    {
        PlayerManager.Instance.SetMapSelectController(this);
        PlayerManager.Instance.InitializeMapSelectors();
    }

    public PlayerInput InstantiateSelector ( PlayerConfig config, int playerCount )
    {
        Transform spawnPoint = mapsList[0];

        if (spawnPoint == null)
        {
            Debug.LogError("No available spawn points for team: " + config.team);
            return null; // Return null if no spawn point is available
        }


        // Instantiate player at the spawn point
        PlayerInput instantiatedPlayer = PlayerInput.Instantiate(
            selectorPrefab,
            controlScheme: config.controlScheme.ToString(),
            pairWithDevice: config.inputDevice,
            playerIndex: playerCount
        );

        // Set the instantiated player's position and rotation
        instantiatedPlayer.transform.position = spawnPoint.position;
        instantiatedPlayer.transform.SetParent(selectorParentGO, false);
        instantiatedPlayer.transform.SetSiblingIndex(0);
        instantiatedPlayer.transform.localScale = new Vector3(4, 4, 4);

        SetupMapSelector(config, instantiatedPlayer);

        return instantiatedPlayer;
    }

    private void SetupMapSelector ( PlayerConfig config, PlayerInput instantiatedPlayer )
    {
        SelectorUI mapSelector = instantiatedPlayer.GetComponent<SelectorUI>();
        mapSelector.SetMapSelectController(this);
        mapSelector.SetPlayerConfig(config);
        mapSelector.GetOptionList();
        mapSelector.ColorFrame(config);

        //RegisterSelector(mapSelector);
    }

    public void UpdateSelectedMap ( SelectorUI selector, int mapIndex)
    {
            uiSelectors[selector] = mapIndex;
   

        UpdateHeroVisuals();
        //UpdateHeroImages();
    }

    private void UpdateHeroVisuals ()
    {
        ResetSelectorUI(uiSelectors);

        UpdateSelectorUI(uiSelectors);
    }

    private void ResetSelectorUI ( Dictionary<SelectorUI, int> selectorMap )
    {
        foreach (var selector in selectorMap.Keys)
        {
            selector.UpdateVisual(1, true); // Default to a full frame
        }
    }

    private void UpdateSelectorUI ( Dictionary<SelectorUI, int> selectorMap )
    {
        var uiSelectors = new Dictionary<int, List<SelectorUI>>();

        // Populate the map
        foreach (var pair in selectorMap)
        {
            int mapIndex = pair.Value;
            if (!uiSelectors.ContainsKey(mapIndex))
            {
                uiSelectors[mapIndex] = new List<SelectorUI>();
            }
            uiSelectors[mapIndex].Add(pair.Key);
        }

        // Update each selector's visual
        foreach (var pair in uiSelectors)
        {
            var selectorsOnMap = pair.Value;
            if (selectorsOnMap.Count > 1)
            {
                for (int i = 0; i < selectorsOnMap.Count; i++)
                {
                    selectorsOnMap[i].UpdateVisual(selectorsOnMap.Count, i == 0);
                }
            }
        }
    }


    // Call this method when a player selects a map
    public void VoteForMap ( int playerIndex, int mapIndex )
    {
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
        // Load the map based on the mapIndex
        // This could be a scene load or other logic depending on your game structure
    }

    // Method to set the total number of players
    public void SetTotalPlayers ( int numPlayers )
    {
        totalPlayers = numPlayers;
    }

}
