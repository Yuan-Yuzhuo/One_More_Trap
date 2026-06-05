using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionController : MonoBehaviour
{
    private const string TransitionClipResourceName = "esclate";

    private static SceneTransitionController instance;

    [SerializeField] private float fadeDuration = 0.45f;
    [SerializeField] private float frozenFadeDuration = 1.1f;
    [SerializeField] private float transitionVolume = 1f;

    private AudioClip transitionClip;
    private bool isTransitioning;
    private float fadeAlpha;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    // Creates the persistent transition controller before any scene loads.
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    public static void LoadScene(int sceneIndex)
    {
        EnsureInstance();
        instance.StartTransition(sceneIndex, true);
    }

    public static void LoadScene(string sceneName)
    {
        EnsureInstance();
        instance.StartTransition(sceneName, true);
    }

    public static void LoadSceneWithoutSound(int sceneIndex)
    {
        EnsureInstance();
        instance.StartTransition(sceneIndex, false, false);
    }

    public static void LoadSceneFrozen(int sceneIndex)
    {
        EnsureInstance();
        instance.StartTransition(sceneIndex, true, true);
    }

    // Ensures there is exactly one transition controller across scenes.
    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject transitionObject = new GameObject("SceneTransitionController");
        DontDestroyOnLoad(transitionObject);
        transitionObject.AddComponent<SceneTransitionController>();
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
        transitionClip = Resources.Load<AudioClip>(TransitionClipResourceName);
    }

    // Begins a fade transition to a scene by build index.
    private void StartTransition(int sceneIndex, bool playSound)
    {
        StartTransition(sceneIndex, playSound, false);
    }

    // Begins a fade transition to a scene by build index.
    private void StartTransition(int sceneIndex, bool playSound, bool freezeBeforeFade)
    {
        if (isTransitioning)
        {
            return;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneIndex);
        loadOperation.allowSceneActivation = false;
        StartCoroutine(TransitionRoutine(loadOperation, playSound, freezeBeforeFade));
    }

    // Begins a fade transition to a scene by name.
    private void StartTransition(string sceneName, bool playSound)
    {
        StartTransition(sceneName, playSound, false);
    }

    // Begins a fade transition to a scene by name.
    private void StartTransition(string sceneName, bool playSound, bool freezeBeforeFade)
    {
        if (isTransitioning)
        {
            return;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        loadOperation.allowSceneActivation = false;
        StartCoroutine(TransitionRoutine(loadOperation, playSound, freezeBeforeFade));
    }

    // Fades to black, activates the loaded scene, then fades back in.
    private IEnumerator TransitionRoutine(AsyncOperation loadOperation, bool playSound, bool freezeBeforeFade)
    {
        isTransitioning = true;
        fadeAlpha = 0f;
        float timeScaleBeforeTransition = Time.timeScale;

        if (playSound)
        {
            PlayTransitionSound();
        }

        if (freezeBeforeFade)
        {
            Time.timeScale = 0f;
        }

        yield return FadeOverlay(0f, 1f, freezeBeforeFade ? frozenFadeDuration : fadeDuration);

        loadOperation.allowSceneActivation = true;

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        yield return null;

        if (freezeBeforeFade)
        {
            Time.timeScale = timeScaleBeforeTransition;
        }

        yield return FadeOverlay(1f, 0f, fadeDuration);
        fadeAlpha = 0f;
        isTransitioning = false;
    }

    // Animates the full-screen black overlay alpha.
    private IEnumerator FadeOverlay(float from, float to, float duration)
    {
        duration = Mathf.Max(0.01f, duration);
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            fadeAlpha = Mathf.Lerp(from, to, Mathf.Clamp01(timer / duration));
            yield return null;
        }

        fadeAlpha = to;
    }

    // Plays scene-transition audio from a temporary object that survives scene activation.
    private void PlayTransitionSound()
    {
        if (transitionClip == null)
        {
            return;
        }

        GameObject soundObject = new GameObject("SceneTransitionSound");
        DontDestroyOnLoad(soundObject);

        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.PlayOneShot(transitionClip, transitionVolume);

        Destroy(soundObject, transitionClip.length + 0.1f);
    }

    // Draws the transition overlay while a scene load is active.
    private void OnGUI()
    {
        if (!isTransitioning)
        {
            return;
        }

        Color oldColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, fadeAlpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = oldColor;
    }
}
