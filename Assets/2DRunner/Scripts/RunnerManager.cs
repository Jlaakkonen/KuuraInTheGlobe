﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public enum GameState
{
    Begin,
    Pause,
    Died
}
public class RunnerManager : MonoBehaviour
{
    public static RunnerManager manager;
    

    public int[] levelIndex;
    public List<int> completedLevels;

    int currentLevel;

    public int levelSelectScene;
    
    public Button thunderButton;
    public Button snowButton;
    public CatMovement catmovement;
    public float Length = 0;
    public float score = 0;
    public float PointsPerCoin;
    public Text lengthText;
    public Text scoreText;
    public Text timerText;
    public float Timer = 60f;
    public GameObject gameOverPanel;
    public Button play, nextLevel,levelSelect;
    public Text currentScore;
    public bool isDaed = false;
    public GameState currentState;
    public float timer = 5;
    public float snowTimer;
    public float thunderTimer;
    public const int snowCost = 20;
    public const int thunderCost = 10;
    public bool isCatMoving;
    public bool blink;
    public int count;

    private Animator anim;
    private bool showSnow;
    private bool showThunder;
    private CatMovement catMovement;
    private bool pausePressed = false;
    private bool isInitialPanelLoaded = false;
    public GameObject[] Coins;
    
    public void Awake()
    {
        /*This wont destroy main gamemanager on scene download, keeps track of level progress and stuff
        When developing new level, comment this and uncomment manager = this; and InitialPanel(); line
        When done developing level, remove Gamemanager from scene
        */
     
         if (manager == null)
         {
             manager = this;
             DontDestroyOnLoad(manager);

             currentState = GameState.Pause;
         }
         else if(manager != this && manager.gameObject.name != "GameManager")
         {
             Destroy(manager);
            
         }
           /*
      manager = this;
      InitialPanel();
       */ 
    }
    void Update()
    {
        if (currentState == GameState.Begin)
        {
            GetLength();
            UseCoin();
            if (isCatMoving)
            {
                StartCoroutine(catMovement.TranformBack(1f));
            }

            foreach (GameObject coin in Coins)
            {
                Rigidbody2D coinRb = coin.transform.GetComponent<Rigidbody2D>();
                coinRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

        }
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        if (Input.GetKeyDown(KeyCode.R))
            Play();
        if (currentState == GameState.Pause)
        {
            foreach(GameObject coin in Coins)
            {
                Rigidbody2D coinRb = coin.transform.GetComponent<Rigidbody2D>();
                coinRb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void UseCoin()
    {
        if (score >= thunderCost)
            showThunder = true;
        // PopUpTimer(out showThunder, ref thunderTimer, thunderCost);

        if (score >= snowCost && showThunder == true)
            showSnow = true;
        /* else if ((score >= snowCost && !showThunder) || (score >= (snowCost+thunderCost) && showThunder)){
			PopUpTimer (out showSnow, ref snowTimer,snowCost);
		}
*/
    }
    public void PopUpTimer(out bool showSnow, ref float start, float cost)
    {
        showSnow = true;
        if (start <= 0)
        {
            start = 0;
        }
        else if (start > 0)
        {
            start -= Time.deltaTime;
            if (start < 3)
            {
                BlinkCount();
            }
        }
        else if (start == 0)
        {
            start = timer;
            score = score - cost;
            Invoke("GetScore", 1);
            showSnow = false;
        }
    }
    public void BlinkCount()
    {
        if (count == 10)
        {
            count = 0;
            blink = true;
        }
        else
            blink = false;

        count++;
    }
    public void GetScore()
    {
        scoreText.text = "" + score;
    }
    public void AddScore()
    {
        score = score + PointsPerCoin;
        scoreText.text = "" + score;
    }
    void GetLength()
    {
        lengthText.text = GameObject.Find("CharacterRobotBoy").transform.position.x.ToString("F1");
    }
    //Loads next level and stores completed level
    public void PlayerWin()
    {
        if (completedLevels.Contains(currentLevel) == false)
        {
            completedLevels.Add(currentLevel);
        }
        //currentLevel++;
        LoadLevel(currentLevel + 1);
    }

    public void PlayerLose()
    {
        //SceneManager.LoadScene("LevelSelect");
        currentState = GameState.Died;
        gameOverPanel.SetActive(true);
        currentScore.text = score.ToString();

    }
    public void PlayerPause()
    {
        if(!pausePressed && currentState != GameState.Died)
        {
            currentState = GameState.Pause;
            gameOverPanel.SetActive(true);
            pausePressed = true;
            currentScore.text = score.ToString();
            PlatformerCharacter2D.previousVelocity = PlatformerCharacter2D.m_Rigidbody2D.velocity.y;

        }
        else if (pausePressed && currentState != GameState.Died)
        {
            currentState = GameState.Begin;
            gameOverPanel.SetActive(false);
            pausePressed = false;
            PlatformerCharacter2D.m_Rigidbody2D.velocity = new Vector2(0,PlatformerCharacter2D.previousVelocity);

        }

    }
    //Loads the given level
    public void LoadLevel(int levelToLoad)
    {
        score = 0;
        //Add check to not go over array bounds
        if (levelToLoad < levelIndex.Length)
        {
            SceneManager.LoadScene(levelIndex[levelToLoad]);
            currentLevel = levelToLoad;
            
            //InitialPanel();
        }
        else
        {
            Debug.Log(levelIndex.Length);
            Debug.Log("Index out of range in level list!");
            SceneManager.LoadScene(levelSelectScene);
        }
    }
    void OnLevelWasLoaded(int level)
    {
        if (level != 0 )
        {
            InitialPanel();
        }
    }

    void InitialPanel()
    {
            currentState = GameState.Begin;
            lengthText = GameObject.Find("Length").GetComponent<Text>();
            scoreText = GameObject.Find("Score").GetComponent<Text>();
            timerText = GameObject.Find("Time").GetComponent<Text>();
            catMovement = GameObject.Find("CatIcon").GetComponent<CatMovement>();
            anim = GameObject.Find("Effector").GetComponent<Animator>();
            gameOverPanel = GameObject.Find("GameOverPanel");
            play = GameObject.Find("Play").GetComponent<Button>();
            nextLevel = GameObject.Find("NextLevel").GetComponent<Button>();
            levelSelect = GameObject.Find("LevelSelect").GetComponent<Button>();
            currentScore = GameObject.Find("CurrentScore").GetComponent<Text>();
            thunderButton = GameObject.Find("ThunderButton").GetComponent<Button>();
            snowButton = GameObject.Find("SnowButton").GetComponent<Button>();
            Coins = GameObject.FindGameObjectsWithTag("Coin");

            thunderButton.gameObject.SetActive(false);
            snowButton.gameObject.SetActive(false);
            gameOverPanel.SetActive(false);

            play.onClick.AddListener(() => Play());
            nextLevel.onClick.AddListener(() => NextLevel());
            levelSelect.onClick.AddListener(() => BackToMenu());
            thunderButton.onClick.AddListener(() => useThunder());
            snowButton.onClick.AddListener(() => useSnow());
            isInitialPanelLoaded = true;
            pausePressed = false;
            

    }
    public void Play()
    {
        //  currentState = GameState.Begin;
       // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        LoadLevel(currentLevel);
        currentState = GameState.Died;
        //Application.LoadLevel (Application.loadedLevelName);
    }
    public void NextLevel()
    {
        Debug.Log("Ei löydyy");
        //SceneManager.LoadScene("level2");
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("LevelSelect");
    }
    
    void OnGUI()
    {
        if(isInitialPanelLoaded)
        {
            if (showThunder)
            thunderButton.gameObject.SetActive(true);
        else if (!showThunder)
            thunderButton.gameObject.SetActive(false);  

        if (showSnow)
            snowButton.gameObject.SetActive(true);
        else if (!showSnow)
            snowButton.gameObject.SetActive(false);
        }
        
    }
    public void useThunder()
    {
        showThunder = false;
        score -= thunderCost;
        isCatMoving = true;
        anim.SetTrigger("Thunder");
        GetScore();
    }
    public void useSnow()
    {
        showSnow = false;
        score -= snowCost;
        isCatMoving = true;
        anim.SetTrigger("Snow");
        showThunder = false;
        GetScore();
    }
}
