#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
#endregion


public class LoadingStateManager : MonoBehaviour
{

	#region Public Static

	public static UnityEvent BootstrapEvent = new UnityEvent();
	public static UnityEvent LoadMainMenuEvent = new UnityEvent();
	public static UnityEvent MainMenuReturnEvent = new UnityEvent();
	public static UnityEvent LoadingEvent = new UnityEvent();

	public static LoadingStateManager s_Instance;

	#endregion

	#region Private Static
	
	private static AsyncOperation s_CurrentSceneLoading;
	private static Coroutine s_LoadingRoutine;

	#endregion

	#region Getters / Setters

	public static bool SceneIsLoading => s_CurrentSceneLoading != null;

	#endregion

	#region Unity References

	private void OnEnable()
	{
		if (s_Instance != null && s_Instance != this)
			Destroy(s_Instance.gameObject);

		s_Instance = this;

		BootstrapEvent.AddListener(OnBootstrap);
		GameManager.OnGameStartEvent.AddListener(OnGameStart);
		MainMenuReturnEvent.AddListener(OnMainMenuReturn);
	}

	private void OnDisable()
	{
		BootstrapEvent.RemoveListener(OnBootstrap);
		GameManager.OnGameStartEvent.RemoveListener(OnGameStart);
		MainMenuReturnEvent.RemoveListener(OnMainMenuReturn);
	}

	#endregion

	#region Private Methods

	private void OnBootstrap()
	{
		if (s_LoadingRoutine != null)
			StopCoroutine(s_LoadingRoutine);

		s_LoadingRoutine = StartCoroutine(Bootstrap());
	}

	private void OnGameStart()
	{
		if (s_LoadingRoutine != null)
			StopCoroutine(s_LoadingRoutine);

		s_LoadingRoutine = StartCoroutine(LoadLevel());
	}

	private void OnMainMenuReturn()
	{
		if (s_LoadingRoutine != null)
			StopCoroutine(s_LoadingRoutine);

		s_LoadingRoutine =  StartCoroutine(UnloadLevel());
	}

	private IEnumerator Bootstrap()
	{
		var l_LoadingWaitTime = GameManager.Debugging == true ? GameManager.loadingWaitTime : 3f;
		s_CurrentSceneLoading = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);

		while (!s_CurrentSceneLoading.isDone)
		{
			yield return null;
		}
		s_CurrentSceneLoading = null;
		LoadingEvent?.Invoke();

		if (GameManager.Debugging)
			Debug.Log($"Loading Scene, displaying loading screen for {l_LoadingWaitTime} seconds!");

		yield return new WaitForSeconds(l_LoadingWaitTime);
		
		s_CurrentSceneLoading = null;
		LoadMainMenuEvent?.Invoke();
		yield return null;
	}

	private IEnumerator LoadLevel()
	{
		LoadingEvent?.Invoke();
		s_CurrentSceneLoading = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
		s_CurrentSceneLoading.allowSceneActivation = false;

		if (GameManager.Debugging)
			Debug.Log($"Loading level for {GameManager.loadingWaitTime} seconds.");
		
		yield return new WaitForSeconds(GameManager.loadingWaitTime);

		while (s_CurrentSceneLoading.progress < 0.89f)
		{  yield return null; }

		s_CurrentSceneLoading.allowSceneActivation = true;
		s_CurrentSceneLoading = null;

		GameManager.OnRestartEvent?.Invoke();
		yield return null;
	}

	private IEnumerator UnloadLevel()
	{
		LoadingEvent?.Invoke();
		s_CurrentSceneLoading = SceneManager.UnloadSceneAsync(2);
		s_CurrentSceneLoading.allowSceneActivation = false;

		if (GameManager.Debugging)
			Debug.Log($"Unloading level over {GameManager.loadingWaitTime}...\nLoading: {s_CurrentSceneLoading}");

		yield return new WaitForSeconds(GameManager.loadingWaitTime);

		while (s_CurrentSceneLoading.progress < 0.89f)
		{  yield return null; }

		s_CurrentSceneLoading.allowSceneActivation = true;
		s_CurrentSceneLoading = null;
		LoadMainMenuEvent?.Invoke();

		yield return null;
	}

	#endregion

}