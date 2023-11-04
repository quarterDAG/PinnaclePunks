using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField] private float slowdownFactor = 0.05f;
    private int slowMotionRequestCount = 0;

    public bool isSlowMotionActive;

    private void Awake ()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update ()
    {
        if (slowMotionRequestCount > 0)
        {
            isSlowMotionActive = true;
        } else
        {
            isSlowMotionActive = false;
        }
    }

    public void RequestSlowMotion ()
    {
        slowMotionRequestCount++;
        UpdateSlowMotionState();
    }

    public void CancelSlowMotionRequest ()
    {
        slowMotionRequestCount = Mathf.Max(0, slowMotionRequestCount - 1);
        UpdateSlowMotionState();
    }

    private void UpdateSlowMotionState ()
    {
        if (isSlowMotionActive && Time.timeScale != slowdownFactor)
        {
            EnterSlowMotion();
        }
        else if (!isSlowMotionActive && Time.timeScale != 1f)
        {
            ExitSlowMotion();
        }
    }

    private void EnterSlowMotion ()
    {
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = Time.timeScale * .02f;
    }

    private void ExitSlowMotion ()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f; // This is the default value for fixedDeltaTime in Unity.
    }
}
