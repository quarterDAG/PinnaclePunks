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

    private void UpdateHeartsUI ()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null) // Check if the Image component is not null
            {
                hearts[i].enabled = i < lives;
            }
            else
            {
                Debug.LogWarning("Heart image at index " + i + " is missing or destroyed.");
            }
        }
    }


    public void ResetLives ()
    {
        lives = hearts.Length;
        UpdateHeartsUI();
    }
}
