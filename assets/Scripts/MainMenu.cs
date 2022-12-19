using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    private GameObject mainMenu;
    private GameObject fileMenu;
    private GameObject optionsMenu;
    
    void Start() {
        mainMenu = GameObject.Find("MainMenu");
        fileMenu = GameObject.Find("FileMenu");
        optionsMenu = GameObject.Find("OptionsMenu");
        fileMenu.SetActive(false);
        optionsMenu.SetActive(false);
    }
    
    public void StartNewGame() {
        SceneManager.LoadScene("Start_Helipad");
    }

    public void GoToMenu(string menuname) {
        if (menuname == "MainMenu") {
            mainMenu.SetActive(true);
            fileMenu.SetActive(false);
            optionsMenu.SetActive(false);
        }
        if (menuname == "FileMenu") {
            mainMenu.SetActive(false);
            fileMenu.SetActive(true);
            optionsMenu.SetActive(false);
        }
        if (menuname == "OptionsMenu") {
            mainMenu.SetActive(false);
            fileMenu.SetActive(false);
            optionsMenu.SetActive(true);
        }
    }

    public void QuitGame() {
        Application.Quit();
    }
}
