using UnityEngine;

public class TimeManager : MonoBehaviour
{

    [SerializeField] private float slowdownFactor = 0.05f;
    public bool isSlowMotionActive { get; private set; }

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
