using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class manages the scene loading and unloading.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameSceneSO _gamecommonScene = default;

    [Header("Load Events")]
    [SerializeField] private LoadEventChannelSO _loadGamePlay = default;
    [SerializeField] private LoadEventChannelSO _loadMenu = default;
    [SerializeField] private LoadEventChannelSO _coldStartupLocation = default;

    [Header("Broadcasting on")]
    [SerializeField] private FadeChannelSO _fadeChannelSO = default;
    [SerializeField] private BoolEventChannelSO _toggleLoadingScreen = default;
    [SerializeField] private VoidEventChannelSO _onSceneReady = default;

    private AsyncOperation _loadingOperationHandle;
    private AsyncOperation _gameplayManagerLoadingOpHandle;

    //Parameters coming from scene loading requests
    private GameSceneSO _sceneToLoad;
    private GameSceneSO _currentlyLoadedScene;
    private bool _showLoadingScreen;

    private Scene _gameplayManagerScene;
    private Scene _currentLoadingScene;

    private void OnEnable()
    {
        _loadGamePlay.OnLoadingRequested += LoadGame;
        _loadMenu.OnLoadingRequested += LoadMenu;
#if UNITY_EDITOR
        _coldStartupLocation.OnLoadingRequested += LocationColdStartup;
#endif
    }

    private void OnDisable()
    {
        _loadGamePlay.OnLoadingRequested -= LoadGame;
        _loadMenu.OnLoadingRequested -= LoadMenu;
#if UNITY_EDITOR
        _coldStartupLocation.OnLoadingRequested -= LocationColdStartup;
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// This special loading function is only used in the editor, when the developer presses Play in a Location scene, without passing by Initialisation.
    /// </summary>
    private void LocationColdStartup(GameSceneSO currentlyOpenedLocation, bool showLoadingScreen)
    {
        _currentlyLoadedScene = currentlyOpenedLocation;

        if (_currentlyLoadedScene.sceneType == GameSceneSO.GameSceneType.Game)
        {
            //Gameplay managers is loaded synchronously
            _gameplayManagerLoadingOpHandle = _gamecommonScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive);
            _gameplayManagerLoadingOpHandle.completed += (asyncccc) =>
            {
                _gameplayManagerScene = SceneManager.GetSceneByName(_gamecommonScene.sceneReference.SceneName);
                StartGameplay();
            };
        }
    }
#endif

    /// <summary>
    /// This function loads the location scenes passed as array parameter
    /// </summary>
    private void LoadGame(GameSceneSO locationToLoad, bool showLoadingScreen)
    {
        _sceneToLoad = locationToLoad;
        _showLoadingScreen = showLoadingScreen;

        //In case we are coming from the main menu, we need to load the Gameplay manager scene first
        if (_gameplayManagerScene == null
            || !_gameplayManagerScene.isLoaded)
        {
            _gameplayManagerLoadingOpHandle = _gamecommonScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive);
            _gameplayManagerLoadingOpHandle.completed += OnGameplayMangersLoaded;
        }
        else
        {
            UnloadPreviousScene();
        }
    }

    private void OnGameplayMangersLoaded(AsyncOperation obj)
    {
        _gameplayManagerScene = SceneManager.GetSceneByName(_gamecommonScene.sceneReference.SceneName);

        UnloadPreviousScene();
    }

    /// <summary>
    /// Prepares to load the main menu scene, first removing the Gameplay scene in case the game is coming back from gameplay to menus.
    /// </summary>
    private void LoadMenu(GameSceneSO menuToLoad, bool showLoadingScreen)
    {
        _sceneToLoad = menuToLoad;
        _showLoadingScreen = showLoadingScreen;

        //In case we are coming from a Location back to the main menu, we need to get rid of the persistent Gameplay manager scene
        if (_gameplayManagerScene != null
            && _gameplayManagerScene.isLoaded)
            SceneManager.UnloadSceneAsync(_gameplayManagerScene);

        UnloadPreviousScene();
    }

    /// <summary>
    /// In both Location and Menu loading, this function takes care of removing previously loaded scenes.
    /// </summary>
    private void UnloadPreviousScene()
    {
        StartCoroutine(IE_UnloadPreviousScene());
    }

    private IEnumerator IE_UnloadPreviousScene()
    {
        _fadeChannelSO.FadeIn(0.5f);
        yield return new WaitForSecondsRealtime(0.5f);
        if (_currentlyLoadedScene != null) //would be null if the game was started in Initialisation
        {
            SceneManager.UnloadSceneAsync(_currentlyLoadedScene.sceneReference.SceneName);
        }


        LoadNewScene();
    }


    /// <summary>
    /// Kicks off the asynchronous loading of a scene, either menu or Location.
    /// </summary>
    private void LoadNewScene()
    {
        if (_showLoadingScreen)
            _toggleLoadingScreen.RaiseEvent(true);

        _loadingOperationHandle = _sceneToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive);

        _loadingOperationHandle.completed += OnNewSceneLoaded;
    }

    private void OnNewSceneLoaded(AsyncOperation obj)
    {

        StartCoroutine(IE_OnNewSceneLoaded(obj));
    }

    private bool _isFirstLoad;
    private IEnumerator IE_OnNewSceneLoaded(AsyncOperation obj)
    {
        
        if (Static.IsUseNewLoading == false)
            yield return new WaitUntil(() => LoadingFill.IsOn);
        _currentLoadingScene = SceneManager.GetSceneByName(_sceneToLoad.sceneReference.SceneName);
        _currentlyLoadedScene = _sceneToLoad;

        if (_isFirstLoad == false && Static.IsUseNewLoading == false)
        {
            _isFirstLoad = true;
            //bool isShowAppOpen = AdsManager.Instance.CountOpenGame >= 2 && AdsManager.Instance.IsCanShowAOA();
            //if (isShowAppOpen)
            //    AdsManager.Instance.ShowAOA(true);
        }
        _fadeChannelSO.FadeOut(0.5f);
        if (_showLoadingScreen)
            _toggleLoadingScreen.RaiseEvent(false);

        SetActiveScene();

    }

    /// <summary>
    /// This function is called when all the scenes have been loaded
    /// </summary>
    private void SetActiveScene()
    {
        SceneManager.SetActiveScene(_currentLoadingScene);
        //LightProbes.TetrahedralizeAsync();
       
        StartGameplay();
    }

    private void StartGameplay()
    {
        _onSceneReady.RaiseEvent();
    }
}
