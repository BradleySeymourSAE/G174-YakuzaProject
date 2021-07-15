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
	public static UnityEvent MainScreenLoadingEvent = new UnityEvent();
	public static UnityEvent ReturnToMainScreen = new UnityEvent();
	public static UnityEvent LoadingEvent = new UnityEvent();

	public static LoadingStateManager s_Instance;

	#endregion

	#region Private Static
	
	private static AsyncOperation s_CurrentSceneLoading;
	private static Coroutine s_LoadingRoutine;

	#endregion

	#region Getters / Setters

	public static bool SceneIsLoading { get { return s_CurrentSceneLoading != null; } }

	#endregion

	#region Unity References

	private void OnEnable()
	{
		if (s_Instance != null && s_Instance != this)
			Destroy(s_Instance.gameObject);

		s_Instance = this;

		BootstrapEvent.AddListener(StartBootstrapSequence);
		GameManager.OnGameStartEvent.AddListener(StartGame);
		ReturnToMainScreen.AddListener(ReturnToMainMenu);
	}

	private void OnDisable()
	{
		BootstrapEvent.RemoveListener(StartBootstrapSequence);
		GameManager.OnGameStartEvent.RemoveListener(StartGame);
		ReturnToMainScreen.RemoveListener(ReturnToMainMenu);
	}

	#endregion

	#region Private Methods

	private void StartBootstrapSequence()
	{
		if (s_LoadingRoutine != null)
			StopCoroutine(s_LoadingRoutine);

		s_LoadingRoutine = StartCoroutine(Bootstrap());
	}

	private void StartGame()
	{
		if (s_LoadingRoutine != null)
			StopCoroutine(s_LoadingRoutine);

		s_LoadingRoutine = StartCoroutine(LoadLevel());


	}

	private void ReturnToMainMenu()
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
		MainScreenLoadingEvent?.Invoke();
		yield return null;
	}

	private IEnumerator LoadLevel()
	{
		LoadingEvent?.Invoke(); // Starting loading in the next scene 
		s_CurrentSceneLoading = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
		s_CurrentSceneLoading.allowSceneActivation = false;

		if (GameManager.Debugging)
			Debug.Log($"Loading level for {GameManager.loadingWaitTime} seconds.");
		
		yield return new WaitForSeconds(GameManager.loadingWaitTime);

		while (s_CurrentSceneLoading.progress < 0.89f)
		{  yield return null; }

		s_CurrentSceneLoading.allowSceneActivation = true;
		s_CurrentSceneLoading = null;

		GameManager.RestartEvent?.Invoke();
		yield return null;
	}

	private IEnumerator UnloadLevel()
	{
		LoadingEvent?.Invoke(); // start unloading the game scene 
		s_CurrentSceneLoading = SceneManager.UnloadSceneAsync(2);
		s_CurrentSceneLoading.allowSceneActivation = false;

		if (GameManager.Debugging)
			Debug.Log($"Unloading level over {GameManager.loadingWaitTime}...\nLoading: {s_CurrentSceneLoading}");

		yield return new WaitForSeconds(GameManager.loadingWaitTime);

		while (s_CurrentSceneLoading.progress < 0.89f)
		{  yield return null; }

		s_CurrentSceneLoading.allowSceneActivation = true;
		s_CurrentSceneLoading = null;

		MainScreenLoadingEvent?.Invoke();
		yield return null;
	}

	#endregion

}