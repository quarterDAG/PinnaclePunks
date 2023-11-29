using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{


    public void NavigatToScene ( string _nextScene )
    {

        SceneManager.LoadScene(_nextScene);
    }

    public void QuitGame ()
    {
        // If we are running in a standalone build of the game
#if UNITY_STANDALONE
        Application.Quit();
#endif

        // If we are running in the editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }




}
