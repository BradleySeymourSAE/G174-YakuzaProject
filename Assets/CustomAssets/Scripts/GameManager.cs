#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#endregion

public class GameManager : MonoBehaviour
{ 


	public enum GameState { MainMenu, Loading, Active, InActive };
	
	[Header("Game Configuration")]
	public GameState currentState;
	public KeyCode quitKeycode = KeyCode.Escape;

	#region Protected

	[SerializeField] protected Transform m_Environment;

	#endregion

	#region Debug

	[Header("Debugging")]
	public static float loadingWaitTime = 3f;
	public static bool Debugging = true;
	
	#endregion

	#region Public Static 
	
	public static GameManager s_Instance;
	public static UnityEvent OnGameStartEvent = new UnityEvent();
	public static UnityEvent OnRestartEvent = new UnityEvent();
	public static UnityEvent OnApplicationQuit = new UnityEvent();

	#endregion

	#region Unity References 

	private void OnEnable()
	{
		if (s_Instance != null && s_Instance != this)
			Destroy(s_Instance.gameObject);

		s_Instance = this;
		OnRestartEvent.AddListener(() => Reset());
		LoadingStateManager.LoadingEvent.AddListener(() => {  currentState = GameState.Loading; });
		LoadingStateManager.LoadMainMenuEvent.AddListener(() => {  currentState = GameState.MainMenu; AudioManager.PlaySound(GameSound.GUI_BackwardsNavigation); });
		OnGameStartEvent.AddListener(() => { currentState = GameState.Active; Debug.Log("GameManager.cs, Started game!"); AudioManager.PlaySound(GameSound.GUI_MouseClick); });
		OnApplicationQuit.AddListener(() => { QuitApplication(); AudioManager.PlaySound(GameSound.GUI_MouseClick); });
	}

	private void OnDisable()
	{
		OnRestartEvent.RemoveListener(() => {  Reset(); });
		LoadingStateManager.LoadingEvent.RemoveListener(() => {  currentState = GameState.Loading; });
		LoadingStateManager.LoadMainMenuEvent.RemoveListener(() => { currentState = GameState.MainMenu; AudioManager.PlaySound(GameSound.GUI_BackwardsNavigation); });
		OnGameStartEvent.RemoveListener(() => {  currentState = GameState.Active; AudioManager.PlaySound(GameSound.GUI_MouseClick); });
		OnApplicationQuit.RemoveListener(() => {  QuitApplication(); AudioManager.PlaySound(GameSound.GUI_MouseClick); });
	}

	private void Start()
	{
		AudioManager.PlaySound(GameSound.BackgroundAmbience);
		LoadingStateManager.BootstrapEvent?.Invoke();
		if (m_Environment == null)
		{
			m_Environment = GameObject.Find("FakeEnvironment").transform;
		}
	}

	private void Update()
	{
		UpdateInput();

		if (LoadingStateManager.SceneIsLoading || currentState.Equals(GameState.MainMenu))
		{
			m_Environment.gameObject.SetActive(true);
		}
		else
		{
			m_Environment.gameObject.SetActive(false);
		}
	}

	private void Reset()
	{
		currentState = GameState.Active;

		if (!LoadingStateManager.SceneIsLoading)
			GameUIManager.s_Instance.loadingScreen.Display(false);
	}

	#endregion

	#region Private Methods

	private void UpdateInput()
	{
		if (Input.GetKeyDown(quitKeycode))
		{
			if (currentState.Equals(GameState.Active) || currentState.Equals(GameState.InActive))
				LoadingStateManager.MainMenuReturnEvent?.Invoke();
			else if (currentState.Equals(GameState.MainMenu))
				GameManager.OnApplicationQuit?.Invoke();
		}
	}

	private void QuitApplication()
	{
		AudioManager.PlaySound(GameSound.GUI_MouseClick);

		string l_Message = "";
		#if UNITY_STANDALONE
			Application.Quit();
			l_Message += " Quitting application!";
		#endif
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			l_Message += " Quitting Application from the editor!";
		#endif

		if (Debugging)
			Debug.Log("[GameManager.cs]: " + l_Message);
	}

	#endregion

}
