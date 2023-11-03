using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowmotionController : MonoBehaviour
{

    [SerializeField] private Bar slowmotionBar;
    [SerializeField] private float slowMotionDrainRate = 1f; 

    private InputManager inputManager;


    private void Awake ()
    {
        inputManager = GetComponent<InputManager>();    
    }

    void Update ()
    {
        HandleSlowMotion();
    }


    private void HandleSlowMotion ()
    {
        if (inputManager.IsSlowmotionPressed && !slowmotionBar.IsEmpty()) // Check if bar has value before activating slow motion.
        {
            TimeManager.Instance.DoSlowMotion();
        }

        if (!inputManager.IsSlowmotionPressed || slowmotionBar.IsEmpty()) // Stop slow motion on key release or when the bar is empty.
        {
            TimeManager.Instance.StopSlowMotion();
        }

        if (TimeManager.Instance.isSlowMotionActive) // If slow motion is active, drain the bar.
        {
            slowmotionBar.UpdateValue(-slowMotionDrainRate * Time.unscaledTime); // Use Time.unscaledDeltaTime to ensure the drain rate is consistent regardless of time scale.
        }
    }
}
