using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Runs the game: title screen, score, lives, game over and restart.
// The interface is built from code (see UIKit) so the scene stays simple.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int startingLives = 3;
    public bool IsPlaying { get; private set; }
    // True during the frame the game starts, so the same key press does not also send a dog.
    public bool JustStarted { get { return Time.frameCount == startFrame; } }

    private int score;
    private int lives;
    private int bestScore;
    private int startFrame = -1;

    private PlayerController player;
    private SpawnManager spawnManager;

    private Transform uiRoot;
    private GameObject titlePanel;
    private GameObject gameOverPanel;
    private GameObject hud;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI livesText;
    private TextMeshProUGUI dogText;
    private RectTransform dogFill;
    private Image dogFillImage;
    private TextMeshProUGUI finalScoreBody;
    private Image hurtFlash;

    private static readonly Color ReadyColor = new Color(0.45f, 0.85f, 0.35f);
    private static readonly Color WaitingColor = new Color(0.95f, 0.75f, 0.25f);

    void Awake()
    {
        Instance = this;
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        player = FindAnyObjectByType<PlayerController>();
        spawnManager = FindAnyObjectByType<SpawnManager>();
        BuildInterface();
        ShowTitle();
    }

    void Update()
    {
        if (!IsPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                StartGame();
            }
            return;
        }

        if (player != null)
        {
            float ready = player.Ready;
            dogFill.anchorMax = new Vector2(ready, 1f);
            dogFillImage.color = ready >= 1f ? ReadyColor : WaitingColor;
            dogText.text = ready >= 1f ? "Dog ready!  <size=70%>Space / click</size>" : "Dog is running back...";
        }
    }

    public void StartGame()
    {
        if (IsPlaying) return;

        // Clear anything left over from the last round.
        foreach (var mover in FindObjectsByType<MoveForward>()) Destroy(mover.gameObject);

        score = 0;
        lives = startingLives;
        startFrame = Time.frameCount;
        IsPlaying = true;
        if (player != null) player.ResetCooldown();
        if (spawnManager != null) spawnManager.ResetSpawning();

        titlePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        hud.SetActive(true);
        UpdateHud();
    }

    // A dog caught a ball.
    public void BallCaught(int points, Vector3 where)
    {
        if (!IsPlaying) return;
        score += points;
        UpdateHud();
        StartCoroutine(ShowPopup("+" + points, where, new Color(0.75f, 1f, 0.45f)));
    }

    // A ball hit the ground.
    public void BallMissed(Vector3 where)
    {
        if (!IsPlaying) return;
        lives--;
        UpdateHud();
        StartCoroutine(FlashHurt());
        if (lives <= 0) GameOver();
    }

    void GameOver()
    {
        IsPlaying = false;
        if (spawnManager != null) spawnManager.StopSpawning();

        bool newBest = score > bestScore;
        if (newBest)
        {
            bestScore = score;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }
        finalScoreBody.text = "Score  <b>" + score + "</b>\n" +
            (newBest ? "<color=#9be36b>New best score!</color>" : "<size=80%>Best  " + bestScore + "</size>");
        hud.SetActive(false);
        gameOverPanel.SetActive(true);
    }

    void ShowTitle()
    {
        IsPlaying = false;
        if (spawnManager != null) spawnManager.StopSpawning();
        hud.SetActive(false);
        gameOverPanel.SetActive(false);
        titlePanel.SetActive(true);
    }

    void UpdateHud()
    {
        int left = Mathf.Max(lives, 0);
        int lost = Mathf.Max(startingLives - lives, 0);
        scoreText.text = "Score  <b>" + score + "</b>";
        livesText.text = "Lives  <color=#ff6b5a>" + new string('●', left) + "</color><color=#6b6f78>" + new string('●', lost) + "</color>";
    }

    IEnumerator FlashHurt()
    {
        for (float t = 0f; t < 0.35f; t += Time.unscaledDeltaTime)
        {
            hurtFlash.color = new Color(0.9f, 0.1f, 0.05f, 0.35f * (1f - t / 0.35f));
            yield return null;
        }
        hurtFlash.color = Color.clear;
    }

    // A "+3" that floats up from the caught ball and fades out.
    IEnumerator ShowPopup(string text, Vector3 worldPosition, Color color)
    {
        var label = UIKit.Text("Score Popup", uiRoot, text, 64, TextAlignmentOptions.Center);
        label.fontStyle = FontStyles.Bold;
        label.color = color;
        label.outlineWidth = 0.2f;
        label.outlineColor = new Color32(10, 12, 18, 255);
        var rect = label.rectTransform;
        rect.sizeDelta = new Vector2(200, 90);

        for (float t = 0f; t < 0.9f; t += Time.deltaTime)
        {
            if (Camera.main != null)
            {
                rect.position = Camera.main.WorldToScreenPoint(worldPosition + Vector3.up * (t * 3f));
            }
            label.alpha = 1f - t / 0.9f;
            yield return null;
        }
        Destroy(label.gameObject);
    }

    // ---------- Interface ----------

    void BuildInterface()
    {
        uiRoot = UIKit.CreateCanvas("Game UI");

        hurtFlash = UIKit.Image("Hurt Flash", uiRoot, Color.clear);
        hurtFlash.raycastTarget = false;
        UIKit.Stretch(hurtFlash.rectTransform);

        // HUD: score, lives and the dog cooldown bar
        hud = UIKit.Group("HUD", uiRoot);
        scoreText = UIKit.HudText("Score", hud.transform, new Vector2(0, 1), new Vector2(48, -36), TextAlignmentOptions.Left);
        livesText = UIKit.HudText("Lives", hud.transform, new Vector2(1, 1), new Vector2(-48, -36), TextAlignmentOptions.Right);

        var barBack = UIKit.Image("Dog Bar", hud.transform, new Color(0.08f, 0.1f, 0.14f, 0.8f));
        barBack.raycastTarget = false;
        var back = barBack.rectTransform;
        back.anchorMin = back.anchorMax = new Vector2(0.5f, 0f);
        back.pivot = new Vector2(0.5f, 0f);
        back.sizeDelta = new Vector2(560, 34);
        back.anchoredPosition = new Vector2(0, 40);
        dogFillImage = UIKit.Image("Fill", barBack.transform, ReadyColor);
        dogFillImage.raycastTarget = false;
        dogFill = dogFillImage.rectTransform;
        dogFill.anchorMin = Vector2.zero;
        dogFill.anchorMax = Vector2.one;
        dogFill.offsetMin = new Vector2(4, 4);
        dogFill.offsetMax = new Vector2(-4, -4);
        dogText = UIKit.Text("Dog Label", hud.transform, "", 36, TextAlignmentOptions.Center);
        dogText.outlineWidth = 0.18f;
        dogText.outlineColor = new Color32(10, 12, 18, 255);
        UIKit.Place(dogText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 82), new Vector2(700, 50));

        // Title screen
        titlePanel = UIKit.Panel("Title", uiRoot, new Vector2(1000, 640));
        TextMeshProUGUI body;
        UIKit.Screen(uiRoot, titlePanel, "Play Fetch", UIKit.Cream, out body, "Play", StartGame);
        body.text = "Balls are falling from the sky! Send your dog running to catch each one before it hits the ground. Small balls are worth more.\n\n" +
                    "<color=#ffd98c><b>Send a dog</b>  Space, or click / tap\n<size=80%>Your dog needs a moment to run back between throws.</size></color>";
        body.fontSize = 34;

        // Game over screen
        gameOverPanel = UIKit.Panel("Game Over", uiRoot, new Vector2(820, 520));
        UIKit.Screen(uiRoot, gameOverPanel, "Game Over", new Color(1f, 0.55f, 0.45f), out finalScoreBody, "Play again", StartGame);
        finalScoreBody.fontSize = 52;
        finalScoreBody.color = UIKit.Cream;
    }
}
