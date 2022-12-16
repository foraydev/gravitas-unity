using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartNewGame() {
        SceneManager.LoadScene("Start_Helipad");
    }

    public void QuitGame() {
        Application.Quit();
    }
}
