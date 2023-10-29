using UnityEngine;

public class TimeManager : MonoBehaviour
{

    public float slowdownFactor = 0.05f;
    public float slowdownLength = 2f;

  /*  private void Update ()
    {
        Time.timeScale += (1f / slowdownLength) * Time.unscaledDeltaTime;
        Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
    }*/

    public void DoSlowMotion()
    {
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = Time.timeScale * .02f;
    }

    public void StopSlowMotion ()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f; // This is the default value for fixedDeltaTime in Unity.
    }

}
