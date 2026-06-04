using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenuController : MonoBehaviour
{
    private const int FirstChallengeSceneIndex = 1;
    private const int RankingLimit = 10;
    private const string DemoVideoResourceName = "Video Project";

    private AudioSource menuAudioSource;
    private AudioClip clickClip;
    private VideoPlayer demoVideoPlayer;
    private RenderTexture demoRenderTexture;
    private VideoClip demoVideoClip;

    [SerializeField] private Texture2D backgroundTexture;

    private enum MenuView
    {
        Login,
        Register,
        Ranking,
        Configuration,
        Demo
    }

    private enum RankingView
    {
        Deaths,
        ClearTime,
        DoubleJumps
    }

    private MenuView currentView = MenuView.Login;
    private RankingView rankingView = RankingView.Deaths;
    private PlayerInputAction? pendingInputAction = null;

    private string userName = "";
    private string password = "";
    private string message = "";
    private bool showLoginErrorDialog = false;
    private string loginErrorMessage = "";

    private GUIStyle titleStyle;
    private GUIStyle titleShadowStyle;
    private GUIStyle labelStyle;
    private GUIStyle messageStyle;
    private GUIStyle panelStyle;
    private GUIStyle tableHeaderStyle;
    private GUIStyle tableCellStyle;
    private GUIStyle tableAltCellStyle;
    private GUIStyle tableEmptyStyle;
    private GUIStyle closeButtonStyle;
    private GUIStyle dialogBoxStyle;
    private GUIStyle dialogTitleStyle;
    private GUIStyle dialogTextStyle;
    private GUIStyle demoMessageStyle;
    private Texture2D panelTexture;
    private Texture2D dialogTexture;
    private Texture2D tableHeaderTexture;
    private Texture2D tableRowTexture;
    private Texture2D tableAltRowTexture;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    // Ensures the menu controller exists whenever the main menu scene loads.
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded += EnsureMenuController;
        EnsureMenuController(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    // Creates a menu controller object for the main menu scene if one is missing.
    private static void EnsureMenuController(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainMenu")
        {
            return;
        }

        if (FindObjectOfType<MainMenuController>() != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject("MainMenuController");
        controllerObject.AddComponent<MainMenuController>();
    }

    // Lazily creates menu audio and loads the click sound.
    private void EnsureAudio()
    {
        if (menuAudioSource != null)
        {
            return;
        }

        menuAudioSource = gameObject.AddComponent<AudioSource>();
        menuAudioSource.playOnAwake = false;

        clickClip = Resources.Load<AudioClip>("click");
    }

    // Plays the menu click sound if it is available.
    private void PlayClickSound()
    {
        
        EnsureAudio();


        if (clickClip != null)
        {
            menuAudioSource.PlayOneShot(clickClip);
        }
    }

    private void OnDestroy()
    {
        if (panelTexture != null)
        {
            Destroy(panelTexture);
        }

        if (dialogTexture != null)
        {
            Destroy(dialogTexture);
        }

        StopDemoVideo();

        if (demoRenderTexture != null)
        {
            demoRenderTexture.Release();
            Destroy(demoRenderTexture);
        }

        if (demoVideoPlayer != null)
        {
            Destroy(demoVideoPlayer);
        }

        if (tableHeaderTexture != null)
        {
            Destroy(tableHeaderTexture);
        }

        if (tableRowTexture != null)
        {
            Destroy(tableRowTexture);
        }

        if (tableAltRowTexture != null)
        {
            Destroy(tableAltRowTexture);
        }
    }

    // Draws the current main menu view and modal overlays.
    private void OnGUI()
    {

        EnsureAudio();

        EnsureStyles();
        DrawBackground();
        CapturePendingInputKey();

        float maxPanelWidth = currentView == MenuView.Ranking || currentView == MenuView.Demo ? 760f : 520f;
        float panelWidth = Mathf.Min(maxPanelWidth, Screen.width - 32f);
        float panelHeight = currentView == MenuView.Ranking || currentView == MenuView.Demo ? 560f : 540f;
        Rect panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            Mathf.Max(20f, (Screen.height - panelHeight) * 0.5f),
            panelWidth,
            panelHeight
        );

        GUI.Box(panelRect, GUIContent.none, panelStyle);

        if (currentView == MenuView.Ranking || currentView == MenuView.Configuration || currentView == MenuView.Demo)
        {
            Rect closeRect = new Rect(panelRect.xMax - 48f, panelRect.y + 14f, 34f, 34f);
            if (GUI.Button(closeRect, "X", closeButtonStyle))
            {
                PlayClickSound();
                currentView = MenuView.Login;
                message = "";
                pendingInputAction = null;
                StopDemoVideo();
                CloseLoginErrorDialog();
            }
        }

        Rect contentRect = new Rect(panelRect.x + 28f, panelRect.y + 24f, panelRect.width - 56f, panelRect.height - 48f);
        DrawTitle(contentRect);

        GUILayout.BeginArea(new Rect(contentRect.x, contentRect.y + 74f, contentRect.width, contentRect.height - 74f));

        if (currentView == MenuView.Login)
        {
            DrawLogin();
        }
        else if (currentView == MenuView.Register)
        {
            DrawRegister();
        }
        else if (currentView == MenuView.Ranking)
        {
            DrawRanking();
        }
        else if (currentView == MenuView.Configuration)
        {
            DrawConfiguration();
        }
        else
        {
            DrawDemo(contentRect.width, contentRect.height - 74f);
        }

        GUILayout.EndArea();

        if (showLoginErrorDialog)
        {
            DrawLoginErrorDialog();
        }
    }

    // Draws the game title with a decorative font, subtle shadow, and divider line.
    private void DrawTitle(Rect contentRect)
    {
        Rect titleRect = new Rect(contentRect.x, contentRect.y, contentRect.width, 52f);
        Rect shadowRect = new Rect(titleRect.x + 2f, titleRect.y + 3f, titleRect.width, titleRect.height);

        GUI.Label(shadowRect, "One More Trap", titleShadowStyle);
        GUI.Label(titleRect, "One More Trap", titleStyle);

        Color oldColor = GUI.color;
        GUI.color = new Color(0.18f, 0.28f, 0.18f, 0.46f);
        GUI.DrawTexture(new Rect(contentRect.x + 54f, contentRect.y + 60f, contentRect.width - 108f, 1f), Texture2D.whiteTexture);
        GUI.color = oldColor;
    }

    // Draws login controls and navigation buttons.
    private void DrawLogin()
    {
        DrawAccountFields();

        GUILayout.Space(18f);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Login", GUILayout.Height(42f)))
        {
            PlayClickSound();

            LoginResult result = LocalGameDatabase.Login(userName, password);
            message = GetLoginMessage(result);

            if (result == LoginResult.Success)
            {
                showLoginErrorDialog = false;
                loginErrorMessage = "";
            }
            else
            {
                loginErrorMessage = message;
                showLoginErrorDialog = true;
            }
        }

        GUI.enabled = LocalGameDatabase.IsLoggedIn;
        if (GUILayout.Button("Start Challenge", GUILayout.Height(42f)))
        {
            PlayClickSound();
            GameStatsTracker.StartChallenge();
            SceneTransitionController.LoadScene(FirstChallengeSceneIndex);
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        GUILayout.Space(18f);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Register", GUILayout.Height(36f)))
        {
            PlayClickSound();
            currentView = MenuView.Register;
            message = "";
            CloseLoginErrorDialog();
        }

        if (GUILayout.Button("Rankings", GUILayout.Height(36f)))
        {
            PlayClickSound();

            currentView = MenuView.Ranking;
            message = "";
            pendingInputAction = null;
            CloseLoginErrorDialog();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(8f);

        if (GUILayout.Button("Personalized Configuration", GUILayout.Height(36f)))
        {
            PlayClickSound();

            currentView = MenuView.Configuration;
            message = "";
            pendingInputAction = null;
            CloseLoginErrorDialog();
        }

        GUILayout.Space(10f);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("View Demo", GUILayout.Height(34f)))
        {
            PlayClickSound();

            currentView = MenuView.Demo;
            message = "";
            pendingInputAction = null;
            CloseLoginErrorDialog();
            StartDemoVideo();
        }

        if (GUILayout.Button("Quit Game", GUILayout.Height(34f)))
        {
            PlayClickSound();
            QuitGame();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(14f);
        DrawStatus();
    }

    // Draws registration controls.
    private void DrawRegister()
    {
        DrawAccountFields();

        GUILayout.Space(12f);

        if (GUILayout.Button("Register", GUILayout.Height(38f)))
        {
            PlayClickSound();

            RegisterResult result = LocalGameDatabase.Register(userName, password);
            message = GetRegisterMessage(result);

            if (result == RegisterResult.Success)
            {
                currentView = MenuView.Login;
            }
        }

        if (GUILayout.Button("Back To Login", GUILayout.Height(34f)))
        {
            PlayClickSound();

            currentView = MenuView.Login;
            message = "";
            CloseLoginErrorDialog();
        }

        GUILayout.Space(10f);
        DrawStatus();
    }

    // Draws ranking category tabs and rows.
    private void DrawRanking()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(rankingView == RankingView.Deaths, "Deaths", "Button", GUILayout.Height(34f)))
        {
            if (rankingView != RankingView.Deaths)
            {
                PlayClickSound();
            }

            rankingView = RankingView.Deaths;
        }
        if (GUILayout.Toggle(rankingView == RankingView.ClearTime, "Clear Time", "Button", GUILayout.Height(34f)))
        {
            if (rankingView != RankingView.ClearTime)
            {
                PlayClickSound();
            }

            rankingView = RankingView.ClearTime;
        }
        if (GUILayout.Toggle(rankingView == RankingView.DoubleJumps, "Double Jumps", "Button", GUILayout.Height(34f)))
        {
            if (rankingView != RankingView.DoubleJumps)
            {
                PlayClickSound();
            }

            rankingView = RankingView.DoubleJumps;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(12f);
        DrawRankingRows();
    }

    // Draws the player key-binding configuration view.
    private void DrawConfiguration()
    {
        GUILayout.Label("Personalized Configuration", labelStyle);
        GUILayout.Space(12f);

        DrawInputBindingRow(PlayerInputAction.MoveLeft);
        DrawInputBindingRow(PlayerInputAction.MoveRight);
        DrawInputBindingRow(PlayerInputAction.Jump);
        DrawInputBindingRow(PlayerInputAction.Dash);

        GUILayout.Space(14f);

        if (pendingInputAction.HasValue)
        {
            GUILayout.Label("Press any key for " + PlayerInputConfig.GetActionLabel(pendingInputAction.Value), messageStyle);
        }
        else
        {
            GUILayout.Label("Click an action, then press the key you want to use.", labelStyle);
        }

        GUILayout.Space(12f);

        if (GUILayout.Button("Reset Defaults", GUILayout.Height(34f)))
        {
            PlayClickSound();
            PlayerInputConfig.ResetDefaults();
            pendingInputAction = null;
        }

        if (GUILayout.Button("Back To Login", GUILayout.Height(34f)))
        {
            PlayClickSound();
            currentView = MenuView.Login;
            pendingInputAction = null;
            message = "";
        }
    }

    // Draws the looping demo video view.
    private void DrawDemo(float contentWidth, float contentHeight)
    {
        EnsureDemoVideo();

        if (demoVideoClip == null || demoRenderTexture == null)
        {
            GUILayout.Label("Demo video could not be loaded.", demoMessageStyle, GUILayout.Height(44f));
            GUILayout.Space(12f);
            if (GUILayout.Button("Back To Login", GUILayout.Height(34f)))
            {
                PlayClickSound();
                currentView = MenuView.Login;
                StopDemoVideo();
            }

            return;
        }

        float videoWidth = contentWidth;
        float videoHeight = Mathf.Min(contentHeight - 54f, videoWidth * 9f / 16f);
        Rect videoRect = GUILayoutUtility.GetRect(videoWidth, videoHeight, GUILayout.ExpandWidth(true));

        GUI.DrawTexture(videoRect, demoRenderTexture, ScaleMode.ScaleToFit, false);

        GUILayout.Space(14f);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Back To Login", GUILayout.Width(150f), GUILayout.Height(34f)))
        {
            PlayClickSound();
            currentView = MenuView.Login;
            StopDemoVideo();
        }
        GUILayout.EndHorizontal();
    }

    // Creates the VideoPlayer and render target used by the demo view.
    private void EnsureDemoVideo()
    {
        if (demoVideoClip == null)
        {
            demoVideoClip = Resources.Load<VideoClip>(DemoVideoResourceName);
        }

        if (demoVideoClip == null)
        {
            return;
        }

        if (demoRenderTexture == null)
        {
            demoRenderTexture = new RenderTexture(1280, 720, 0);
            demoRenderTexture.Create();
        }

        if (demoVideoPlayer == null)
        {
            demoVideoPlayer = gameObject.AddComponent<VideoPlayer>();
            demoVideoPlayer.playOnAwake = false;
            demoVideoPlayer.isLooping = true;
            demoVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
            demoVideoPlayer.targetTexture = demoRenderTexture;
            demoVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            demoVideoPlayer.controlledAudioTrackCount = 1;
            demoVideoPlayer.SetDirectAudioMute(0, true);
        }

        demoVideoPlayer.clip = demoVideoClip;
        demoVideoPlayer.targetTexture = demoRenderTexture;
    }

    // Starts the demo video from the beginning and loops it.
    private void StartDemoVideo()
    {
        EnsureAudio();
        EnsureDemoVideo();

        if (demoVideoPlayer == null || demoVideoClip == null)
        {
            return;
        }

        demoVideoPlayer.Stop();
        demoVideoPlayer.time = 0f;
        demoVideoPlayer.Play();
    }

    // Stops demo playback when leaving the demo view.
    private void StopDemoVideo()
    {
        if (demoVideoPlayer != null)
        {
            demoVideoPlayer.Stop();
        }
    }

    // Draws one configurable action row and starts key capture when clicked.
    private void DrawInputBindingRow(PlayerInputAction action)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(PlayerInputConfig.GetActionLabel(action), labelStyle, GUILayout.Width(160f), GUILayout.Height(34f));

        string buttonText = pendingInputAction.HasValue && pendingInputAction.Value == action
            ? "Press a key..."
            : PlayerInputConfig.GetKey(action).ToString();

        if (GUILayout.Button(buttonText, GUILayout.Height(34f)))
        {
            PlayClickSound();
            pendingInputAction = action;
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(6f);
    }

    // Draws shared user name and password fields.
    private void DrawAccountFields()
    {
        GUILayout.Label("User Name", labelStyle);
        userName = GUILayout.TextField(userName, GUILayout.Height(32f));

        GUILayout.Space(8f);

        GUILayout.Label("Password", labelStyle);
        password = GUILayout.PasswordField(password, '*', GUILayout.Height(32f));
    }

    // Stores the next pressed key for the action currently being configured.
    private void CapturePendingInputKey()
    {
        if (!pendingInputAction.HasValue)
        {
            return;
        }

        KeyCode key;
        if (!PlayerInputConfig.TryGetPressedKey(Event.current, out key))
        {
            return;
        }

        if (key == KeyCode.Escape)
        {
            pendingInputAction = null;
            Event.current.Use();
            return;
        }

        PlayerInputConfig.SetKey(pendingInputAction.Value, key);
        pendingInputAction = null;
        Event.current.Use();
    }

    // Draws login status and the latest account message.
    private void DrawStatus()
    {
        string status = LocalGameDatabase.IsLoggedIn
            ? "Logged in as " + LocalGameDatabase.CurrentUserName
            : "Please login before starting challenge";

        GUILayout.Label(status, labelStyle);

        if (!string.IsNullOrEmpty(message))
        {
            GUILayout.Label(message, messageStyle);
        }
    }

    // Exits the built game, or stops Play Mode when running inside the Unity Editor.
    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void CloseLoginErrorDialog()
    {
        showLoginErrorDialog = false;
        loginErrorMessage = "";
    }

    // Draws the login error dialog shown after failed login attempts.
    private void DrawLoginErrorDialog()
    {
        Rect dialogRect = new Rect(
            (Screen.width - 360f) * 0.5f,
            (Screen.height - 170f) * 0.5f,
            360f,
            170f
        );

        GUI.Box(dialogRect, GUIContent.none, dialogBoxStyle);
        GUI.Label(new Rect(dialogRect.x + 20f, dialogRect.y + 18f, dialogRect.width - 40f, 32f), "Login Failed", dialogTitleStyle);
        GUI.Label(new Rect(dialogRect.x + 28f, dialogRect.y + 62f, dialogRect.width - 56f, 42f), loginErrorMessage, dialogTextStyle);

        if (GUI.Button(new Rect(dialogRect.x + 120f, dialogRect.y + 118f, 120f, 34f), "OK"))
        {
            PlayClickSound();
            CloseLoginErrorDialog();
        }
    }

    // Fetches and draws the rows for the active ranking category.
    private void DrawRankingRows()
    {
        List<ChallengeRecord> records;

        if (rankingView == RankingView.Deaths)
        {
            records = LocalGameDatabase.GetDeathRanking(RankingLimit);
            DrawTableHeader("Deaths");
        }
        else if (rankingView == RankingView.ClearTime)
        {
            records = LocalGameDatabase.GetTimeRanking(RankingLimit);
            DrawTableHeader("Clear Time");
        }
        else
        {
            records = LocalGameDatabase.GetDoubleJumpRanking(RankingLimit);
            DrawTableHeader("Double Jumps");
        }

        GUILayout.Space(2f);

        if (records.Count == 0)
        {
            GUILayout.Label("No challenge records yet.", tableEmptyStyle, GUILayout.Height(42f));
            return;
        }

        for (int i = 0; i < records.Count; i++)
        {
            ChallengeRecord record = records[i];
            string value;

            if (rankingView == RankingView.Deaths)
            {
                value = record.totalDeaths.ToString();
            }
            else if (rankingView == RankingView.ClearTime)
            {
                value = GameStatsTracker.FormatTime(record.clearTimeSeconds);
            }
            else
            {
                value = record.doubleJumpUses.ToString();
            }

            DrawTableRow(i + 1, record.challengerName, value, record.completedAtBeijing, i % 2 == 1);
        }
    }

    // Draws the ranking table header.
    private void DrawTableHeader(string valueTitle)
    {
        GUILayout.BeginHorizontal(tableHeaderStyle, GUILayout.Height(34f));
        GUILayout.Label("Rank", tableHeaderStyle, GUILayout.Width(58f));
        GUILayout.Label("Name", tableHeaderStyle, GUILayout.Width(150f));
        GUILayout.Label(valueTitle, tableHeaderStyle, GUILayout.Width(110f));
        GUILayout.Label("Beijing Time", tableHeaderStyle);
        GUILayout.EndHorizontal();
    }

    // Draws one ranking table row.
    private void DrawTableRow(int rank, string challengerName, string value, string completedAtBeijing, bool useAltStyle)
    {
        GUIStyle rowStyle = useAltStyle ? tableAltCellStyle : tableCellStyle;

        GUILayout.BeginHorizontal(rowStyle, GUILayout.Height(34f));
        GUILayout.Label(rank.ToString(), rowStyle, GUILayout.Width(58f));
        GUILayout.Label(challengerName, rowStyle, GUILayout.Width(150f));
        GUILayout.Label(value, rowStyle, GUILayout.Width(110f));
        GUILayout.Label(completedAtBeijing, rowStyle);
        GUILayout.EndHorizontal();
    }

    // Lazily builds GUI styles and backing textures.
    private void EnsureStyles()
    {
        if (titleStyle != null)
        {
            return;
        }

        panelTexture = CreateTexture(new Color(1f, 0.97f, 0.84f, 0.78f));
        dialogTexture = CreateTexture(new Color(0.08f, 0.09f, 0.08f, 0.9f));
        tableHeaderTexture = CreateTexture(new Color(0.25f, 0.34f, 0.26f, 0.86f));
        tableRowTexture = CreateTexture(new Color(1f, 1f, 1f, 0.62f));
        tableAltRowTexture = CreateTexture(new Color(0.92f, 0.98f, 0.88f, 0.62f));

        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.normal.background = panelTexture;

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.font = Font.CreateDynamicFontFromOSFont(new string[] { "Georgia", "Times New Roman" }, 42);
        titleStyle.fontSize = 42;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.08f, 0.16f, 0.09f);

        titleShadowStyle = new GUIStyle(titleStyle);
        titleShadowStyle.normal.textColor = new Color(1f, 0.98f, 0.84f, 0.72f);

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 15;
        labelStyle.normal.textColor = new Color(0.13f, 0.16f, 0.12f);

        messageStyle = new GUIStyle(labelStyle);
        messageStyle.normal.textColor = new Color(0.55f, 0.25f, 0.04f);

        tableHeaderStyle = new GUIStyle(GUI.skin.label);
        tableHeaderStyle.fontSize = 15;
        tableHeaderStyle.fontStyle = FontStyle.Bold;
        tableHeaderStyle.alignment = TextAnchor.MiddleLeft;
        tableHeaderStyle.padding = new RectOffset(10, 10, 7, 7);
        tableHeaderStyle.normal.textColor = Color.white;
        tableHeaderStyle.normal.background = tableHeaderTexture;

        tableCellStyle = new GUIStyle(GUI.skin.label);
        tableCellStyle.fontSize = 15;
        tableCellStyle.alignment = TextAnchor.MiddleLeft;
        tableCellStyle.padding = new RectOffset(10, 10, 7, 7);
        tableCellStyle.clipping = TextClipping.Clip;
        tableCellStyle.normal.textColor = new Color(0.12f, 0.14f, 0.12f);
        tableCellStyle.normal.background = tableRowTexture;

        tableAltCellStyle = new GUIStyle(tableCellStyle);
        tableAltCellStyle.normal.background = tableAltRowTexture;

        tableEmptyStyle = new GUIStyle(tableCellStyle);
        tableEmptyStyle.alignment = TextAnchor.MiddleCenter;

        closeButtonStyle = new GUIStyle(GUI.skin.button);
        closeButtonStyle.fontSize = 18;
        closeButtonStyle.fontStyle = FontStyle.Bold;
        closeButtonStyle.normal.textColor = new Color(0.13f, 0.16f, 0.12f);

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
        dialogTextStyle.wordWrap = true;
        dialogTextStyle.normal.textColor = Color.white;

        demoMessageStyle = new GUIStyle(labelStyle);
        demoMessageStyle.alignment = TextAnchor.MiddleCenter;
        demoMessageStyle.fontStyle = FontStyle.Bold;
    }

    // Creates a 1x1 texture used as a GUI background.
    private static Texture2D CreateTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    // Draws the configured background texture as a cover image.
    private void DrawBackground()
    {
        if (backgroundTexture == null)
        {
            return;
        }

        Rect targetRect = GetCoverRect(backgroundTexture.width, backgroundTexture.height);
        GUI.DrawTexture(targetRect, backgroundTexture, ScaleMode.StretchToFill);
    }

    // Calculates a cover-fit rectangle for the background image.
    private static Rect GetCoverRect(float textureWidth, float textureHeight)
    {
        float screenRatio = Screen.width / (float)Screen.height;
        float textureRatio = textureWidth / textureHeight;

        if (textureRatio > screenRatio)
        {
            float width = Screen.height * textureRatio;
            return new Rect((Screen.width - width) * 0.5f, 0f, width, Screen.height);
        }

        float height = Screen.width / textureRatio;
        return new Rect(0f, (Screen.height - height) * 0.5f, Screen.width, height);
    }

    // Converts a login result into a player-facing message.
    private static string GetLoginMessage(LoginResult result)
    {
        if (result == LoginResult.Success)
        {
            return "Login successful.";
        }

        if (result == LoginResult.EmptyUserName)
        {
            return "User name is required.";
        }

        if (result == LoginResult.EmptyPassword)
        {
            return "Password is required.";
        }

        if (result == LoginResult.UserNotFound)
        {
            return "User not found.";
        }

        return "Wrong password.";
    }

    // Converts a registration result into a player-facing message.
    private static string GetRegisterMessage(RegisterResult result)
    {
        if (result == RegisterResult.Success)
        {
            return "Register successful. You are logged in.";
        }

        if (result == RegisterResult.EmptyUserName)
        {
            return "User name is required.";
        }

        if (result == RegisterResult.EmptyPassword)
        {
            return "Password is required.";
        }

        return "User name already exists.";
    }
}
