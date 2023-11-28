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
}
