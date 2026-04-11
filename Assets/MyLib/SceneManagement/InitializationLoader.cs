using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is responsible for starting the game by loading the persistent managers scene 
/// and raising the event to load the Main Menu
/// </summary>

public class InitializationLoader : MonoBehaviour
{
    [SerializeField] private GameSceneSO _managersScene = default;
    [SerializeField] private GameSceneSO _gamePlayScene = default;

    [Header("Broadcasting on")]
    [SerializeField] private LoadEventChannelSO _loadEventChannelSO = default;

    private void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        //Load the persistent managers scene
        _managersScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive).completed += LoadNewScene;
    }

    private void LoadNewScene(AsyncOperation obj)
    {
        _loadEventChannelSO.RaiseEvent(_gamePlayScene, true);

        SceneManager.UnloadSceneAsync(0); //Initialization is the only scene in BuildSettings, thus it has index 0
    }
}
