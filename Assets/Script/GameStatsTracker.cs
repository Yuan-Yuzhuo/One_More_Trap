using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStatsTracker : MonoBehaviour
{
    private const float HudX = 12f;
    private const float HudY = 12f;
    private const float HudWidth = 360f;
    private const float HudHeight = 136f;
    private const string MainMenuSceneName = "MainMenu";

    private static GameStatsTracker instance;

    private float totalStartTime;
    private float levelStartTime;
    private int currentLevelDeaths;
    private int totalDeaths;
    private int currentLevelDoubleJumps;
    private int totalDoubleJumps;
    private int lastDeathFrame = -1;
    private string currentSceneKey = "";
    private bool challengeActive;
    private bool challengeCompleted;
    private bool showHud;
    private bool showExitConfirm;
    private bool showSaveRecordConfirm;
    private float timeScaleBeforeDialog = 1f;
    private ChallengeRecord pendingRecord;

    private GUIStyle boxStyle;
    private GUIStyle labelStyle;
    private GUIStyle dialogBoxStyle;
    private GUIStyle dialogTitleStyle;
    private GUIStyle dialogTextStyle;
    private Texture2D backgroundTexture;
    private Texture2D dialogTexture;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    public static void StartChallenge()
    {
        EnsureInstance();
        instance.StartChallengeInternal();
    }

    public static void CompleteChallenge()
    {
        EnsureInstance();
        instance.CompleteChallengeInternal();
    }

    public static void RegisterDeath()
    {
        EnsureInstance();
        instance.RegisterDeathInternal();
    }

    public static void RegisterDoubleJumpUse()
    {
        EnsureInstance();
        instance.RegisterDoubleJumpUseInternal();
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject statsObject = new GameObject("GameStatsTracker");
        DontDestroyOnLoad(statsObject);
        statsObject.AddComponent<GameStatsTracker>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += HandleSceneLoaded;

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid() && activeScene.isLoaded)
        {
            HandleSceneLoaded(activeScene, LoadSceneMode.Single);
        }
    }

    private void OnDestroy()
    {
        if (instance != this)
        {
            return;
        }

        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (backgroundTexture != null)
        {
            Destroy(backgroundTexture);
        }

        if (dialogTexture != null)
        {
            Destroy(dialogTexture);
        }

        instance = null;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == MainMenuSceneName)
        {
            showHud = false;
            showExitConfirm = false;
            showSaveRecordConfirm = false;
            currentSceneKey = GetSceneKey(scene);
            return;
        }

        if (!challengeActive)
        {
            StartChallengeInternal();
        }

        string sceneKey = GetSceneKey(scene);

        if (sceneKey == currentSceneKey)
        {
            showHud = true;
            return;
        }

        currentSceneKey = sceneKey;
        currentLevelDeaths = 0;
        currentLevelDoubleJumps = 0;
        levelStartTime = Time.unscaledTime;
        showHud = true;
    }

    private static string GetSceneKey(Scene scene)
    {
        if (!string.IsNullOrEmpty(scene.path))
        {
            return scene.path;
        }

        return scene.buildIndex.ToString();
    }

    private void StartChallengeInternal()
    {
        challengeActive = true;
        challengeCompleted = false;
        showHud = false;
        showExitConfirm = false;
        showSaveRecordConfirm = false;
        pendingRecord = null;
        totalStartTime = Time.unscaledTime;
        levelStartTime = Time.unscaledTime;
        currentLevelDeaths = 0;
        totalDeaths = 0;
        currentLevelDoubleJumps = 0;
        totalDoubleJumps = 0;
        currentSceneKey = "";
    }

    private void CompleteChallengeInternal()
    {
        if (!challengeActive || challengeCompleted)
        {
            return;
        }

        challengeCompleted = true;
        challengeActive = false;
        showHud = false;

        if (!LocalGameDatabase.IsLoggedIn)
        {
            SceneManager.LoadScene(MainMenuSceneName);
            return;
        }

        pendingRecord = new ChallengeRecord();
        pendingRecord.challengerName = LocalGameDatabase.CurrentUserName;
        pendingRecord.totalDeaths = totalDeaths;
        pendingRecord.clearTimeSeconds = Mathf.Max(0f, Time.unscaledTime - totalStartTime);
        pendingRecord.doubleJumpUses = totalDoubleJumps;
        pendingRecord.completedAtBeijing = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");

        OpenSaveRecordConfirmDialog();
    }

    private void RegisterDeathInternal()
    {
        if (!challengeActive)
        {
            StartChallengeInternal();
        }

        if (lastDeathFrame == Time.frameCount)
        {
            return;
        }

        lastDeathFrame = Time.frameCount;
        currentLevelDeaths++;
        totalDeaths++;
    }

    private void RegisterDoubleJumpUseInternal()
    {
        if (!challengeActive)
        {
            StartChallengeInternal();
        }

        currentLevelDoubleJumps++;
        totalDoubleJumps++;
    }

    private void OnGUI()
    {
        if (!showHud && !showExitConfirm && !showSaveRecordConfirm)
        {
            return;
        }

        EnsureStyles();

        if (showHud)
        {
            Rect boxRect = new Rect(HudX, HudY, HudWidth, HudHeight);
            GUI.Box(boxRect, GUIContent.none, boxStyle);

            float levelTime = Time.unscaledTime - levelStartTime;
            float totalTime = Time.unscaledTime - totalStartTime;

            GUI.Label(
                new Rect(HudX + 12f, HudY + 10f, HudWidth - 24f, 24f),
                "Time: Level " + FormatTime(levelTime) + " / Total " + FormatTime(totalTime),
                labelStyle
            );

            GUI.Label(
                new Rect(HudX + 12f, HudY + 38f, HudWidth - 24f, 24f),
                "Deaths: Level " + currentLevelDeaths + " / Total " + totalDeaths,
                labelStyle
            );

            GUI.Label(
                new Rect(HudX + 12f, HudY + 66f, HudWidth - 24f, 24f),
                "Double Jumps: Level " + currentLevelDoubleJumps + " / Total " + totalDoubleJumps,
                labelStyle
            );

            if (GUI.Button(new Rect(HudX + 12f, HudY + 98f, 142f, 28f), "Quit To Menu"))
            {
                OpenExitConfirmDialog();
            }
        }

        if (showExitConfirm)
        {
            DrawExitConfirmDialog();
        }

        if (showSaveRecordConfirm)
        {
            DrawSaveRecordConfirmDialog();
        }
    }

    private void OpenExitConfirmDialog()
    {
        showExitConfirm = true;
        PauseForDialog();
    }

    private void OpenSaveRecordConfirmDialog()
    {
        showSaveRecordConfirm = true;
        PauseForDialog();
    }

    private void PauseForDialog()
    {
        if (Time.timeScale > 0f)
        {
            timeScaleBeforeDialog = Time.timeScale;
        }

        Time.timeScale = 0f;
    }

    private void ResumeAfterDialog()
    {
        Time.timeScale = timeScaleBeforeDialog;
    }

    private void EnsureStyles()
    {
        if (boxStyle != null && labelStyle != null && dialogBoxStyle != null)
        {
            return;
        }

        backgroundTexture = new Texture2D(1, 1);
        backgroundTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.55f));
        backgroundTexture.Apply();

        dialogTexture = new Texture2D(1, 1);
        dialogTexture.SetPixel(0, 0, new Color(0.08f, 0.09f, 0.08f, 0.88f));
        dialogTexture.Apply();

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = backgroundTexture;

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 16;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = Color.white;

        dialogBoxStyle = new GUIStyle(GUI.skin.box);
        dialogBoxStyle.normal.background = dialogTexture;

        dialogTitleStyle = new GUIStyle(GUI.skin.label);
        dialogTitleStyle.fontSize = 22;
        dialogTitleStyle.fontStyle = FontStyle.Bold;
        dialogTitleStyle.alignment = TextAnchor.MiddleCenter;
        dialogTitleStyle.normal.textColor = Color.white;

        dialogTextStyle = new GUIStyle(GUI.skin.label);
        dialogTextStyle.fontSize = 16;
        dialogTextStyle.alignment = TextAnchor.MiddleCenter;
        dialogTextStyle.normal.textColor = Color.white;
    }

    private void DrawExitConfirmDialog()
    {
        Rect dialogRect = GetCenteredDialogRect(420f, 170f);
        GUI.Box(dialogRect, GUIContent.none, dialogBoxStyle);
        GUI.Label(new Rect(dialogRect.x + 20f, dialogRect.y + 20f, dialogRect.width - 40f, 32f), "Are you sure you want to quit?", dialogTitleStyle);
        GUI.Label(new Rect(dialogRect.x + 24f, dialogRect.y + 60f, dialogRect.width - 48f, 32f), "Current challenge progress will not be saved.", dialogTextStyle);

        if (GUI.Button(new Rect(dialogRect.x + 74f, dialogRect.y + 112f, 120f, 34f), "Cancel"))
        {
            showExitConfirm = false;
            ResumeAfterDialog();
        }

        if (GUI.Button(new Rect(dialogRect.x + dialogRect.width - 194f, dialogRect.y + 112f, 120f, 34f), "Quit"))
        {
            challengeActive = false;
            challengeCompleted = false;
            showExitConfirm = false;
            showSaveRecordConfirm = false;
            pendingRecord = null;
            ResumeAfterDialog();
            SceneManager.LoadScene(MainMenuSceneName);
        }
    }

    private void DrawSaveRecordConfirmDialog()
    {
        Rect dialogRect = GetCenteredDialogRect(460f, 210f);
        GUI.Box(dialogRect, GUIContent.none, dialogBoxStyle);
        GUI.Label(new Rect(dialogRect.x + 20f, dialogRect.y + 20f, dialogRect.width - 40f, 32f), "Save this clear record?", dialogTitleStyle);

        string recordText = "";
        if (pendingRecord != null)
        {
            recordText =
                "Time: " + FormatTime(pendingRecord.clearTimeSeconds) +
                "   Deaths: " + pendingRecord.totalDeaths +
                "   Double Jumps: " + pendingRecord.doubleJumpUses;
        }

        GUI.Label(new Rect(dialogRect.x + 24f, dialogRect.y + 62f, dialogRect.width - 48f, 54f), recordText, dialogTextStyle);

        if (GUI.Button(new Rect(dialogRect.x + 74f, dialogRect.y + 146f, 140f, 34f), "Do Not Save"))
        {
            pendingRecord = null;
            showSaveRecordConfirm = false;
            ResumeAfterDialog();
            SceneManager.LoadScene(MainMenuSceneName);
        }

        if (GUI.Button(new Rect(dialogRect.x + dialogRect.width - 214f, dialogRect.y + 146f, 140f, 34f), "Save Record"))
        {
            if (pendingRecord != null)
            {
                LocalGameDatabase.AddChallengeRecord(pendingRecord);
            }

            pendingRecord = null;
            showSaveRecordConfirm = false;
            ResumeAfterDialog();
            SceneManager.LoadScene(MainMenuSceneName);
        }
    }

    private static Rect GetCenteredDialogRect(float width, float height)
    {
        return new Rect(
            (Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f,
            width,
            height
        );
    }

    public static string FormatTime(float time)
    {
        time = Mathf.Max(0f, time);

        int totalSeconds = Mathf.FloorToInt(time);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        int hundredths = Mathf.FloorToInt((time - totalSeconds) * 100f);

        return minutes.ToString("00") + ":" + seconds.ToString("00") + "." + hundredths.ToString("00");
    }
}
