#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
#endregion



public enum GameSound 
{ 
	NONE,
	BackgroundAmbience,
	StaticRadio,
	LightSwitchOn,
	LightSwitchOff,
	GUI_MouseClick,
	GUI_ForwardNavigation,
	GUI_BackwardsNavigation
};


/// <summary>
///		Static Audio Manager Class
/// </summary>
public static class AudioManager
{


	/// <summary>
	///		Reference to the Audio Mixer Group 
	/// </summary>
	private static AudioMixerGroup m_AudioMixer;

	/// <summary>
	///		Reference to the Game Assets Script 
	/// </summary>
	private static GameAssets m_GameAssetsReference;

	#region Public Methods 

	/// <summary>
	///		Sets up the audio manager references 
	/// </summary>
	/// <param name="p_GameAssets"></param>
	/// <param name="p_AllGameSoundEffects"></param>
	/// <param name="p_AudioMixer"></param>
	public static void SetupGameSounds(GameAssets p_GameAssets, GameSoundEffect[] p_AllGameSoundEffects, AudioMixerGroup p_AudioMixer)
	{

		m_AudioMixer = p_AudioMixer;
		m_GameAssetsReference = p_GameAssets;

		if (m_GameAssetsReference != null && p_AllGameSoundEffects.Length > 0)
		{
			foreach (GameSoundEffect sound in p_AllGameSoundEffects)
			{

				// Add the audio source component to the game assets game object 
				sound.source = p_GameAssets.gameObject.AddComponent<AudioSource>();

				// Set the audio source clip to the audio clip added  
				sound.source.clip = sound.clip;

				// Set whether the audio source should loop 
				sound.source.loop = sound.loop;

				// Set whether the audio source should play on awake 
				sound.source.playOnAwake = sound.awake;

				// Set the pitch of the audio source 
				sound.source.pitch = sound.pitch;

				// Set the volume of the audio source 
				sound.source.volume = sound.volume;

				// Set the output audio mixer group for the game sound effect's audio source 
				sound.source.outputAudioMixerGroup = m_AudioMixer;
			}
		}
		else
		{
			Debug.LogWarning("[AudioManager.SetupSounds]: " + "Could not find Game Assets Reference or total game sounds less than 0!");
		}
	}

	/// <summary>
	///		Check whether a game sound effect is currently being played 
	/// </summary>
	/// <param name="p_SoundEffect"></param>
	/// <returns></returns>
	public static bool IsPlayingSound(GameSound p_SoundEffect) => ReturnGameSound(p_SoundEffect).source.isPlaying == true;

	/// <summary>
	///		Handles playing a game sound effect 
	/// </summary>
	/// <param name="p_GameSound">Enum - The Game Sound to play</param>
	public static void PlaySound(GameSound p_GameSound)
	{
		var l_Sound = ReturnGameSound(p_GameSound);

		if (l_Sound == null)
		{
			Debug.LogWarning("[AudioManager.PlaySound]: " + "Could not find game sound effect: " + p_GameSound);
			return;
		}

		l_Sound.source.volume = l_Sound.volume;
		l_Sound.source.playOnAwake = l_Sound.awake;
		l_Sound.source.loop = l_Sound.loop;
		l_Sound.source.pitch = l_Sound.pitch;
		l_Sound.source.Play();
	}

	/// <summary>
	///		Finds the sound effect's audio source and stops playing the audio - if already playing.
	/// </summary>
	/// <param name="p_GameSound">Enum - The Game Sound  to stop playing</param>
	public static void StopPlayingSound(GameSound p_GameSound)
	{
		var l_Sound = ReturnGameSound(p_GameSound);
		if (l_Sound == null)
		{
			Debug.LogWarning("[AudioManager.StopPlayingSound]: " + "Could not find the game sound: " + p_GameSound);
			return;
		}
		if (l_Sound.source.isPlaying)
		{ 
			l_Sound.source.Stop();
		}
	}

	#endregion

	#region Private Methods 

	/// <summary>
	///		Finds a GameSoundEffect by using an enum 'GameSound', its name. 
	/// </summary>
	/// <param name="p_GameSound">The sound effect to search for</param>
	/// <returns>Returns a game sound effect if found, otherwise returns null.</returns>
	private static GameSoundEffect ReturnGameSound(GameSound p_GameSound)
	{
		if (!m_GameAssetsReference)
		{
			Debug.LogWarning("Game Assets Reference has not been setup.");
			return null;
		}
		foreach (GameSoundEffect s_GameSoundEffect in GameAssets.instance.AllGameSoundEffects)
		{
			if (s_GameSoundEffect.Sound == p_GameSound)
			{ 
				return s_GameSoundEffect;
			}
		}
		return null;
	}

	#endregion

}
