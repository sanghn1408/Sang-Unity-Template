using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Allows a "cold start" in the editor, when pressing Play and not passing from the Initialisation scene.
/// </summary> 
public class EditorColdStartup : MonoBehaviour
{
#if UNITY_EDITOR
	[SerializeField] private GameSceneSO _thisSceneSO = default;
	[SerializeField] private GameSceneSO _persistentManagersSO = default;
	[SerializeField] private LoadEventChannelSO _notifyColdStartupChannel = default;
	[SerializeField] private VoidEventChannelSO _onSceneReadyChannel = default;

	private bool isColdStart = false;

	private void Awake()
	{
		if (!SceneManager.GetSceneByName(_persistentManagersSO.sceneReference.SceneName).isLoaded)
		{
			isColdStart = true;
		}
	}

	private void Start()
	{
		if (isColdStart)
		{
			_persistentManagersSO.sceneReference.LoadSceneAsync(LoadSceneMode.Additive).completed += OnNotifyChannelLoaded;
		}
	}

	private void OnNotifyChannelLoaded(AsyncOperation obj)
	{
		if(_thisSceneSO != null)
		{
			_notifyColdStartupChannel.RaiseEvent(_thisSceneSO);
		}
		else
		{
			//Raise a fake scene ready event, so the player is spawned
			_onSceneReadyChannel.RaiseEvent();
			//When this happens, the player won't be able to move between scenes because the SceneLoader has no conception of which scene we are in
		}
	}
#endif
}
