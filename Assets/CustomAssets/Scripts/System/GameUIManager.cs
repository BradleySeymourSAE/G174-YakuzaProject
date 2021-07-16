#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Linq;
#endregion


/// <summary>
///		Managers UI Screens in the Game 
/// </summary>

public class GameUIManager : MonoBehaviour
{

	#region Static
	
	public static GameUIManager s_Instance;

	#endregion

	#region Protected

	[SerializeField] protected List<BaseUIScreen> m_AllScreens = new List<BaseUIScreen>();

	#endregion

	#region Public Variables

	public MainMenuScreen mainMenuScreen;
	public LoadingScreen loadingScreen;
	public CreditsMenuScreen creditsScreen;
	// public OnScreenUI onScreenDisplay;

	#endregion

	#region Unity References

	private void OnEnable()
	{
		if (s_Instance != null && s_Instance != this)
			Destroy(s_Instance.gameObject);

		s_Instance = this;

		LoadingStateManager.MainScreenLoadingEvent.AddListener(() => EnableScreen(mainMenuScreen));
		LoadingStateManager.LoadingEvent.AddListener(() => EnableScreen(loadingScreen));
	}

	private void OnDisable()
	{
		LoadingStateManager.MainScreenLoadingEvent.RemoveListener(() => EnableScreen(mainMenuScreen));
		LoadingStateManager.LoadingEvent.RemoveListener(() => EnableScreen(loadingScreen));
	}

	protected void Awake() => Setup();

	protected void Start() { }

	protected void Update() => UpdateAll();
	
	#endregion

	#region Public Methods 

	public void DisplayCredits(bool p_ShouldDisplayCredits)
	{
		if (p_ShouldDisplayCredits)
			EnableScreen(creditsScreen);
		else
			EnableScreen(mainMenuScreen);

		AudioManager.PlaySound(GameSound.GUI_BackwardsNavigation);
	}

	#endregion

	#region Private Methods

	private void EnableScreen(BaseUIScreen p_Screen)
	{
		HideAll();
		p_Screen.Display(true);
	}

	private void Setup()
	{
		AddScreens();
		foreach (BaseUIScreen l_Screen in m_AllScreens)
		{
			l_Screen.Setup();
			l_Screen.SetupButtonMethods();
		}
	}

	private void AddScreens()
	{
		m_AllScreens.Add(mainMenuScreen);
		m_AllScreens.Add(creditsScreen);
		m_AllScreens.Add(loadingScreen);
		// m_AllScreens.Add(onScreenDisplay);
	}

	private void HideAll()
	{
		foreach (BaseUIScreen l_Screen in m_AllScreens)
			l_Screen.Display(false);
	}

	private void UpdateAll()
	{
		foreach (BaseUIScreen l_Screen in m_AllScreens)
			l_Screen.Update();
	}

	#endregion

	#region UI Data Classes 

	[System.Serializable]
	public class BaseUIScreen
	{
		public GameObject screen;

		public virtual bool IsEnabled() => screen.activeSelf == true;

		public virtual void Display(bool p_ShouldDisplayScreen)
		{
			screen.SetActive(p_ShouldDisplayScreen);
		}

		public virtual void Update()
		{
			if (!IsEnabled())
				return;
		}

		public virtual void Start() { if (!IsEnabled()) return; }

		public virtual void SetupButtonMethods() 
		{
		
		}

		public virtual void Setup() { }
	}

	[System.Serializable]
	public class MainMenuScreen : BaseUIScreen
	{
		#region Public Variables
		
		public TMP_Text gameTitle;
		public TMP_Text gameSubtitle;
		public TMP_Text gameVersion;

		public Button startButton;
		public Button creditsButton;
		public Button leaveButton;

		#endregion

		#region Public Methods

		public override void Setup()
		{
			gameTitle.text = GameDetails.Title;
			gameSubtitle.text = GameDetails.Catchline;
			gameVersion.text = $"{GameDetails.Environment} - {GameDetails.Version}";
		}

		public override void SetupButtonMethods()
		{
			UI.SetupButtonMethods(startButton, () => GameManager.OnGameStartEvent?.Invoke(), UI.MainMenu_StartButton);
			UI.SetupButtonMethods(creditsButton, () => GameUIManager.s_Instance.DisplayCredits(true), UI.MainMenu_CreditsButton);
			UI.SetupButtonMethods(leaveButton, () => Application.Quit(), UI.MainMenu_LeaveButton);
		}

		#endregion


	}

	[System.Serializable]
	public class LoadingScreen : BaseUIScreen
	{

		#region Public Variables

		public TMP_Text loadingStatus;

		#endregion

		#region Protected Variables
		
		protected Coroutine m_LoadingStatus;
	
		[SerializeField]
		[Range(0.5f, 4f)]
		protected float m_LoadingStatusUpdateInterval = 2.5f;

		#endregion

		#region Public Methods

		public override void Display(bool p_ShouldDisplayScreen)
		{
			if (m_LoadingStatus != null)
				GameUIManager.s_Instance.StopCoroutine(m_LoadingStatus);

			if (p_ShouldDisplayScreen)
				m_LoadingStatus = GameUIManager.s_Instance.StartCoroutine(UpdateStatus());

			base.Display(p_ShouldDisplayScreen);
		}

		#endregion

		#region Private Methods

		private IEnumerator UpdateStatus()
		{
			while (true)
			{
				loadingStatus.text = GameDetails.LoadingStatusQuotes[Random.Range(0, GameDetails.LoadingStatusQuotes.Count - 1)];
				yield return new WaitForSeconds(m_LoadingStatusUpdateInterval);
			}
			#pragma warning disable 0162
			yield return null;
		}

		#endregion

	}

	[System.Serializable]
	public class CreditsMenuScreen : BaseUIScreen
	{

		#region Public Variables

		public TMP_Text creditsTitle;
		public TMP_Text developerHeading;
		public TMP_Text developer;
		public Button closeButton;

		#endregion

		#region Public Methods 

		public override void Setup()
		{
			creditsTitle.text = UI.CreditsMenu_Title;
			developerHeading.text = UI.CreditsMenu_Developer_Heading;
		}

		public override void SetupButtonMethods() => UI.SetupButtonMethods(closeButton, () => GameUIManager.s_Instance.DisplayCredits(false), UI.CreditsMenu_CloseButton);
		
		#endregion

	}
	#endregion

}

public class UI
{

	#region Public Static 

	#region Main Menu 

	public const string MainMenu_StartButton = "Play Game";
	public const string MainMenu_CreditsButton = "Credits";
	public const string MainMenu_LeaveButton = "Leave";

	#endregion

	#region Credits Menu 
	
	public const string CreditsMenu_Title = "Prison Break - Credits";
	public const string CreditsMenu_Developer_Heading = "G174 Shippable Assets Project\nCreated by Bradley Seymour 21T2";
	public const string CreditsMenu_CloseButton = "Return";

	#endregion

	#endregion

	#region Public Methods 

	public static void SetupButtonMethods(Button p_ButtonToSetup, UnityAction p_UnityAction, string p_ButtonText)
	{
		p_ButtonToSetup.onClick.RemoveAllListeners();
		p_ButtonToSetup.onClick.AddListener(p_UnityAction);

		if (p_ButtonToSetup.gameObject.GetComponentInChildren<Text>())
			p_ButtonToSetup.gameObject.GetComponentInChildren<Text>().text = p_ButtonText;
		else if (p_ButtonToSetup.gameObject.GetComponentInChildren<TMP_Text>())
			p_ButtonToSetup.gameObject.GetComponentInChildren<TMP_Text>().text = p_ButtonText;
	}

	#endregion

}
