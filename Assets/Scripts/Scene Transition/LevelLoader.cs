using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }

    [SerializeField] private Animator transition;
    [SerializeField] private float transitionTime = 1f;

    [Tooltip("Optional. Swallows clicks while the transition is covering the screen.")]
    [SerializeField] private CanvasGroup blocker;

    private bool useLock = false;

    public bool IsLoading => useLock;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetBlocking(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // public API 

    public void LoadLevel(string sceneName)
    {
        if (useLock) return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[LevelLoader] Scene name is empty.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[LevelLoader] Scene '{sceneName}' is not in Build Settings.", this);
            return;
        }

        StartCoroutine(LoadLevelRoutine(() => SceneManager.LoadSceneAsync(sceneName)));
    }

    public void LoadLevel(int buildIndex)
    {
        if (useLock) return;

        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"[LevelLoader] Build index {buildIndex} is out of range.", this);
            return;
        }

        StartCoroutine(LoadLevelRoutine(() => SceneManager.LoadSceneAsync(buildIndex)));
    }

    public void LoadNext() => LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);

    public void ReloadLevel() => LoadLevel(SceneManager.GetActiveScene().buildIndex);

  


    private IEnumerator LoadLevelRoutine(Func<AsyncOperation> beginLoad)
    {
        useLock = true;
        SetBlocking(true);

        if (transition != null)
            transition.SetTrigger("Start");

        // Start streaming immediately so the load overlaps the transition
        // instead of hitching after it.
        AsyncOperation op = beginLoad();
        op.allowSceneActivation = false;

        yield return new WaitForSeconds(transitionTime);

        // progress stalls at 0.9 while activation is blocked; if the scene is
        // heavy this holds the covered screen a bit longer instead of hitching.
        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        SetBlocking(false);
        useLock = false;
    }

    private void SetBlocking(bool on)
    {
        if (blocker != null)
            blocker.blocksRaycasts = on;
    }
}