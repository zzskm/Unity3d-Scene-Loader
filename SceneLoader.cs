//#define UNI_RX
using UnityEngine;
using System.Collections;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

#if UNI_RX
using UniRx;
#endif

public class SceneLoader : MonoBehaviour
{
    private const string LoadingScene = "Loading";

    protected static string _PreviousScene;
    protected static string _CurrentScene;

    protected static System.Action _LoadStart;
    protected static System.Action _LoadComplete;

    protected static bool _IsLoading;

    public static bool IsLoading()
    {
        return _IsLoading;
    }

    public static void Load(string scene, System.Action startAction = null, System.Action completeAction = null)
    {
        _PreviousScene = _CurrentScene;
        _CurrentScene = scene;

        _LoadStart = startAction;
        _LoadComplete = completeAction;

        _IsLoading = true;

        CreateSceneLoader();
    }

    protected static void CreateSceneLoader()
    {
        System.Type sceneLoaderType = typeof(SceneLoader);
        GameObject go = new GameObject(sceneLoaderType.FullName, sceneLoaderType);
        DontDestroyOnLoad(go);
    }

    protected WaitForEndOfFrame _waitForEndOfFrame;

    void OnEnable()
    {
        // cached
        _waitForEndOfFrame = new WaitForEndOfFrame();

#if UNI_RX
        Observable.FromCoroutine(StartLoad).Subscribe().AddTo(this);
#else
        StartCoroutine(StartLoad());
#endif
    }

    protected virtual IEnumerator StartLoad()
    {
        //UIManager.Instance.ShowLoading();

        yield return _waitForEndOfFrame;
        yield return LoadSceneAsync(LoadingScene);

        if (_LoadStart != null)
        {
            _LoadStart();
            _LoadStart = null;

            yield return _waitForEndOfFrame;
        }

        //ZPlayerPrefs.Save();
        //UIManager.Instance.Cleanse();

        yield return _waitForEndOfFrame;

        yield return LoadSceneAsync(_CurrentScene);
        yield return _waitForEndOfFrame;

        // cleanup memory
        //UIDrawCall.ReleaseInactive();

        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();

        //DG.Tweening.DOTween.Clear();

        yield return _waitForEndOfFrame;

        if (_LoadComplete != null)
        {
            _LoadComplete();
            _LoadComplete = null;
        }

        Destroy(gameObject);
    }

    public AsyncOperation LoadSceneAsync(string sceneName)
    {
#if UNITY_5_3_OR_NEWER
        return SceneManager.LoadSceneAsync(sceneName);
#else
        return Application.LoadLevelAsync(sceneName);
#endif
    }
}
