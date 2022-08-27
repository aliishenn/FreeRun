using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelController : MonoBehaviour
{
    public static LevelController Current;
    public bool gameActive = false;

    public GameObject startMenu, gameMenu, gameOverMenu, finishMenu;
    public TMP_Text scoreText, finishScoreText, currentLevelText, nextLevelText;
    public Slider levelProgressBar;
    public float maxDistance;
    public GameObject finishLine;
    public AudioSource gameMusicAudioSource;
    public AudioClip victoryAudioClip, gameOverAudioClip;

    private int curentLevel;
    private int score;

    void Start()
    {
        Current = this;
        curentLevel = PlayerPrefs.GetInt("currentLevel");
        if (SceneManager.GetActiveScene().name != "Level " + curentLevel)
        {
            SceneManager.LoadScene("Level " + curentLevel);
        }
        else
        {
            currentLevelText.text = (curentLevel + 1).ToString();
            nextLevelText.text = (curentLevel + 2).ToString();
        }

        gameMusicAudioSource = Camera.main.GetComponent<AudioSource>();
    }

    void Update()
    {
        Debug.Log(score);
        if (gameActive)
        {
            PlayerController player = PlayerController.Curent;
            float distance = finishLine.transform.position.z - PlayerController.Curent.transform.position.z;
            levelProgressBar.value = 1 - (distance / maxDistance);
        }
    }

    public void StartLevel()
    {
        maxDistance = finishLine.transform.position.z - PlayerController.Curent.transform.position.z;
        PlayerController.Curent.CahangeSpeed(PlayerController.Curent.runningSpeed);
        startMenu.SetActive(false);
        gameMenu.SetActive(true);
        PlayerController.Curent.animator.SetBool("running", true);

        gameActive = true;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene("Level " + (curentLevel + 1));
    }

    public void GameOver()
    {
        gameMusicAudioSource.Stop();
        gameMusicAudioSource.PlayOneShot(gameOverAudioClip);
        gameMenu.SetActive(false);
        gameOverMenu.SetActive(true);
        gameActive = false;
    }

    public void FinishGame()
    {
        gameMusicAudioSource.Stop();
        gameMusicAudioSource.PlayOneShot(victoryAudioClip);
        PlayerPrefs.SetInt("currentLevel", curentLevel + 1);
        finishScoreText.text = score.ToString();
        gameMenu.SetActive(false);
        finishMenu.SetActive(true);
        gameActive = false;
    }

    public void ChangeScore(int increment)
    {
        score += increment;
        scoreText.text = score.ToString();
    }
}