using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionController : MonoBehaviour
{
    private const string TransitionClipResourceName = "esclate";

    private static SceneTransitionController instance;

    [SerializeField] private float fadeDuration = 0.45f;
    [SerializeField] private float transitionVolume = 1f;

    private AudioClip transitionClip;
    private bool isTransitioning;
    private float fadeAlpha;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
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
        instance.StartTransition(sceneIndex, false);
    }

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

    private void StartTransition(int sceneIndex, bool playSound)
    {
        if (isTransitioning)
        {
            return;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneIndex);
        loadOperation.allowSceneActivation = false;
        StartCoroutine(TransitionRoutine(loadOperation, playSound));
    }

    private void StartTransition(string sceneName, bool playSound)
    {
        if (isTransitioning)
        {
            return;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        loadOperation.allowSceneActivation = false;
        StartCoroutine(TransitionRoutine(loadOperation, playSound));
    }

    private IEnumerator TransitionRoutine(AsyncOperation loadOperation, bool playSound)
    {
        isTransitioning = true;
        fadeAlpha = 0f;

        if (playSound)
        {
            PlayTransitionSound();
        }

        yield return FadeOverlay(0f, 1f);

        loadOperation.allowSceneActivation = true;

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        yield return null;

        yield return FadeOverlay(1f, 0f);
        fadeAlpha = 0f;
        isTransitioning = false;
    }

    private IEnumerator FadeOverlay(float from, float to)
    {
        float duration = Mathf.Max(0.01f, fadeDuration);
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            fadeAlpha = Mathf.Lerp(from, to, Mathf.Clamp01(timer / duration));
            yield return null;
        }

        fadeAlpha = to;
    }

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
