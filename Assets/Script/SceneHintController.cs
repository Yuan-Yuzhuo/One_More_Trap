using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHintController : MonoBehaviour
{
    private static SceneHintController instance;

    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float holdDuration = 2f;
    [SerializeField] private float startDelay = 0.55f;
    [SerializeField] private float panelWidth = 760f;
    [SerializeField] private float panelHeight = 86f;

    private string currentHint = "";
    private float hintAlpha = 0f;
    private Coroutine hintRoutine;
    private GUIStyle textStyle;
    private GUIStyle shadowStyle;

    public static bool IsHintVisible
    {
        get { return instance != null && !string.IsNullOrEmpty(instance.currentHint) && instance.hintAlpha > 0f; }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    // Creates the persistent hint controller before gameplay scenes load.
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    // Ensures there is exactly one persistent scene hint controller.
    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject hintObject = new GameObject("SceneHintController");
        DontDestroyOnLoad(hintObject);
        hintObject.AddComponent<SceneHintController>();
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
    }

    private void OnDestroy()
    {
        if (instance != this)
        {
            return;
        }

        SceneManager.sceneLoaded -= HandleSceneLoaded;

        instance = null;
    }

    // Shows a hint when the loaded scene has one configured.
    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string hint = GetHintForScene(scene.name, scene.buildIndex);

        if (hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
            hintRoutine = null;
        }

        currentHint = hint;
        hintAlpha = 0f;

        if (!string.IsNullOrEmpty(currentHint))
        {
            hintRoutine = StartCoroutine(ShowHintRoutine());
        }
    }

    // Maps scene names to their player-facing hint text.
    private static string GetHintForScene(string sceneName, int buildIndex)
    {
        if (sceneName == "1.Beginning" || buildIndex == 1)
        {
            return "Watch for falling traps and unstable ground.";
        }

        if (sceneName == "Hurdle2" || sceneName == "2.MovingSpike" || buildIndex == 2 || buildIndex == 3)
        {
            return "Spikes are lethal. Dodge them.";
        }

        if (sceneName == "5.ChasingSpikes" || buildIndex == 6)
        {
            return "Use your dash wisely to stay ahead of the chasing spikes.";
        }

        if (sceneName == "7.TrapCoin" || buildIndex == 8)
        {
            return "Greed can sometimes lead to ruin.";
        }

        if (sceneName == "8.maze" || buildIndex == 9)
        {
            return "You are now entering the space station.";
        }

        if (sceneName == "9.FreeFalling" || buildIndex == 10)
        {
            return "Be careful: inertia will affect your movement. Watch out for the red lines; they are lethal.";
        }

        return "";
    }

    // Fades the hint in, holds it, then fades it out.
    private IEnumerator ShowHintRoutine()
    {
        yield return new WaitForSecondsRealtime(startDelay);
        yield return FadeHint(0f, 1f);
        yield return new WaitForSecondsRealtime(holdDuration);
        yield return FadeHint(1f, 0f);

        currentHint = "";
        hintRoutine = null;
    }

    // Animates hint alpha using unscaled time so it works during scene transitions.
    private IEnumerator FadeHint(float from, float to)
    {
        float duration = Mathf.Max(0.01f, fadeDuration);
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            hintAlpha = Mathf.Lerp(from, to, Mathf.Clamp01(timer / duration));
            yield return null;
        }

        hintAlpha = to;
    }

    // Lazily builds GUI styles for high-contrast hint text.
    private void EnsureStyles()
    {
        if (textStyle != null && shadowStyle != null)
        {
            return;
        }

        textStyle = new GUIStyle(GUI.skin.label);
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.fontSize = 24;
        textStyle.fontStyle = FontStyle.Normal;
        textStyle.wordWrap = true;
        textStyle.normal.textColor = Color.white;

        shadowStyle = new GUIStyle(textStyle);
        shadowStyle.normal.textColor = Color.black;
    }

    // Draws the active hint near the top of the screen.
    private void OnGUI()
    {
        if (string.IsNullOrEmpty(currentHint) || hintAlpha <= 0f)
        {
            return;
        }

        EnsureStyles();

        float width = Mathf.Min(panelWidth, Screen.width - 40f);
        Rect rect = new Rect(
            (Screen.width - width) * 0.5f,
            Mathf.Max(22f, Screen.height * 0.09f),
            width,
            panelHeight
        );

        Color oldColor = GUI.color;

        GUI.color = new Color(0f, 0f, 0f, hintAlpha * 0.75f);
        GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), currentHint, shadowStyle);
        GUI.color = new Color(0f, 0f, 0f, hintAlpha * 0.45f);
        GUI.Label(new Rect(rect.x - 1f, rect.y + 1f, rect.width, rect.height), currentHint, shadowStyle);

        GUI.color = new Color(1f, 1f, 1f, hintAlpha);
        GUI.Label(rect, currentHint, textStyle);
        GUI.color = oldColor;
    }
}
