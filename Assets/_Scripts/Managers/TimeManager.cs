using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField] private float slowdownFactor = 0.05f;
    //public bool isSlowMotionActive { get; private set; }

    private int slowMotionRequestCount = 0; // Counter for the number of slow motion requests

    public bool isSlowMotionActive => slowMotionRequestCount > 0;


    private void Awake ()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This will ensure the instance is not destroyed between scene changes.
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // If another instance exists, destroy this one.
        }
    }

    public void RequestSlowMotion ()
    {
        if (slowMotionRequestCount == 0) // Only enter slow motion if no other slow motion is active
        {
            EnterSlowMotion();
        }
        slowMotionRequestCount++; // Increment the number of slow motion requests
    }

    // Call this method to cancel a slow motion request.
    public void CancelSlowMotionRequest ()
    {
        slowMotionRequestCount--; // Decrement the number of slow motion requests
        slowMotionRequestCount = Mathf.Max(0, slowMotionRequestCount); // Ensure it never goes below 0

        if (slowMotionRequestCount == 0) // Only exit slow motion if there are no more requests
        {
            ExitSlowMotion();
        }
    }

    // Enter slow motion
    private void EnterSlowMotion ()
    {
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = Time.timeScale * .02f;
    }

    // Exit slow motion
    private void ExitSlowMotion ()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f; // This is the default value for fixedDeltaTime in Unity.
    }


  /*  public void DoSlowMotion ()
    {
        isSlowMotionActive = true;
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = Time.timeScale * .02f;
    }

    public void StopSlowMotion ()
    {
        isSlowMotionActive = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f; // This is the default value for fixedDeltaTime in Unity.
    }

*/

}
