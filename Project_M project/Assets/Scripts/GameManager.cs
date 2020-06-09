using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static bool gamePaused = false;
    private static bool firstGame = true;

    [SerializeField]
    private GameObject startGamePanel;
    [SerializeField]
    private GameObject gameOverPanel;
    [SerializeField]
    private GameObject victoryPanel;

    private string startPName;
    private string gameOverPName;
    private string victoryPName;
    private void Awake()
    {
        gamePaused = true;
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad( this );
        }
        else if (this != instance)
        {
            Destroy( gameObject );
            return;
        }
        startPName = startGamePanel.name;
        gameOverPName = gameOverPanel.name;
        victoryPName = victoryPanel.name;

        /*gameOverPanel.gameObject.SetActive( false );
        victoryPanel.gameObject.SetActive( false );*/
        if (firstGame)
        {
            Time.timeScale = 0f;
            startGamePanel.gameObject.SetActive( true );
        }
        else
        {
            startGamePanel.gameObject.SetActive( false );
        }
        firstGame = false;
        SceneManager.sceneLoaded += instance.Init;
    }

    private void Init(Scene s, LoadSceneMode sm)
    {
        if (startPName.Length == 0 || gameOverPName.Length == 0 || victoryPName.Length == 0)
            return;
        startGamePanel = GameObject.Find( startPName );
        gameOverPanel = GameObject.Find( gameOverPName );
        victoryPanel = GameObject.Find( victoryPName );

        gameOverPanel.gameObject.SetActive( false );
        victoryPanel.gameObject.SetActive( false );
        //instance.startGamePanel.gameObject.SetActive( true );
    }
    public static void Restart()
    {
        gamePaused = true;
        SceneManager.LoadScene( 0 );
        //instance.startGamePanel.gameObject.SetActive( true );
    }

    public static void GameOver()
    {
        Time.timeScale = 0f;
        instance?.gameOverPanel.gameObject.SetActive( true );
    }

    public static void Victory()
    {
        Time.timeScale = 0f;
        instance?.victoryPanel.gameObject.SetActive( true );
    }
}
