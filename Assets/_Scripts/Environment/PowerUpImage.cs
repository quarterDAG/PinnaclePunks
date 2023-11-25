using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class PowerUpImage : MonoBehaviour
{
    public List<Sprite> powerUpSprites; // List of sprites for different power-ups
    public Image powerUpFillImage; // Image component with Fill method

    private float powerUpDuration;
    private float timer;

    // Call this method to activate a power-up
    public void ActivatePowerUp ( int powerUpIndex, float duration )
    {
        if (powerUpIndex < 0 || powerUpIndex >= powerUpSprites.Count)
        {
            Debug.LogError("Invalid power-up index.");
            return;
        }

        // Set the power-up sprite
        powerUpFillImage.sprite = powerUpSprites[powerUpIndex];
        powerUpFillImage.enabled = true;


        // Initialize timer and duration
        powerUpDuration = duration;
        timer = duration;

        // Start the countdown
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown ()
    {
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            powerUpFillImage.fillAmount = timer / powerUpDuration;
            yield return null;
        }

        // Reset the fill amount at the end of the countdown
        powerUpFillImage.enabled = false;
        powerUpFillImage.fillAmount = 1;
    }
}
