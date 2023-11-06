using UnityEngine;
using UnityEngine.UI;

public class LivesManager : MonoBehaviour
{
    public Image[] hearts; // Assign this in the inspector with your heart UI images
    private int lives;

    void Start ()
    {
        // Initialize lives (assuming 3 for this example)
        lives = hearts.Length;
        UpdateHeartsUI();
    }

    // Call this method whenever the player loses a life
    public void LoseLife ()
    {
        if (lives > 0)
        {
            lives--;
            UpdateHeartsUI();
        }

        if (lives <= 0)
        {
            // Player has died, you can handle game over logic here
            Debug.Log("Player has no more lives!");
        }
    }

    // Call this method to update the UI hearts
    private void UpdateHeartsUI ()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < lives)
            {
                hearts[i].enabled = true; // Show heart
            }
            else
            {
                hearts[i].enabled = false; // Hide heart
            }
        }
    }
}
