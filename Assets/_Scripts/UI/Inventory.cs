using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // This is required for interacting with UI components

public class Inventory : MonoBehaviour
{
    public PlayerMonsterSpawner[] inventoryOwners;
    private InputManager[] inputManagers;

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

    private int selectedIndex = 0;

    public Color defaultColor;
    public Color selectedColor;


    private void Start ()
    {
        GatherInputManagersFromOwners();

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


    public void AddInventoryOwner ( PlayerMonsterSpawner newOwner )
    {
        if (inventoryOwners == null)
        {
            inventoryOwners = new PlayerMonsterSpawner[1] { newOwner };
        }
        else
        {
            // Create a new array that is one element larger than the current one
            PlayerMonsterSpawner[] newInventoryOwners = new PlayerMonsterSpawner[inventoryOwners.Length + 1];
            inventoryOwners.CopyTo(newInventoryOwners, 0);
            newInventoryOwners[inventoryOwners.Length] = newOwner;
            inventoryOwners = newInventoryOwners;
        }
    }

    private void GatherInputManagersFromOwners ()
    {
        inputManagers = new InputManager[inventoryOwners.Length];

        for (int i = 0; i < inventoryOwners.Length; i++)
        {
            inputManagers[i] = inventoryOwners[i].GetComponent<InputManager>();

            if (inventoryOwners[i] != null)
            {
                InputManager manager = inventoryOwners[i].GetComponent<InputManager>();
                if (manager != null)
                {
                    inputManagers[i] = manager;
                }
                else
                {
                    Debug.LogWarning("InputManager component not found on object with PlayerMonsterSpawner component.");
                }
            }
        }
    }

    private void Update ()
    {
        // Calculate the minimum length to avoid IndexOutOfRangeException
        int minLength = Mathf.Min(inventoryOwners.Length, inputManagers.Length);

        for (int i = 0; i < minLength; i++)
        {
            if (inputManagers[i] != null)
            {
                Vector2 inputDirection = inputManagers[i].InventoryInput; // Make sure this method exists and returns Vector2

                if (inputDirection != Vector2.zero)
                {
                    TrySelectMonsterWithDirection(inputDirection, inventoryOwners[i]);
                }
            }
        }
    }


    private void TrySelectMonsterWithDirection ( Vector2 inputDirection, PlayerMonsterSpawner owner )
    {
        Vector2 direction = inputDirection.normalized;


        if (directionToIndexMapping.TryGetValue(direction, out int monsterIndex))
        {
            OnMonsterSelected(monsterIndex, owner);
        }
    }


    public void OnMonsterSelected ( int monsterIndex, PlayerMonsterSpawner owner )
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
            owner.SetSelectedMonster(selectedItem.monsterPrefab);
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

    public void AddMonster ()
    {
        MonsterInventoryItem item = monsterInventory[0]; // because there is only one moster currently
        item.count++;
        item.countText.text = item.count.ToString();

        monsterInventory[selectedIndex] = item;

    }

    public void ResetInventory ()
    {
        // Reset each item in the inventory
        for (int i = 0; i < monsterInventory.Count; i++)
        {
            MonsterInventoryItem item = monsterInventory[i];

            item.count = 1;

            // Update the UI elements for the item
            item.countText.text = item.count.ToString();
            item.button.interactable = true;
            item.button.image.color = defaultColor;
        }

        // Reset the selected index
        if (selectedIndex != -1 && selectedIndex < monsterInventory.Count)
        {
            monsterInventory[selectedIndex].button.image.color = defaultColor;
        }
        selectedIndex = 0;

        /*      // Optionally, update the selected monster in the spawner, if necessary
              if (inventoryOwners.Length > 0)
              {
                  PlayerMonsterSpawner owner = inventoryOwners[0]; // Example: Resetting for the first owner
                  owner.SetSelectedMonster(monsterInventory[selectedIndex].monsterPrefab);
              }*/
    }

}
