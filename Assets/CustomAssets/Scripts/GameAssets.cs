#region Namespaces
using UnityEngine;
using UnityEngine.Audio;
using System.Reflection;
#endregion



public class GameAssets : MonoBehaviour
{

	#region Static 
	
	public static GameAssets instance;

	#endregion

	#region Protected

	[Header("Game Audio")]
	[SerializeField] protected AudioMixerGroup m_AudioMixer;
	[SerializeField] protected GameSoundEffect[] m_GameSounds;

	#endregion

	#region Getters / Setters 

	public GameSoundEffect[] AllGameSoundEffects => m_GameSounds;

	#endregion

	#region Unity References

	private void OnEnable()
	{
		if (instance != null && instance != this)
		{
			Destroy(instance.gameObject);
		}
		instance = this;


		if (m_AudioMixer != null && m_GameSounds.Length > 0)
		{
			AudioManager.SetupGameSounds(this, m_GameSounds, m_AudioMixer);
		}
	}

	#endregion

}

/// <summary>
///		A custom data class for game sound effects 
/// </summary>
[System.Serializable]
public class GameSoundEffect
{

	/// <summary>
	///		The Sound Effect Category Name - This is used to find and play the sound effect 
	/// </summary>
	public GameSound Sound;

	/// <summary>
	///		The name of the sound effect 
	/// </summary>
	public string name;

	/// <summary>
	///		Whether the sound effect should loop 
	/// </summary>
	public bool loop = false; // default to false  

	/// <summary>
	///		Whether the sound should play on awake or not 
	/// </summary>
	public bool awake = false; 

	/// <summary>
	///		The volume for the sound effect 
	/// </summary>
	public float volume = 0.5f; // default to 1f 

	/// <summary>
	///		The Pitch for the sound fx 
	/// </summary>
	[Range(0.1f, 3f)] public float pitch = 1f; // default to 1f

	/// <summary>
	///		The Sound Effect Clip 
	/// </summary>
	public AudioClip clip;

	/// <summary>
	///		The Audio Source for the Sound Effect 
	/// </summary>
	[HideInInspector] public AudioSource source;

}