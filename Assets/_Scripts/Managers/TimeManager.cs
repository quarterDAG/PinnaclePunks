using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField] private float slowdownFactor = 0.05f;
    public bool isSlowMotionActive { get; private set; }


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


    public void DoSlowMotion ()
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



}
