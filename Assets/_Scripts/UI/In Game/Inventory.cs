using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // This is required for interacting with UI components

public class Inventory : MonoBehaviour
{
    public PlayerMinionSpawner[] inventoryOwners;
    private InputManager[] inputManagers;

    // A struct to hold both the prefab and the count of each type
    [System.Serializable]
    public struct MinionInventoryItem
    {
        public Minion minionPrefab;
        public int count;
        public Button button;
        public TextMeshProUGUI countText;

    }

    public List<MinionInventoryItem> minionInventory;

    private Dictionary<Vector2, int> directionToIndexMapping;

    private int selectedIndex = 0;

    public Color defaultColor;
    public Color selectedColor;


    private void Start ()
    {
        //GatherInputManagersFromOwners();

        directionToIndexMapping = new Dictionary<Vector2, int>
        {
            { Vector2.up, 0 },
            { Vector2.down, 1 },
            { Vector2.left, 2 },
            { Vector2.right, 3 }
        };

        foreach (var item in minionInventory)
        {
            item.button.image.color = defaultColor;
            item.countText.text = item.count.ToString();
        }

    }


    public void AddInventoryOwner ( PlayerMinionSpawner newOwner )
    {

        if (inventoryOwners == null)
        {
            inventoryOwners = new PlayerMinionSpawner[1] { newOwner };
        }
        else
        {
            // Create a new array that is one element larger than the current one
            PlayerMinionSpawner[] newInventoryOwners = new PlayerMinionSpawner[inventoryOwners.Length + 1];
            inventoryOwners.CopyTo(newInventoryOwners, 0);
            newInventoryOwners[inventoryOwners.Length] = newOwner;
            inventoryOwners = newInventoryOwners;
        }

        GatherInputManagersFromOwners();
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
        if (inventoryOwners == null || inputManagers == null)
        {
            Debug.LogError("inventoryOwners or inputManagers is null");
            return;
        }


        // Calculate the minimum length once, outside the loop
        int minLength = Mathf.Min(inventoryOwners.Length, inputManagers.Length);

        for (int i = 0; i < minLength; i++)
        {
            InputManager inputManager = inputManagers[i]; // Cache the inputManager
            if (inputManager != null)
            {
                Vector2 inputDirection = inputManager.InventoryInput;
                if (inputDirection != Vector2.zero)
                {
                    // Normalize the vector only if necessary
                    Vector2 direction = inputDirection.normalized;

                    if (directionToIndexMapping.TryGetValue(direction, out int minionIndex))
                    {
                        OnMinionSelected(minionIndex, inventoryOwners[i]); // Cache inventoryOwner
                    }
                }
            }
        }
    }



    private void TrySelectMinionWithDirection ( Vector2 inputDirection, PlayerMinionSpawner owner )
    {
        Vector2 direction = inputDirection.normalized;


        if (directionToIndexMapping.TryGetValue(direction, out int minionIndex))
        {
            OnMinionSelected(minionIndex, owner);
        }
    }


    public void OnMinionSelected ( int minionIndex, PlayerMinionSpawner owner )
    {
        if (minionIndex >= 0 && minionIndex < minionInventory.Count)
        {
            // Change the previously selected button back to its default color
            if (selectedIndex != -1)
            {
                MinionInventoryItem previousItem = minionInventory[selectedIndex];
                previousItem.button.image.color = defaultColor;
            }

            // Update the selected index and change the button color to indicate selection
            selectedIndex = minionIndex;
            MinionInventoryItem selectedItem = minionInventory[selectedIndex];
            selectedItem.button.image.color = selectedColor;

            // Pass the monster prefab to the spawner without instantiating
            owner.SetSelectedMinion(selectedItem.minionPrefab);
        }
        else
        {
            Debug.LogError("Monster index out of range: " + minionIndex);
        }
    }


    public void UpdateInventoryAfterSpawn ()
    {
        if (selectedIndex >= 0 && selectedIndex < minionInventory.Count)
        {
            MinionInventoryItem item = minionInventory[selectedIndex];
            item.count--;

            item.countText.text = item.count.ToString();

            // Update the button's interactability and reset color if count is 0
            if (item.count <= 0)
            {
                item.button.interactable = false;
                item.button.image.color = defaultColor;
            }

            minionInventory[selectedIndex] = item; // Update the inventory item
        }
    }

    public int GetSelectedMinionAmount ( Minion selectedMinion )
    {
        foreach (var item in minionInventory)
        {
            if (item.minionPrefab == selectedMinion)
            {
                return item.count;
            }
        }
        return 0;
    }

    public void AddMinion ()
    {
        MinionInventoryItem item = minionInventory[0]; // because there is only one moster currently
        item.count++;
        item.countText.text = item.count.ToString();

        minionInventory[selectedIndex] = item;

    }

    public void ResetInventory ()
    {
        inventoryOwners = null;

        // Reset each item in the inventory
        for (int i = 0; i < minionInventory.Count; i++)
        {
            MinionInventoryItem item = minionInventory[i];

            item.count = 3;

            // Update the UI elements for the item
            item.countText.text = item.count.ToString();
            item.button.interactable = true;
            item.button.image.color = defaultColor;
        }

        // Reset the selected index
        if (selectedIndex != -1 && selectedIndex < minionInventory.Count)
        {
            minionInventory[selectedIndex].button.image.color = defaultColor;
        }
        selectedIndex = 0;
    }

}
