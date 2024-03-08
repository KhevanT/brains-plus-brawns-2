using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Start()
    {
    }

    // Loads first level scene
    public void DemoButtonClick()
    {
        SceneManager.LoadScene("Demo Overworld");
    }

    // Quits game
    public void MainButtonClick()
    {
        SceneManager.LoadScene("Overworld Main");
    }
}
