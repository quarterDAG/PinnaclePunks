using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Bar;

public class SlowmotionController : MonoBehaviour
{

    public Bar slowmotionBar { get; private set; }
    [SerializeField] private float slowMotionDrainRate = 1f;

    private InputManager inputManager;

    private void Awake ()
    {
        inputManager = GetComponent<InputManager>();
    }

    private void Start ()
    {
        FindAndAssignSMBar();
    }

    void Update ()
    {
        if (inputManager.IsSlowmotionPressed)
        {
            Debug.Log(gameObject.name + " is pressing the slow motion button. Triggering slow motion.");
            TimeManager.Instance.RequestSlowMotion();  // Force trigger slow motion for testing
        }
        else
        {
            TimeManager.Instance.CancelSlowMotionRequest(); // Force stop slow motion for testing
        }

        HandleSlowMotion();
    }


    private void FindAndAssignSMBar ()
    {
        Bar[] allBars = FindObjectsOfType<Bar>();

        foreach (var bar in allBars)
        {
            if (bar.barType == BarType.Slowmotion)
            {
                if (bar.tag == gameObject.tag)
                {
                    slowmotionBar = bar;
                    break;
                }

            }

        }
    }


    private void HandleSlowMotion ()
    {
        if (inputManager.IsSlowmotionPressed && !slowmotionBar.IsEmpty()) // Check if bar has value before activating slow motion.
        {
            TimeManager.Instance.RequestSlowMotion();
        }

        if (!inputManager.IsSlowmotionPressed || slowmotionBar.IsEmpty()) // Stop slow motion on key release or when the bar is empty.
        {
            TimeManager.Instance.CancelSlowMotionRequest();
        }

        if (TimeManager.Instance.isSlowMotionActive) // If slow motion is active, drain the bar.
        {
            slowmotionBar.UpdateValue(-slowMotionDrainRate * Time.unscaledTime); // Use Time.unscaledDeltaTime to ensure the drain rate is consistent regardless of time scale.
        }
    }


}
