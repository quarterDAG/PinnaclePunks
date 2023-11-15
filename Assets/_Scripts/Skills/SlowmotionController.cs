using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Bar;

public class SlowmotionController : MonoBehaviour
{

    public Bar slowmotionBar { get; private set; }
    [SerializeField] private float slowMotionDrainRate = 1f;

    private bool isRequestingSlowMotion = false;


    private InputManager inputManager;

    private void Awake ()
    {
        inputManager = GetComponent<InputManager>();
    }

 /*   private void Start ()
    {
        FindAndAssignSMBar();
    }
*/
    void Update ()
    {
        if (inputManager.IsSlowmotionPressed)
        {
            TimeManager.Instance.RequestSlowMotion();  // Force trigger slow motion for testing
        }
        else
        {
            TimeManager.Instance.CancelSlowMotionRequest(); // Force stop slow motion for testing
        }

        HandleSlowMotion();
    }

/*
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
    }*/

    public void SetSMBar(Bar _smBar)
    {
       slowmotionBar = _smBar;
    }


    private void HandleSlowMotion ()
    {
        // If slow motion is pressed and the bar is not empty, request slow motion
        if (inputManager.IsSlowmotionPressed && !slowmotionBar.IsEmpty() && !isRequestingSlowMotion)
        {
            isRequestingSlowMotion = true;
            TimeManager.Instance.RequestSlowMotion();
        }
        // If slow motion is not pressed or the bar is empty, and slow motion was previously requested, cancel it
        else if ((!inputManager.IsSlowmotionPressed || slowmotionBar.IsEmpty()) && isRequestingSlowMotion)
        {
            isRequestingSlowMotion = false;
            TimeManager.Instance.CancelSlowMotionRequest();
        }

        // If slow motion is active, drain the bar
        if (TimeManager.Instance.isSlowMotionActive && isRequestingSlowMotion)
        {
            slowmotionBar.UpdateValue(-slowMotionDrainRate * Time.unscaledDeltaTime); // Use unscaledDeltaTime to ensure the drain rate is consistent
        }
    }


}