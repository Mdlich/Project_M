using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public GameObject uiPanel;
    public void StartGame()
    {
        uiPanel?.SetActive( false );
        Time.timeScale = 1f;
        GameManager.gamePaused = false;
    }

    public void Restart()
    {
        uiPanel?.SetActive( false );
        GameManager.Restart();
    }
}
