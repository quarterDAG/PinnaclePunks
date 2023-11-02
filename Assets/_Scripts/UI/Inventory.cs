using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // This is required for interacting with UI components

public class Inventory : MonoBehaviour
{
    public PlayerMonsterSpawner spawner;

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
    private int selectedIndex = -1;

    public Color defaultColor;
    public Color selectedColor;


    private void Start ()
    {
        // Initialize button colors and listeners
        foreach (var item in monsterInventory)
        {
            item.button.image.color = defaultColor;
            item.countText.text = item.count.ToString();
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
