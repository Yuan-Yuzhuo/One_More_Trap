using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenuController : MonoBehaviour
{
    private const int FirstChallengeSceneIndex = 1;
    private const int RankingLimit = 10;
    private const string DemoVideoResourceName = "Video Project";
    private const string MenuBgmResourceName = "candidate_1";
    private const string MenuBgmFallbackResourceName = "cadidate_1";
    private const string MenuBgmMutedPrefKey = "MainMenu_BgmMuted";
    private const float MenuBgmVolume = 0.22f;
    private const float MenuClickVolume = 1f;
    private const float MenuMuteButtonSize = 40f;
    private const float MenuMuteButtonMargin = 18f;

    private AudioSource menuAudioSource;
    private AudioSource menuBgmAudioSource;
    private AudioClip clickClip;
    private AudioClip menuBgmClip;
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
    private bool hasLoadedMenuBgmMutePreference = false;
    private bool isMenuBgmMuted = false;

    private string userName = "";
    private string password = "";
    private string message = "";
    private bool showLoginErrorDialog = false;
    private string loginErrorMessage = "";

    private GUIStyle titleStyle;
    private GUIStyle titleShadowStyle;
    private GUIStyle subtitleStyle;
    private GUIStyle labelStyle;
    private GUIStyle messageStyle;
    private GUIStyle statusStyle;
    private GUIStyle panelStyle;
    private GUIStyle primaryButtonStyle;
    private GUIStyle secondaryButtonStyle;
    private GUIStyle tertiaryButtonStyle;
    private GUIStyle textFieldStyle;
    private GUIStyle tableHeaderStyle;
    private GUIStyle tableCellStyle;
    private GUIStyle tableAltCellStyle;
    private GUIStyle tableEmptyStyle;
    private GUIStyle closeButtonStyle;
    private GUIStyle dialogBoxStyle;
    private GUIStyle dialogTitleStyle;
    private GUIStyle dialogTextStyle;
    private GUIStyle demoMessageStyle;
    private GUIStyle iconButtonStyle;
    private Texture2D panelTexture;
    private Texture2D dialogTexture;
    private Texture2D screenShadeTexture;
    private Texture2D titleGlowTexture;
    private Texture2D particleTexture;
    private Texture2D leafTexture;
    private Texture2D primaryButtonTexture;
    private Texture2D primaryButtonHoverTexture;
    private Texture2D primaryButtonActiveTexture;
    private Texture2D secondaryButtonTexture;
    private Texture2D secondaryButtonHoverTexture;
    private Texture2D secondaryButtonActiveTexture;
    private Texture2D tertiaryButtonTexture;
    private Texture2D tertiaryButtonHoverTexture;
    private Texture2D tertiaryButtonActiveTexture;
    private Texture2D textFieldTexture;
    private Texture2D tableHeaderTexture;
    private Texture2D tableRowTexture;
    private Texture2D tableAltRowTexture;
    private Texture2D iconButtonTexture;
    private Texture2D iconButtonHoverTexture;
    private Texture2D iconButtonActiveTexture;
    private Texture2D speakerIconTexture;
    private Texture2D mutedSpeakerIconTexture;

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

    // Lazily creates menu audio, loads click feedback, and starts low-volume BGM.
    private void EnsureAudio()
    {
        if (menuAudioSource == null)
        {
            menuAudioSource = gameObject.AddComponent<AudioSource>();
            menuAudioSource.playOnAwake = false;
            menuAudioSource.spatialBlend = 0f;
        }

        if (menuBgmAudioSource == null)
        {
            menuBgmAudioSource = gameObject.AddComponent<AudioSource>();
            menuBgmAudioSource.playOnAwake = false;
            menuBgmAudioSource.loop = true;
            menuBgmAudioSource.volume = MenuBgmVolume;
            menuBgmAudioSource.spatialBlend = 0f;
        }

        if (clickClip == null)
        {
            clickClip = Resources.Load<AudioClip>("click");
        }

        if (menuBgmClip == null)
        {
            menuBgmClip = Resources.Load<AudioClip>(MenuBgmResourceName);

            if (menuBgmClip == null)
            {
                menuBgmClip = Resources.Load<AudioClip>(MenuBgmFallbackResourceName);
            }
        }

        EnsureMenuBgmMutePreference();
        PlayMenuBgm();
    }

    // Loads the saved BGM mute preference once per menu controller.
    private void EnsureMenuBgmMutePreference()
    {
        if (hasLoadedMenuBgmMutePreference)
        {
            return;
        }

        isMenuBgmMuted = PlayerPrefs.GetInt(MenuBgmMutedPrefKey, 0) == 1;
        hasLoadedMenuBgmMutePreference = true;
    }

    // Plays the menu click sound if it is available.
    private void PlayClickSound()
    {
        EnsureAudio();

        if (clickClip != null)
        {
            menuAudioSource.PlayOneShot(clickClip, MenuClickVolume);
        }
    }

    // Keeps the main menu background music looping quietly under UI sounds.
    private void PlayMenuBgm()
    {
        if (menuBgmAudioSource == null || menuBgmClip == null)
        {
            return;
        }

        menuBgmAudioSource.volume = MenuBgmVolume;
        menuBgmAudioSource.mute = isMenuBgmMuted;

        if (menuBgmAudioSource.clip != menuBgmClip)
        {
            menuBgmAudioSource.clip = menuBgmClip;
        }

        if (!menuBgmAudioSource.isPlaying)
        {
            menuBgmAudioSource.Play();
        }
    }

    // Toggles only the menu BGM, leaving click feedback audible.
    private void SetMenuBgmMuted(bool muted)
    {
        isMenuBgmMuted = muted;
        PlayerPrefs.SetInt(MenuBgmMutedPrefKey, isMenuBgmMuted ? 1 : 0);
        PlayerPrefs.Save();

        if (menuBgmAudioSource != null)
        {
            menuBgmAudioSource.mute = isMenuBgmMuted;
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

        if (screenShadeTexture != null)
        {
            Destroy(screenShadeTexture);
        }

        if (titleGlowTexture != null)
        {
            Destroy(titleGlowTexture);
        }

        if (particleTexture != null)
        {
            Destroy(particleTexture);
        }

        if (leafTexture != null)
        {
            Destroy(leafTexture);
        }

        if (primaryButtonTexture != null)
        {
            Destroy(primaryButtonTexture);
        }

        if (primaryButtonHoverTexture != null)
        {
            Destroy(primaryButtonHoverTexture);
        }

        if (primaryButtonActiveTexture != null)
        {
            Destroy(primaryButtonActiveTexture);
        }

        if (secondaryButtonTexture != null)
        {
            Destroy(secondaryButtonTexture);
        }

        if (secondaryButtonHoverTexture != null)
        {
            Destroy(secondaryButtonHoverTexture);
        }

        if (secondaryButtonActiveTexture != null)
        {
            Destroy(secondaryButtonActiveTexture);
        }

        if (tertiaryButtonTexture != null)
        {
            Destroy(tertiaryButtonTexture);
        }

        if (tertiaryButtonHoverTexture != null)
        {
            Destroy(tertiaryButtonHoverTexture);
        }

        if (tertiaryButtonActiveTexture != null)
        {
            Destroy(tertiaryButtonActiveTexture);
        }

        if (textFieldTexture != null)
        {
            Destroy(textFieldTexture);
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

        if (iconButtonTexture != null)
        {
            Destroy(iconButtonTexture);
        }

        if (iconButtonHoverTexture != null)
        {
            Destroy(iconButtonHoverTexture);
        }

        if (iconButtonActiveTexture != null)
        {
            Destroy(iconButtonActiveTexture);
        }

        if (speakerIconTexture != null)
        {
            Destroy(speakerIconTexture);
        }

        if (mutedSpeakerIconTexture != null)
        {
            Destroy(mutedSpeakerIconTexture);
        }
    }

    // Draws the current main menu view and modal overlays.
    private void OnGUI()
    {
        EnsureAudio();

        EnsureStyles();
        DrawBackground();
        DrawAmbientEffects();
        CapturePendingInputKey();

        Rect panelRect = GetMenuPanelRect();
        DrawTitle(panelRect);

        GUI.Box(panelRect, GUIContent.none, panelStyle);

        if (currentView == MenuView.Ranking || currentView == MenuView.Configuration || currentView == MenuView.Demo)
        {
            Rect closeRect = new Rect(panelRect.xMax - 46f, panelRect.y + 14f, 32f, 32f);
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

        Rect contentRect = new Rect(panelRect.x + 30f, panelRect.y + 28f, panelRect.width - 60f, panelRect.height - 54f);
        GUILayout.BeginArea(contentRect);

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
            DrawDemo(contentRect.width, contentRect.height);
        }

        GUILayout.EndArea();

        if (showLoginErrorDialog)
        {
            DrawLoginErrorDialog();
        }

        DrawMenuMuteButton();
    }

    // Positions compact menu cards so the background character and scenery remain visible.
    private Rect GetMenuPanelRect()
    {
        bool useLargePanel = currentView == MenuView.Ranking || currentView == MenuView.Demo;

        float targetWidth = useLargePanel ? 720f : 370f;
        float targetHeight = useLargePanel ? 540f : 470f;
        float panelWidth = Mathf.Min(targetWidth, Screen.width - 32f);
        float panelHeight = Mathf.Min(targetHeight, Screen.height - 36f);

        float x;
        if (useLargePanel || Screen.width < 820f)
        {
            x = (Screen.width - panelWidth) * 0.5f;
        }
        else
        {
            x = Screen.width - panelWidth - Mathf.Max(54f, Screen.width * 0.075f);
        }

        float desiredMinY = Screen.width < 820f ? 96f : 62f;
        float maxY = Mathf.Max(18f, Screen.height - panelHeight - 18f);
        float minY = Mathf.Min(desiredMinY, maxY);
        float y = Mathf.Clamp((Screen.height - panelHeight) * 0.52f, minY, maxY);
        return new Rect(x, y, panelWidth, panelHeight);
    }

    // Draws a corner icon button for toggling menu BGM.
    private void DrawMenuMuteButton()
    {
        EnsureMuteButtonIcons();

        Texture2D icon = isMenuBgmMuted ? mutedSpeakerIconTexture : speakerIconTexture;
        bool hasPanelCloseButton =
            currentView == MenuView.Ranking ||
            currentView == MenuView.Configuration ||
            currentView == MenuView.Demo;

        Rect buttonRect = new Rect(
            hasPanelCloseButton
                ? MenuMuteButtonMargin
                : Screen.width - MenuMuteButtonSize - MenuMuteButtonMargin,
            MenuMuteButtonMargin,
            MenuMuteButtonSize,
            MenuMuteButtonSize
        );

        if (GUI.Button(buttonRect, new GUIContent(icon), iconButtonStyle))
        {
            PlayClickSound();
            SetMenuBgmMuted(!isMenuBgmMuted);
        }
    }

    // Draws the fantasy title treatment over the open background sky.
    private void DrawTitle(Rect panelRect)
    {
        float titleWidth = Mathf.Min(620f, Screen.width - 40f);
        float titleX = Screen.width >= 820f ? 42f : (Screen.width - titleWidth) * 0.5f;
        float titleY = Mathf.Max(20f, panelRect.y - 116f);
        Rect glowRect = new Rect(titleX - 46f, titleY - 30f, titleWidth + 92f, 140f);
        Rect titleRect = new Rect(titleX, titleY, titleWidth, 70f);
        Rect shadowRect = new Rect(titleRect.x + 3f, titleRect.y + 4f, titleRect.width, titleRect.height);
        Rect subtitleRect = new Rect(titleRect.x + 8f, titleRect.yMax - 2f, titleRect.width - 16f, 28f);

        Color oldColor = GUI.color;
        GUI.color = new Color(1f, 0.92f, 0.52f, 0.36f);
        GUI.DrawTexture(glowRect, titleGlowTexture, ScaleMode.StretchToFill);
        GUI.color = oldColor;

        GUI.Label(shadowRect, "One More Trap", titleShadowStyle);
        GUI.Label(titleRect, "One More Trap", titleStyle);
        GUI.Label(subtitleRect, "A cozy adventure with suspiciously honest platforms", subtitleStyle);
    }

    // Draws login controls and navigation buttons.
    private void DrawLogin()
    {
        GUILayout.Label("Begin Adventure", labelStyle);
        GUILayout.Space(8f);

        GUI.enabled = LocalGameDatabase.IsLoggedIn;
        if (GUILayout.Button("Start Challenge", primaryButtonStyle, GUILayout.Height(58f)))
        {
            PlayClickSound();
            GameStatsTracker.StartChallenge();
            SceneTransitionController.LoadScene(FirstChallengeSceneIndex);
        }
        GUI.enabled = true;

        GUILayout.Space(14f);
        DrawAccountFields();

        GUILayout.Space(12f);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Login", secondaryButtonStyle, GUILayout.Height(42f)))
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

        if (GUILayout.Button("Register", secondaryButtonStyle, GUILayout.Height(42f)))
        {
            PlayClickSound();
            currentView = MenuView.Register;
            message = "";
            CloseLoginErrorDialog();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(14f);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Rankings", tertiaryButtonStyle, GUILayout.Height(36f)))
        {
            PlayClickSound();

            currentView = MenuView.Ranking;
            message = "";
            pendingInputAction = null;
            CloseLoginErrorDialog();
        }

        if (GUILayout.Button("Settings", tertiaryButtonStyle, GUILayout.Height(36f)))
        {
            PlayClickSound();

            currentView = MenuView.Configuration;
            message = "";
            pendingInputAction = null;
            CloseLoginErrorDialog();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(7f);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("View Demo", tertiaryButtonStyle, GUILayout.Height(36f)))
        {
            PlayClickSound();

            currentView = MenuView.Demo;
            message = "";
            pendingInputAction = null;
            CloseLoginErrorDialog();
            StartDemoVideo();
        }

        if (GUILayout.Button("Quit Game", tertiaryButtonStyle, GUILayout.Height(36f)))
        {
            PlayClickSound();
            QuitGame();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(12f);
        DrawStatus();
    }

    // Draws registration controls.
    private void DrawRegister()
    {
        GUILayout.Label("Create Account", labelStyle);
        GUILayout.Space(10f);
        DrawAccountFields();

        GUILayout.Space(14f);

        if (GUILayout.Button("Register", primaryButtonStyle, GUILayout.Height(50f)))
        {
            PlayClickSound();

            RegisterResult result = LocalGameDatabase.Register(userName, password);
            message = GetRegisterMessage(result);

            if (result == RegisterResult.Success)
            {
                currentView = MenuView.Login;
            }
        }

        GUILayout.Space(10f);

        if (GUILayout.Button("Back To Login", tertiaryButtonStyle, GUILayout.Height(36f)))
        {
            PlayClickSound();

            currentView = MenuView.Login;
            message = "";
            CloseLoginErrorDialog();
        }

        GUILayout.Space(12f);
        DrawStatus();
    }

    // Draws ranking category tabs and rows.
    private void DrawRanking()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(rankingView == RankingView.Deaths, "Deaths", tertiaryButtonStyle, GUILayout.Height(36f)))
        {
            if (rankingView != RankingView.Deaths)
            {
                PlayClickSound();
            }

            rankingView = RankingView.Deaths;
        }
        if (GUILayout.Toggle(rankingView == RankingView.ClearTime, "Clear Time", tertiaryButtonStyle, GUILayout.Height(36f)))
        {
            if (rankingView != RankingView.ClearTime)
            {
                PlayClickSound();
            }

            rankingView = RankingView.ClearTime;
        }
        if (GUILayout.Toggle(rankingView == RankingView.DoubleJumps, "Double Jumps", tertiaryButtonStyle, GUILayout.Height(36f)))
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

        if (GUILayout.Button("Reset Defaults", secondaryButtonStyle, GUILayout.Height(38f)))
        {
            PlayClickSound();
            PlayerInputConfig.ResetDefaults();
            pendingInputAction = null;
        }

        if (GUILayout.Button("Back To Login", tertiaryButtonStyle, GUILayout.Height(36f)))
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
            if (GUILayout.Button("Back To Login", tertiaryButtonStyle, GUILayout.Height(36f)))
            {
                PlayClickSound();
                currentView = MenuView.Login;
                StopDemoVideo();
            }

            return;
        }

        const float buttonHeight = 36f;
        const float buttonWidth = 160f;
        const float buttonGap = 18f;
        const float videoVerticalOffset = 24f;

        float availableVideoHeight = Mathf.Max(120f, contentHeight - buttonHeight - buttonGap);
        float videoWidth = Mathf.Min(contentWidth, availableVideoHeight * 16f / 9f);
        float videoHeight = videoWidth * 9f / 16f;

        if (videoHeight > availableVideoHeight)
        {
            videoHeight = availableVideoHeight;
            videoWidth = videoHeight * 16f / 9f;
        }

        float videoX = (contentWidth - videoWidth) * 0.5f;
        float maxVideoY = Mathf.Max(0f, availableVideoHeight - videoHeight);
        float videoY = Mathf.Min(
            maxVideoY,
            Mathf.Max(0f, (availableVideoHeight - videoHeight) * 0.5f + videoVerticalOffset)
        );
        Rect videoRect = new Rect(videoX, videoY, videoWidth, videoHeight);

        GUI.DrawTexture(videoRect, demoRenderTexture, ScaleMode.ScaleToFit, false);

        Rect backButtonRect = new Rect(
            (contentWidth - buttonWidth) * 0.5f,
            availableVideoHeight + buttonGap,
            buttonWidth,
            buttonHeight
        );

        if (GUI.Button(backButtonRect, "Back To Login", tertiaryButtonStyle))
        {
            PlayClickSound();
            currentView = MenuView.Login;
            StopDemoVideo();
        }
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

        if (GUILayout.Button(buttonText, secondaryButtonStyle, GUILayout.Height(36f)))
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
        userName = GUILayout.TextField(userName, textFieldStyle, GUILayout.Height(34f));

        GUILayout.Space(8f);

        GUILayout.Label("Password", labelStyle);
        password = GUILayout.PasswordField(password, '*', textFieldStyle, GUILayout.Height(34f));
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

        GUILayout.Label(status, statusStyle);

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

        if (GUI.Button(new Rect(dialogRect.x + 120f, dialogRect.y + 118f, 120f, 34f), "OK", secondaryButtonStyle))
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

        panelTexture = CreatePanelTexture();
        dialogTexture = CreateTexture(new Color(0.08f, 0.09f, 0.08f, 0.9f));
        screenShadeTexture = CreateTexture(new Color(0.05f, 0.12f, 0.08f, 0.16f));
        titleGlowTexture = CreateSoftCircleTexture(128, new Color(1f, 0.9f, 0.45f, 1f));
        particleTexture = CreateSoftCircleTexture(24, new Color(1f, 0.96f, 0.65f, 1f));
        leafTexture = CreateLeafTexture();
        primaryButtonTexture = CreateWoodButtonTexture(new Color(0.62f, 0.33f, 0.13f, 1f), new Color(0.94f, 0.67f, 0.31f, 1f), 1.08f);
        primaryButtonHoverTexture = CreateWoodButtonTexture(new Color(0.72f, 0.4f, 0.17f, 1f), new Color(1f, 0.76f, 0.38f, 1f), 1.18f);
        primaryButtonActiveTexture = CreateWoodButtonTexture(new Color(0.46f, 0.24f, 0.1f, 1f), new Color(0.78f, 0.49f, 0.2f, 1f), 0.9f);
        secondaryButtonTexture = CreateWoodButtonTexture(new Color(0.43f, 0.26f, 0.13f, 1f), new Color(0.78f, 0.55f, 0.3f, 1f), 0.92f);
        secondaryButtonHoverTexture = CreateWoodButtonTexture(new Color(0.53f, 0.32f, 0.16f, 1f), new Color(0.91f, 0.66f, 0.36f, 1f), 1.03f);
        secondaryButtonActiveTexture = CreateWoodButtonTexture(new Color(0.34f, 0.2f, 0.1f, 1f), new Color(0.66f, 0.42f, 0.21f, 1f), 0.84f);
        tertiaryButtonTexture = CreateWoodButtonTexture(new Color(0.28f, 0.38f, 0.24f, 1f), new Color(0.62f, 0.76f, 0.45f, 1f), 0.78f);
        tertiaryButtonHoverTexture = CreateWoodButtonTexture(new Color(0.34f, 0.46f, 0.28f, 1f), new Color(0.74f, 0.87f, 0.55f, 1f), 0.88f);
        tertiaryButtonActiveTexture = CreateWoodButtonTexture(new Color(0.2f, 0.29f, 0.18f, 1f), new Color(0.48f, 0.62f, 0.34f, 1f), 0.68f);
        textFieldTexture = CreateTextFieldTexture();
        tableHeaderTexture = CreateTexture(new Color(0.25f, 0.34f, 0.26f, 0.86f));
        tableRowTexture = CreateTexture(new Color(1f, 1f, 1f, 0.62f));
        tableAltRowTexture = CreateTexture(new Color(0.92f, 0.98f, 0.88f, 0.62f));
        iconButtonTexture = CreateIconButtonTexture(new Color(1f, 0.96f, 0.76f, 0.92f));
        iconButtonHoverTexture = CreateIconButtonTexture(new Color(1f, 0.99f, 0.86f, 0.98f));
        iconButtonActiveTexture = CreateIconButtonTexture(new Color(0.86f, 0.94f, 0.72f, 0.98f));

        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.normal.background = panelTexture;
        panelStyle.border = new RectOffset(22, 22, 22, 22);
        panelStyle.padding = new RectOffset(0, 0, 0, 0);

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.font = Font.CreateDynamicFontFromOSFont(new string[] { "Georgia", "Garamond", "Times New Roman" }, 58);
        titleStyle.fontSize = Screen.width < 820f ? 42 : 58;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = Screen.width < 820f ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
        titleStyle.normal.textColor = new Color(0.15f, 0.28f, 0.11f);

        titleShadowStyle = new GUIStyle(titleStyle);
        titleShadowStyle.normal.textColor = new Color(1f, 0.95f, 0.62f, 0.92f);

        subtitleStyle = new GUIStyle(GUI.skin.label);
        subtitleStyle.fontSize = Screen.width < 820f ? 12 : 15;
        subtitleStyle.fontStyle = FontStyle.Bold;
        subtitleStyle.alignment = titleStyle.alignment;
        subtitleStyle.normal.textColor = new Color(0.96f, 0.86f, 0.5f, 0.95f);

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.font = Font.CreateDynamicFontFromOSFont(new string[] { "Verdana", "Arial" }, 15);
        labelStyle.fontSize = 15;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = new Color(0.18f, 0.16f, 0.1f);

        messageStyle = new GUIStyle(labelStyle);
        messageStyle.normal.textColor = new Color(0.55f, 0.25f, 0.04f);
        messageStyle.wordWrap = true;

        statusStyle = new GUIStyle(labelStyle);
        statusStyle.fontSize = 13;
        statusStyle.fontStyle = FontStyle.Normal;
        statusStyle.alignment = TextAnchor.MiddleCenter;
        statusStyle.wordWrap = true;
        statusStyle.normal.textColor = new Color(0.18f, 0.22f, 0.15f);

        primaryButtonStyle = CreateButtonStyle(primaryButtonTexture, primaryButtonHoverTexture, primaryButtonActiveTexture, 19, Color.white);
        primaryButtonStyle.fontStyle = FontStyle.Bold;

        secondaryButtonStyle = CreateButtonStyle(secondaryButtonTexture, secondaryButtonHoverTexture, secondaryButtonActiveTexture, 15, new Color(1f, 0.97f, 0.82f));
        tertiaryButtonStyle = CreateButtonStyle(tertiaryButtonTexture, tertiaryButtonHoverTexture, tertiaryButtonActiveTexture, 13, new Color(0.96f, 1f, 0.86f));

        textFieldStyle = new GUIStyle(GUI.skin.textField);
        textFieldStyle.fontSize = 14;
        textFieldStyle.alignment = TextAnchor.MiddleLeft;
        textFieldStyle.padding = new RectOffset(12, 12, 7, 7);
        textFieldStyle.border = new RectOffset(10, 10, 10, 10);
        textFieldStyle.normal.background = textFieldTexture;
        textFieldStyle.focused.background = textFieldTexture;
        textFieldStyle.hover.background = textFieldTexture;
        textFieldStyle.normal.textColor = new Color(0.14f, 0.1f, 0.07f);
        textFieldStyle.focused.textColor = textFieldStyle.normal.textColor;

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

        closeButtonStyle = new GUIStyle(tertiaryButtonStyle);
        closeButtonStyle.fontSize = 16;
        closeButtonStyle.fontStyle = FontStyle.Bold;

        iconButtonStyle = new GUIStyle(GUI.skin.button);
        iconButtonStyle.alignment = TextAnchor.MiddleCenter;
        iconButtonStyle.padding = new RectOffset(7, 7, 7, 7);
        iconButtonStyle.margin = new RectOffset(0, 0, 0, 0);
        iconButtonStyle.normal.background = iconButtonTexture;
        iconButtonStyle.hover.background = iconButtonHoverTexture;
        iconButtonStyle.active.background = iconButtonActiveTexture;
        iconButtonStyle.focused.background = iconButtonHoverTexture;
        iconButtonStyle.normal.textColor = Color.white;
        iconButtonStyle.hover.textColor = Color.white;
        iconButtonStyle.active.textColor = Color.white;

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

    // Builds a reusable IMGUI button style with generated fantasy textures.
    private static GUIStyle CreateButtonStyle(Texture2D normal, Texture2D hover, Texture2D active, int fontSize, Color textColor)
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = fontSize;
        style.alignment = TextAnchor.MiddleCenter;
        style.padding = new RectOffset(12, 12, 8, 9);
        style.margin = new RectOffset(3, 3, 3, 3);
        style.border = new RectOffset(18, 18, 16, 18);
        style.normal.background = normal;
        style.hover.background = hover;
        style.active.background = active;
        style.focused.background = hover;
        style.onNormal.background = active;
        style.onHover.background = hover;
        style.onActive.background = active;
        style.onFocused.background = hover;
        style.normal.textColor = textColor;
        style.hover.textColor = Color.white;
        style.active.textColor = new Color(0.94f, 0.84f, 0.58f);
        style.focused.textColor = Color.white;
        style.onNormal.textColor = Color.white;
        style.onHover.textColor = Color.white;
        style.onActive.textColor = new Color(0.94f, 0.84f, 0.58f);
        style.onFocused.textColor = Color.white;
        return style;
    }

    // Creates a 1x1 texture used as a GUI background.
    private static Texture2D CreateTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    // Creates the parchment-style menu card with soft edges and a drop shadow.
    private static Texture2D CreatePanelTexture()
    {
        const int textureSize = 96;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color shadow = new Color(0.05f, 0.03f, 0.01f, 0.26f);
        Color border = new Color(0.42f, 0.27f, 0.12f, 0.88f);
        Color innerBorder = new Color(1f, 0.91f, 0.58f, 0.55f);
        Color top = new Color(1f, 0.91f, 0.66f, 0.91f);
        Color bottom = new Color(0.8f, 0.63f, 0.36f, 0.88f);

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                texture.SetPixel(x, y, clear);

                if (IsInsideRoundedRect(x, y, 9f, 11f, 78f, 78f, 14f))
                {
                    texture.SetPixel(x, y, shadow);
                }

                bool outer = IsInsideRoundedRect(x, y, 6f, 5f, 80f, 78f, 13f);
                if (!outer)
                {
                    continue;
                }

                bool inner = IsInsideRoundedRect(x, y, 10f, 9f, 72f, 70f, 10f);
                if (!inner)
                {
                    texture.SetPixel(x, y, border);
                    continue;
                }

                float vertical = Mathf.InverseLerp(9f, 79f, y);
                float grain = Mathf.PerlinNoise(x * 0.18f, y * 0.12f) * 0.055f;
                Color fill = Color.Lerp(top, bottom, vertical + grain);
                texture.SetPixel(x, y, fill);

                bool highlight = IsInsideRoundedRect(x, y, 12f, 11f, 68f, 66f, 8f) &&
                    !IsInsideRoundedRect(x, y, 14f, 13f, 64f, 62f, 7f);
                if (highlight)
                {
                    texture.SetPixel(x, y, innerBorder);
                }
            }
        }

        texture.Apply();
        return texture;
    }

    // Creates a sliced wooden button texture with grain, bevel, and shadow.
    private static Texture2D CreateWoodButtonTexture(Color baseColor, Color highlightColor, float brightness)
    {
        const int width = 96;
        const int height = 48;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color shadow = new Color(0.04f, 0.02f, 0.01f, 0.34f);
        Color edge = new Color(0.18f, 0.09f, 0.03f, 1f);
        Color shine = new Color(1f, 0.92f, 0.58f, 0.35f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, clear);

                if (IsInsideRoundedRect(x, y, 5f, 7f, 86f, 36f, 12f))
                {
                    texture.SetPixel(x, y, shadow);
                }

                bool body = IsInsideRoundedRect(x, y, 3f, 3f, 88f, 36f, 12f);
                if (!body)
                {
                    continue;
                }

                bool inner = IsInsideRoundedRect(x, y, 7f, 7f, 80f, 28f, 9f);
                if (!inner)
                {
                    texture.SetPixel(x, y, edge);
                    continue;
                }

                float vertical = Mathf.InverseLerp(7f, 35f, y);
                float grain = Mathf.PerlinNoise(x * 0.08f, y * 0.42f) * 0.16f;
                float streak = Mathf.Sin((x + y * 0.35f) * 0.24f) * 0.045f;
                Color color = Color.Lerp(highlightColor, baseColor, vertical + grain + streak);
                color = new Color(
                    Mathf.Clamp01(color.r * brightness),
                    Mathf.Clamp01(color.g * brightness),
                    Mathf.Clamp01(color.b * brightness),
                    color.a
                );
                texture.SetPixel(x, y, color);

                if (y >= 8 && y <= 12 && x >= 14 && x <= 82)
                {
                    texture.SetPixel(x, y, Color.Lerp(texture.GetPixel(x, y), shine, 0.45f));
                }
            }
        }

        texture.Apply();
        return texture;
    }

    // Creates a warm input field texture that fits the fantasy card.
    private static Texture2D CreateTextFieldTexture()
    {
        const int width = 64;
        const int height = 40;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color border = new Color(0.4f, 0.25f, 0.11f, 0.9f);
        Color fill = new Color(1f, 0.94f, 0.72f, 0.88f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, clear);

                bool outer = IsInsideRoundedRect(x, y, 2f, 3f, 60f, 32f, 8f);
                if (!outer)
                {
                    continue;
                }

                bool inner = IsInsideRoundedRect(x, y, 5f, 6f, 54f, 26f, 6f);
                texture.SetPixel(x, y, inner ? fill : border);
            }
        }

        texture.Apply();
        return texture;
    }

    // Creates a soft circular texture used for bloom and drifting motes.
    private static Texture2D CreateSoftCircleTexture(int textureSize, Color tint)
    {
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
        float radius = textureSize * 0.5f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                float alpha = Mathf.SmoothStep(1f, 0f, distance);
                texture.SetPixel(x, y, new Color(tint.r, tint.g, tint.b, tint.a * alpha));
            }
        }

        texture.Apply();
        return texture;
    }

    // Creates a small leaf texture for lightweight ambient motion.
    private static Texture2D CreateLeafTexture()
    {
        const int textureSize = 32;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color leaf = new Color(0.34f, 0.58f, 0.18f, 0.85f);
        Color vein = new Color(0.78f, 0.86f, 0.34f, 0.9f);

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                texture.SetPixel(x, y, clear);

                float nx = (x - 16f - (y - 16f) * 0.38f) / 12f;
                float ny = (y - 16f) / 6f;
                if (nx * nx + ny * ny <= 1f)
                {
                    texture.SetPixel(x, y, leaf);
                }

                if (Mathf.Abs(y - 16f) <= 1f && x >= 8 && x <= 24)
                {
                    texture.SetPixel(x, y, vein);
                }
            }
        }

        texture.Apply();
        return texture;
    }

    // Tests whether a generated texture pixel falls inside a rounded rectangle.
    private static bool IsInsideRoundedRect(float x, float y, float left, float top, float width, float height, float radius)
    {
        if (x < left || y < top || x >= left + width || y >= top + height)
        {
            return false;
        }

        float closestX = Mathf.Clamp(x, left + radius, left + width - radius - 1f);
        float closestY = Mathf.Clamp(y, top + radius, top + height - radius - 1f);
        float dx = x - closestX;
        float dy = y - closestY;
        return dx * dx + dy * dy <= radius * radius;
    }

    // Creates a high-contrast square backing for icon-only menu controls.
    private static Texture2D CreateIconButtonTexture(Color fillColor)
    {
        const int textureSize = 32;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color borderColor = new Color(0.08f, 0.16f, 0.09f, 0.75f);
        Color shadowColor = new Color(0f, 0f, 0f, 0.22f);

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        FillRect(texture, 4, 5, 25, 24, shadowColor);
        FillRect(texture, 3, 3, 26, 25, fillColor);
        FillRect(texture, 3, 3, 26, 2, borderColor);
        FillRect(texture, 3, 26, 26, 2, borderColor);
        FillRect(texture, 3, 3, 2, 25, borderColor);
        FillRect(texture, 27, 3, 2, 25, borderColor);

        texture.Apply();
        return texture;
    }

    // Builds speaker icons at runtime so the mute control does not depend on text.
    private void EnsureMuteButtonIcons()
    {
        if (speakerIconTexture == null)
        {
            speakerIconTexture = CreateSpeakerIcon(false);
        }

        if (mutedSpeakerIconTexture == null)
        {
            mutedSpeakerIconTexture = CreateSpeakerIcon(true);
        }
    }

    // Creates a compact speaker or muted-speaker texture for the menu corner button.
    private static Texture2D CreateSpeakerIcon(bool muted)
    {
        const int iconSize = 64;
        Texture2D texture = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
        texture.name = muted ? "MenuMutedSpeakerIcon" : "MenuSpeakerIcon";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color speakerColor = new Color(0.02f, 0.08f, 0.04f, 1f);
        Color muteColor = new Color(0.72f, 0.08f, 0.04f, 1f);

        for (int y = 0; y < iconSize; y++)
        {
            for (int x = 0; x < iconSize; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        FillRect(texture, 11, 24, 13, 16, speakerColor);
        FillTriangle(texture, new Vector2(24f, 24f), new Vector2(40f, 12f), new Vector2(40f, 52f), speakerColor);

        if (muted)
        {
            DrawThickLine(texture, new Vector2(46f, 22f), new Vector2(58f, 42f), muteColor, 5f);
            DrawThickLine(texture, new Vector2(58f, 22f), new Vector2(46f, 42f), muteColor, 5f);
        }
        else
        {
            DrawThickLine(texture, new Vector2(44f, 25f), new Vector2(51f, 19f), speakerColor, 4f);
            DrawThickLine(texture, new Vector2(44f, 39f), new Vector2(51f, 45f), speakerColor, 4f);
            DrawThickLine(texture, new Vector2(50f, 21f), new Vector2(58f, 14f), speakerColor, 3f);
            DrawThickLine(texture, new Vector2(50f, 43f), new Vector2(58f, 50f), speakerColor, 3f);
        }

        texture.Apply();
        return texture;
    }

    // Fills a rectangle on a generated icon texture.
    private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int py = y; py < y + height; py++)
        {
            for (int px = x; px < x + width; px++)
            {
                texture.SetPixel(px, py, color);
            }
        }
    }

    // Fills a triangle on a generated icon texture.
    private static void FillTriangle(Texture2D texture, Vector2 a, Vector2 b, Vector2 c, Color color)
    {
        int minX = Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x)));
        int maxX = Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x)));
        int minY = Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y)));
        int maxY = Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y)));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2 point = new Vector2(x + 0.5f, y + 0.5f);

                if (IsPointInsideTriangle(point, a, b, c))
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
    }

    // Draws a thick line segment on a generated icon texture.
    private static void DrawThickLine(Texture2D texture, Vector2 start, Vector2 end, Color color, float thickness)
    {
        int minX = Mathf.FloorToInt(Mathf.Min(start.x, end.x) - thickness);
        int maxX = Mathf.CeilToInt(Mathf.Max(start.x, end.x) + thickness);
        int minY = Mathf.FloorToInt(Mathf.Min(start.y, end.y) - thickness);
        int maxY = Mathf.CeilToInt(Mathf.Max(start.y, end.y) + thickness);
        Vector2 line = end - start;
        float lineLengthSquared = line.sqrMagnitude;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
                {
                    continue;
                }

                Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                float t = Mathf.Clamp01(Vector2.Dot(point - start, line) / lineLengthSquared);
                Vector2 closest = start + line * t;

                if (Vector2.Distance(point, closest) <= thickness * 0.5f)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
    }

    // Tests whether a point is inside a triangle using signed areas.
    private static bool IsPointInsideTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
    {
        float area1 = TriangleSign(point, a, b);
        float area2 = TriangleSign(point, b, c);
        float area3 = TriangleSign(point, c, a);

        bool hasNegative = area1 < 0f || area2 < 0f || area3 < 0f;
        bool hasPositive = area1 > 0f || area2 > 0f || area3 > 0f;

        return !(hasNegative && hasPositive);
    }

    // Calculates the signed area helper used by triangle rasterization.
    private static float TriangleSign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) -
            (p2.x - p3.x) * (p1.y - p3.y);
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

        Color oldColor = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), screenShadeTexture, ScaleMode.StretchToFill);

        GUI.color = new Color(1f, 0.92f, 0.42f, 0.2f);
        GUI.DrawTexture(new Rect(Screen.width * 0.18f, Screen.height * 0.08f, Screen.width * 0.46f, Screen.height * 0.42f), titleGlowTexture, ScaleMode.StretchToFill);
        GUI.color = oldColor;
    }

    // Draws lightweight ambient motes and drifting leaves over the illustrated background.
    private void DrawAmbientEffects()
    {
        float time = Time.realtimeSinceStartup;
        Color oldColor = GUI.color;
        Matrix4x4 oldMatrix = GUI.matrix;

        for (int i = 0; i < 18; i++)
        {
            float seed = i * 37.137f;
            float x = Mathf.Repeat(seed * 41f + time * (10f + i * 0.6f), Screen.width + 80f) - 40f;
            float y = Mathf.Repeat(seed * 19f - time * (5f + i * 0.22f), Screen.height * 0.72f) + Screen.height * 0.08f;
            float pulse = 0.45f + Mathf.Sin(time * 1.8f + seed) * 0.2f;
            float size = 5f + (i % 4) * 2.4f;

            GUI.color = new Color(1f, 0.94f, 0.58f, 0.16f + pulse * 0.18f);
            GUI.DrawTexture(new Rect(x, y, size, size), particleTexture, ScaleMode.StretchToFill);
        }

        for (int i = 0; i < 6; i++)
        {
            float seed = i * 53.91f;
            float x = Mathf.Repeat(seed * 31f + time * (18f + i * 1.5f), Screen.width + 90f) - 45f;
            float y = Screen.height * (0.18f + i * 0.08f) + Mathf.Sin(time * 0.9f + seed) * 18f;
            float size = 13f + (i % 3) * 4f;

            GUI.matrix = oldMatrix;
            GUIUtility.RotateAroundPivot(Mathf.Sin(time + seed) * 12f, new Vector2(x + size * 0.5f, y + size * 0.5f));
            GUI.color = new Color(0.42f, 0.68f, 0.2f, 0.36f);
            GUI.DrawTexture(new Rect(x, y, size, size), leafTexture, ScaleMode.ScaleToFit);
        }

        GUI.matrix = oldMatrix;
        GUI.color = oldColor;
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
