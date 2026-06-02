using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private const int FirstChallengeSceneIndex = 1;
    private const int RankingLimit = 10;

    private AudioSource menuAudioSource;
    private AudioClip clickClip;

    [SerializeField] private Texture2D backgroundTexture;

    private enum MenuView
    {
        Login,
        Register,
        Ranking,
        Configuration
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
    private Texture2D panelTexture;
    private Texture2D dialogTexture;
    private Texture2D tableHeaderTexture;
    private Texture2D tableRowTexture;
    private Texture2D tableAltRowTexture;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded += EnsureMenuController;
        EnsureMenuController(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

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

    private void OnGUI()
    {

        EnsureAudio();

        EnsureStyles();
        DrawBackground();
        CapturePendingInputKey();

        float maxPanelWidth = currentView == MenuView.Ranking ? 760f : 520f;
        float panelWidth = Mathf.Min(maxPanelWidth, Screen.width - 32f);
        float panelHeight = currentView == MenuView.Ranking ? 560f : 500f;
        Rect panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            Mathf.Max(20f, (Screen.height - panelHeight) * 0.5f),
            panelWidth,
            panelHeight
        );

        GUI.Box(panelRect, GUIContent.none, panelStyle);

        if (currentView == MenuView.Ranking || currentView == MenuView.Configuration)
        {
            Rect closeRect = new Rect(panelRect.xMax - 48f, panelRect.y + 14f, 34f, 34f);
            if (GUI.Button(closeRect, "X", closeButtonStyle))
            {
                PlayClickSound();
                currentView = MenuView.Login;
                message = "";
                pendingInputAction = null;
                CloseLoginErrorDialog();
            }
        }

        GUILayout.BeginArea(new Rect(panelRect.x + 28f, panelRect.y + 24f, panelRect.width - 56f, panelRect.height - 48f));
        GUILayout.Label("One More Trap", titleStyle);
        GUILayout.Space(16f);

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
        else
        {
            DrawConfiguration();
        }

        GUILayout.EndArea();

        if (showLoginErrorDialog)
        {
            DrawLoginErrorDialog();
        }
    }

    private void DrawLogin()
    {
        DrawAccountFields();

        GUILayout.Space(12f);

        if (GUILayout.Button("Login", GUILayout.Height(38f)))
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
        if (GUILayout.Button("Start Challenge", GUILayout.Height(34f)))
        {
            PlayClickSound();
            GameStatsTracker.StartChallenge();
            SceneTransitionController.LoadScene(FirstChallengeSceneIndex);
        }
        GUI.enabled = true;

        GUILayout.Space(8f);

        if (GUILayout.Button("Register Account", GUILayout.Height(34f)))
        {
            PlayClickSound();
            currentView = MenuView.Register;
            message = "";
            CloseLoginErrorDialog();
        }

        if (GUILayout.Button("View Rankings", GUILayout.Height(34f)))
        {
            PlayClickSound();

            currentView = MenuView.Ranking;
            message = "";
            pendingInputAction = null;
            CloseLoginErrorDialog();
        }

        if (GUILayout.Button("Personalized Configuration", GUILayout.Height(34f)))
        {
            PlayClickSound();

            currentView = MenuView.Configuration;
            message = "";
            pendingInputAction = null;
            CloseLoginErrorDialog();
        }

        GUILayout.Space(10f);
        DrawStatus();
    }

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

    private void DrawAccountFields()
    {
        GUILayout.Label("User Name", labelStyle);
        userName = GUILayout.TextField(userName, GUILayout.Height(32f));

        GUILayout.Space(8f);

        GUILayout.Label("Password", labelStyle);
        password = GUILayout.PasswordField(password, '*', GUILayout.Height(32f));
    }

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

    private void CloseLoginErrorDialog()
    {
        showLoginErrorDialog = false;
        loginErrorMessage = "";
    }

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

    private void DrawTableHeader(string valueTitle)
    {
        GUILayout.BeginHorizontal(tableHeaderStyle, GUILayout.Height(34f));
        GUILayout.Label("Rank", tableHeaderStyle, GUILayout.Width(58f));
        GUILayout.Label("Name", tableHeaderStyle, GUILayout.Width(150f));
        GUILayout.Label(valueTitle, tableHeaderStyle, GUILayout.Width(110f));
        GUILayout.Label("Beijing Time", tableHeaderStyle);
        GUILayout.EndHorizontal();
    }

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
        titleStyle.fontSize = 34;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.15f, 0.23f, 0.16f);

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
    }

    private static Texture2D CreateTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private void DrawBackground()
    {
        if (backgroundTexture == null)
        {
            return;
        }

        Rect targetRect = GetCoverRect(backgroundTexture.width, backgroundTexture.height);
        GUI.DrawTexture(targetRect, backgroundTexture, ScaleMode.StretchToFill);
    }

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
