﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameController : MonoBehaviour
{
    public int score = 0;
    public int level = 1;
    public float enemymissileSpeed = 3f;
    [SerializeField] private float enemyMissileSpeedMultiplier = 1.1f;
    public int playermissilesLeft = 30;
    private int enemyMissilesThisRound = 10;
    private int enemyMissilesLeftInRound = 10;
    EnemyMissileSpawner enemyMissileSpawner = null;
    [SerializeField] GameObject sceneloader = null;
    SoundController soundController = null;
    CoopScript coopcontroller = null;
    //Score Values
    private int missileDestroyedPoints = 25;
    //private int cityLossPenalty = 100;
    //private int launcherLossPenalty = 200;
    [SerializeField] private int missileEndOfRoundpoints = 50;
    [SerializeField] private int cityEndOfRoundpoints = 100;

    //TextGUI
    [SerializeField] private TextMeshProUGUI myScoreText = null;
    [SerializeField] private TextMeshProUGUI myLevelText = null;
    [SerializeField] private TextMeshProUGUI myMissilesLeftText = null;
    [SerializeField] private TextMeshProUGUI lowMissileWarning = null;
    [SerializeField] private TextMeshProUGUI noMissileWarning = null;
    [SerializeField] private TextMeshProUGUI leftOverMissileBonusText = null;
    [SerializeField] private TextMeshProUGUI leftOverCityBonusText = null;
    [SerializeField] private TextMeshProUGUI totalBonusText = null;
    [SerializeField] private TextMeshProUGUI CountdownText = null;
    [SerializeField] private GameObject endOfRoundPanel = null;
    [SerializeField] private GameObject startGamePanel = null;
    [SerializeField] private GameObject gameOverScreen = null;
    [SerializeField] private TextMeshProUGUI FinalScoreText = null;
    [SerializeField] private Button closeScreen = null;
    

    //Calculations used for various aspects of administrative game functionality. 
    public bool gameStart = false;
    public bool isRoundOver = false;
    private bool gameOverState = false;
    private bool warned = false;
    private bool zeroMissiles = false;
    int citycount = 6;
    [SerializeField] GameObject maincam = null;
    private bool isPaused = false;
    public bool coopCheck = false;
    // Start is called before the first frame update
    void Start()
    {
        enemyMissileSpawner = GameObject.FindObjectOfType<EnemyMissileSpawner>();
        soundController = GameObject.FindObjectOfType<SoundController>();
        sceneloader.GetComponent<Scenetransition>().sceneName = SceneManager.GetActiveScene().name;

        gameStart = false;
        if (sceneloader.GetComponent<Scenetransition>().sceneName == "CoopScene")
        {
            coopCheck = true;
            playermissilesLeft = 15;
            enemyMissileSpawner.missileToSpawn = 0;
            UpdateMissilesLeftText();
            coopcontroller = GameObject.FindObjectOfType<CoopScript>();
            coopcontroller.LoadCoop();
        }
        if (coopCheck == false)
        {
            UpdateScoreText();
            UpdateLevelText();
            UpdateMissilesLeftText();
            StartCoroutine(LoadIn());

            StartCoroutine(MissileWaiter());
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(enemyMissilesLeftInRound);
        if (coopCheck == false) { 
                if (enemyMissilesLeftInRound <= 0 && isRoundOver == false && gameOverState == false)
            {
                isRoundOver = true;
                StartCoroutine(EndOfRound());
            }
        
            MissileCheck();
            GameOver();
        }
        else
        {
            CoopRoundOver();
        }
    }
    private void MissileCheck()
    {
        if (playermissilesLeft == 15 && warned == false)
        {
            soundController.LowMissileSFX();
            warned = true;
            lowMissileWarning.gameObject.SetActive(true);
        }
        else if (playermissilesLeft == 0 && zeroMissiles == false)
        {
            soundController.NoMissilesSFX();
            zeroMissiles = true;
            noMissileWarning.gameObject.SetActive(true);
            lowMissileWarning.gameObject.SetActive(false);
        }
    }
    
    private void InstructionScreen()
    {
        Time.timeScale = 0.0f;
        startGamePanel.SetActive(true);
        closeScreen.onClick.AddListener(CloseScreen);
        
    }
    
    private void CloseScreen()
    {
        Time.timeScale = 1.0f;
        startGamePanel.SetActive(false);
        gameStart = true;
    }
    public void UpdateMissilesLeftText()
    {
        myMissilesLeftText.text = "Missiles Left: " + playermissilesLeft;
    }
    public void UpdateScoreText()
    {
        myScoreText.text = "Score: " + score;
    }
    public void UpdateLevelText()
    {
        myLevelText.text = "Level: " + level;
    }

    public void AddMissileDestroyedPoints()
    {
        score += missileDestroyedPoints;
        enemyMissilesLeftInRound--;
        UpdateScoreText();
    }

    public void MissileDestroyedCity()
    {
        //score -= cityLossPenalty;
        enemyMissilesLeftInRound--;
        UpdateScoreText();
    }

    public void MissileDestroyedSilo()
    {
        //score -= launcherLossPenalty;
        enemyMissilesLeftInRound--;
        UpdateScoreText();
    }
    public void MissileHitDead()
    {
        enemyMissilesLeftInRound--;
    }

    private void StartRound()
    {
        enemyMissileSpawner.missileToSpawn = enemyMissilesThisRound;
        enemyMissilesLeftInRound = enemyMissilesThisRound;
        enemyMissileSpawner.StartRound();
    }
    private void CitySearch()
    {
        citycount = 6;
        CityController[] cities = GameObject.FindObjectsOfType<CityController>();
        foreach (CityController city in cities)
            
            if (city.CityDead() == true)
            {
                citycount--;
            }
            //Debug.Log(citycount);
    }

    private void GameOver()
    {
        int siloDeath = maincam.GetComponent<CursorMover>().DeathCounter();
        CitySearch();
        if ((citycount == 0 || siloDeath >= 3) )
        {
            
            gameOverScreen.SetActive(true);
            gameOverState = true;
            
            FinalScoreText.text = "Final Score: " + score;
            if (Time.timeScale == 1.0f && isPaused == false)
            {
                soundController.GameOverSFX();
                Time.timeScale = 0.0f;
                isPaused = true;
            }
            if (Input.GetKeyDown(KeyCode.Space) && gameOverState == true)
                {
                    
                    Time.timeScale = 1.0f;
                    gameOverScreen.SetActive(false);
                    gameOverState = false;
                    ResetGame();
                }
            if (Input.GetKeyDown(KeyCode.Q) && gameOverState == true)
            {
                Time.timeScale = 1.0f;
                
            }
            
        }

    }
    private void CoopRoundOver()
    {
        int siloDeath = maincam.GetComponent<CursorMover>().DeathCounter();
        CitySearch();
        if ((citycount == 0 || siloDeath >= 3))
        {
            isRoundOver = true;
            if (coopcontroller.turncount == 0)
            {
                coopcontroller.RoundOverCoop();
                citycount = 6;
                siloDeath = 0;
            }
            if (coopcontroller.turncount == 1)
            {
                coopcontroller.coopGameOver = true;
                coopcontroller.CoopGameOver();
                if (Time.timeScale == 1.0f && isPaused == false)
                {
                    soundController.GameOverSFX();
                    Time.timeScale = 0.0f;
                    isPaused = true;
                }
                if (Input.GetKeyDown(KeyCode.Space) && coopcontroller.coopGameOver == true)
                {

                    Time.timeScale = 1.0f;
                    //gameOverScreen.SetActive(false);    Replace with Coop end screen
                    coopcontroller.coopGameOver = false;
                    ResetCoop();
                }
                if (Input.GetKeyDown(KeyCode.Q) && coopcontroller.coopGameOver == true)
                {
                    Time.timeScale = 1.0f;

                }
            }
            

        }
    }
        private void ResetGame()
    {
        
        sceneloader.GetComponent<Scenetransition>().sceneName = "MainGame";
        sceneloader.GetComponent<Scenetransition>().SceneReset();
    }
    private void ResetCoop()
    {

        sceneloader.GetComponent<Scenetransition>().sceneName = "CoopScene";
        sceneloader.GetComponent<Scenetransition>().SceneReset();
    }
    private void QuitGame()
    {
        sceneloader.GetComponent<Scenetransition>().sceneName = "Title Screen";
        sceneloader.GetComponent<Scenetransition>().SceneReset();
    }
    public IEnumerator EndOfRound()
    {
        yield return new WaitForSeconds(0.5f);
         
        endOfRoundPanel.SetActive(true);
        int leftOverMissileBonus = playermissilesLeft * missileEndOfRoundpoints;
        soundController.StartRoundSFX();
        CitySearch();

        int leftOverCityBonus = citycount * cityEndOfRoundpoints;
        int totalBonus = leftOverCityBonus + leftOverMissileBonus;
        leftOverMissileBonusText.text = "Left over missile bonus: " + leftOverMissileBonus;
        leftOverCityBonusText.text = "Left over city bonus: " + leftOverCityBonus;
        totalBonusText.text = "Total bonus: " + totalBonus;




        score += totalBonus;
        UpdateScoreText();

        //Updating new round values
        CountdownText.text = "5";
        yield return new WaitForSeconds(1f);
        CountdownText.text = "4";
        yield return new WaitForSeconds(1f);
        CountdownText.text = "3";
        yield return new WaitForSeconds(1f);
        CountdownText.text = "2";
        yield return new WaitForSeconds(1f);
        CountdownText.text = "1";
        yield return new WaitForSeconds(1f);

        endOfRoundPanel.SetActive(false);
        playermissilesLeft = 30;
        level++;
        enemymissileSpeed = enemymissileSpeed * enemyMissileSpeedMultiplier;
        enemyMissileSpeedMultiplier += 0.1f;
        isRoundOver = false;
        warned = false;
        zeroMissiles = false;
        noMissileWarning.gameObject.SetActive(false);
        lowMissileWarning.gameObject.SetActive(false);
        StartRound();
        UpdateLevelText();
        UpdateMissilesLeftText();
    }
    public IEnumerator LoadIn()
    {
        yield return new WaitForSeconds(1.5f);
        InstructionScreen();
    }
    public IEnumerator MissileWaiter()
    {
        yield return new WaitForSeconds(1.5f);
        StartRound();
    }
}
