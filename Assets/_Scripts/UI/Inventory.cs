using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // This is required for interacting with UI components

public class Inventory : MonoBehaviour
{
    public PlayerMonsterSpawner spawner;
    private InputManager inputManager;
    [SerializeField] private PlayerController inventoryOwnerPlayerController;

    // A struct to hold both the prefab and the count of each type
    [System.Serializable]
    public struct MonsterInventoryItem
    {
        public Monster monsterPrefab;
        public int count;
        public Button button;
        public TextMeshProUGUI countText;

    }

    public List<MonsterInventoryItem> monsterInventory;

    private Dictionary<Vector2, int> directionToIndexMapping;

    private int selectedIndex = -1;

    public Color defaultColor;
    public Color selectedColor;

    private void Awake ()
    {
        inputManager = inventoryOwnerPlayerController.GetComponent<InputManager>();
    }

    private void Start ()
    {
        directionToIndexMapping = new Dictionary<Vector2, int>
        {
            { Vector2.up, 0 },
            { Vector2.down, 1 },
            { Vector2.left, 2 },
            { Vector2.right, 3 }
        };

        foreach (var item in monsterInventory)
        {
            item.button.image.color = defaultColor;
            item.countText.text = item.count.ToString();
        }

    }

    private void Update ()
    {
        Vector2 inputDirection = inputManager.InventoryInput; // This method should return Vector2.zero if no direction is pressed

        if (inputDirection != Vector2.zero)
        {
            TrySelectMonsterWithDirection(inputDirection);
        }
    }

    private void TrySelectMonsterWithDirection ( Vector2 direction )
    {
        if (directionToIndexMapping.TryGetValue(direction, out int monsterIndex))
        {
            OnMonsterSelected(monsterIndex);
        }
    }


    public void OnMonsterSelected ( int monsterIndex )
    {
        if (monsterIndex >= 0 && monsterIndex < monsterInventory.Count)
        {
            // Change the previously selected button back to its default color
            if (selectedIndex != -1)
            {
                MonsterInventoryItem previousItem = monsterInventory[selectedIndex];
                previousItem.button.image.color = defaultColor;
            }

            // Update the selected index and change the button color to indicate selection
            selectedIndex = monsterIndex;
            MonsterInventoryItem selectedItem = monsterInventory[selectedIndex];
            selectedItem.button.image.color = selectedColor;

            // Pass the monster prefab to the spawner without instantiating
            spawner.SetSelectedMonster(selectedItem.monsterPrefab);
        }
        else
        {
            Debug.LogError("Monster index out of range: " + monsterIndex);
        }
    }


    public void UpdateInventoryAfterSpawn ()
    {
        if (selectedIndex >= 0 && selectedIndex < monsterInventory.Count)
        {
            MonsterInventoryItem item = monsterInventory[selectedIndex];
            item.count--;

            item.countText.text = item.count.ToString();

            // Update the button's interactability and reset color if count is 0
            if (item.count <= 0)
            {
                item.button.interactable = false;
                item.button.image.color = defaultColor;
            }

            monsterInventory[selectedIndex] = item; // Update the inventory item
        }
    }

    public int GetSelectedMonsterAmount ( Monster selectedMonster )
    {
        foreach (var item in monsterInventory)
        {
            if (item.monsterPrefab == selectedMonster)
            {
                return item.count;
            }
        }
        return 0;
    }

}
