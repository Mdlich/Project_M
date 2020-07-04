using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStateButton : CustomButton
{
    public GameObject uiPanel;
    public void StartGame()
    {
        uiPanel?.SetActive( false );
        GameManager.StartGame();
    }

    public void LoadNextLevel()
    {
        uiPanel?.SetActive( false );
        GameManager.LoadNext();
    }

    public void Restart()
    {
        GameManager.Reload();
    }
}
