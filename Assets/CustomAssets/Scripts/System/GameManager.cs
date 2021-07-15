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
	[SerializeField] protected Transform m_MainCamera;
	[SerializeField] protected Transform m_CameraSpawnPoint;
	[SerializeField] protected GameObject m_CameraPrefab;


	#endregion

	#region Debug

	[Header("Debugging")]
	public static float loadingWaitTime = 3f;
	public static bool Debugging = true;

	[SerializeField] private Color m_CameraSpawnPointColor = Color.red;
	[SerializeField] [Range(0.5f, 5f)] private float m_GizmoSphereSize = 0.3f;
	[SerializeField] private bool useWireSphere = false;
	#endregion

	#region Public Static 
	
	public static GameManager s_Instance;
	public static UnityEvent OnGameStartEvent = new UnityEvent();
	public static UnityEvent RestartEvent = new UnityEvent();
	public static UnityEvent OnGameEndedEvent = new UnityEvent();
	public Transform GetCameraSpawnPosition => m_CameraSpawnPoint;

	#endregion

	#region Unity References 

	private void OnDrawGizmos()
	{
		if (Debugging)
		{
				Gizmos.color = m_CameraSpawnPointColor;
				if (useWireSphere)
				Gizmos.DrawWireSphere(m_CameraSpawnPoint.position, m_GizmoSphereSize);
				else 
				Gizmos.DrawSphere(m_CameraSpawnPoint.position, m_GizmoSphereSize);
		}
	}

	private void OnEnable()
	{
		if (s_Instance != null && s_Instance != this)
			Destroy(s_Instance.gameObject);

		s_Instance = this;
		RestartEvent.AddListener(Reset);
		LoadingStateManager.LoadingEvent.AddListener(() => { 
			currentState = GameState.Loading;
		});
		LoadingStateManager.MainScreenLoadingEvent.AddListener(() => {  
			currentState = GameState.MainMenu; 
			AudioManager.PlaySound(GameSound.GUI_BackwardsNavigation); 
		});

		OnGameStartEvent.AddListener(() => { currentState = GameState.Active; Debug.Log("GameManager.cs, Started game!"); AudioManager.PlaySound(GameSound.GUI_MouseClick); });
	}

	private void OnDisable()
	{
		RestartEvent.RemoveListener(Reset);
		LoadingStateManager.LoadingEvent.RemoveListener(() => {
			currentState = GameState.Loading; 
		});
		LoadingStateManager.MainScreenLoadingEvent.RemoveListener(() => {
			currentState = GameState.MainMenu; 
			AudioManager.PlaySound(GameSound.GUI_BackwardsNavigation); 
		});
		OnGameStartEvent.RemoveListener(() => { currentState = GameState.Active; Debug.Log("GameManager.cs, Started game!"); AudioManager.PlaySound(GameSound.GUI_MouseClick); });
	}

	private void Start()
	{
		AudioManager.PlaySound(GameSound.BackgroundAmbience);
		LoadingStateManager.BootstrapEvent?.Invoke();
	}

	private void Update()
	{
		
		UpdateInput();

		UpdateEnvironment();
	}

	private void Reset()
	{
		currentState = GameState.Active;

		if (!LoadingStateManager.SceneIsLoading)
			GameUIManager.s_Instance.loadingScreen.Display(false);
	}

	#endregion

	#region Private Methods

	private void UpdateEnvironment()
	{
		if (LoadingStateManager.SceneIsLoading || currentState.Equals(GameState.MainMenu) || currentState.Equals(GameState.Loading))
		{
			m_Environment.gameObject.SetActive(true);
		}
		else
		{
			m_Environment.gameObject.SetActive(false);
		}
	}

	private void UpdateInput()
	{

				if (Input.GetKeyDown(quitKeycode))
				{
					if (currentState.Equals(GameState.Active))
					{
						OnGameEndedEvent?.Invoke();
						LoadingStateManager.ReturnToMainScreen?.Invoke();
					}
					else if (currentState.Equals(GameState.MainMenu))
					{
						#if UNITY_STANDALONE
										Debug.Log("[MainMenu.QuitApplication]: " + "Quitting Application!");
										Application.Quit();
						#endif
						#if UNITY_EDITOR
										// Stop playing the scene 
										Debug.Log("[MainMenu.QuitApplication]: " + "Running in the Editor - Editor application has stopped playing!");
										UnityEditor.EditorApplication.isPlaying = false;
						#endif
					}
				}

	}

	#endregion

}
