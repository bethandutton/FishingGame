using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public const int TARGET_FISH = 10;
    public const int GAME_TIME = 10;
    public const float BONUS_TIME = 2f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreLabel;
    [SerializeField] private TextMeshProUGUI timerLabel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI resultLabel;
    [SerializeField] private TextMeshProUGUI winLoseLabel;
    [SerializeField] private GameObject pausePanel;

    [Header("Game References")]
    [SerializeField] private Claw claw;
    [SerializeField] private FishSpawner fishSpawner;

    private int score;
    private float timeRemaining;
    private bool gameActive;
    private bool isPaused;

    private void Start()
    {
        // Wire buttons at runtime by name
        WireButton("PauseButton", OnPausePressed);
        WireButton("RestartButton", OnRestartPressed);
        WireButton("HomeButton", OnHomePressed);
        WireButton("ResumeButton", OnResumePressed);
        WireButton("RestartPauseButton", OnRestartFromPause);
        WireButton("HomePauseButton", OnHomePressed);

        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        StartGame();
    }

    private void WireButton(string name, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            Button btn = obj.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(action);
        }
    }

    public void StartGame()
    {
        score = 0;
        timeRemaining = GAME_TIME;
        gameActive = true;
        isPaused = false;
        Time.timeScale = 1f;

        claw.ResetClaw();
        claw.SetEnabled(true);
        fishSpawner.ResetFish();

        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        UpdateUI();
    }

    private void Update()
    {
        if (!gameActive || isPaused) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndGame();
        }
        UpdateUI();
    }

    public void OnFishDropped()
    {
        if (!gameActive || isPaused) return;

        score++;
        timeRemaining += BONUS_TIME;
        UpdateUI();

        // Haptic feedback on catch
#if UNITY_IOS
        Handheld.Vibrate();
#endif

        if (score >= TARGET_FISH)
            EndGame();
    }

    private void UpdateUI()
    {
        scoreLabel.text = $"{score}";
        timerLabel.text = $"Time: {Mathf.CeilToInt(timeRemaining)}";
    }

    private void EndGame()
    {
        gameActive = false;
        claw.SetEnabled(false);

        resultLabel.text = $"You caught {score} fish!";

        if (score >= TARGET_FISH)
        {
            winLoseLabel.text = "YOU WIN!";
            winLoseLabel.color = new Color(0.2f, 1f, 0.4f);
        }
        else
        {
            winLoseLabel.text = "TRY AGAIN!";
            winLoseLabel.color = new Color(1f, 0.4f, 0.4f);
        }

        gameOverPanel.SetActive(true);
    }

    public void OnRestartPressed()
    {
        StartGame();
    }

    public void OnHomePressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("HomeScreen");
    }

    public void OnPausePressed()
    {
        if (!gameActive || isPaused) return;
        isPaused = true;
        Time.timeScale = 0f;
        claw.SetEnabled(false);
        pausePanel.SetActive(true);
    }

    public void OnResumePressed()
    {
        isPaused = false;
        Time.timeScale = 1f;
        claw.SetEnabled(true);
        pausePanel.SetActive(false);
    }

    public void OnRestartFromPause()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        StartGame();
    }
}
