#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Rendering;
#endregion


[RequireComponent(typeof(SphereCollider))]
public class LightSwitchTrigger : MonoBehaviour, IMouseClickable
{

	public static UnityEvent DisplayLightSwitchUIEvent = new UnityEvent();

	[SerializeField] protected Transform m_TargetLight;

	[Header("Debug")]
	[SerializeField] protected Light m_Light;
	[SerializeField] protected Outline m_Outline;
	protected SphereCollider m_Collider;

	#region Getters / Setters

	public bool IsLightOn => m_Light.isActiveAndEnabled == true;

	#endregion

	#region Unity References

	private void Awake()
	{
		m_Outline = transform.GetComponentInChildren<Outline>();
		m_Collider = transform.GetComponent<SphereCollider>();
		m_Light = m_TargetLight.GetComponent<Light>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			Debug.Log("Player in trigger zone!");
		
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			Debug.Log("Player left trigger zone!");

		}
	}

	#endregion

	#region IMouseClickable Methods 
	
	public void OnLeftClick()
	{
		Debug.Log("LightSwitchTrigger: Interacting!");
		if (IsLightOn)
		{ 
			AudioManager.PlaySound(GameSound.LightSwitchOff);
			m_TargetLight.gameObject.SetActive(false);
			m_Light.gameObject.SetActive(false);
			Volume l_Volume = FindObjectOfType<Volume>();
			
		}
		else
		{
			AudioManager.PlaySound(GameSound.LightSwitchOn);
			m_TargetLight.gameObject.SetActive(true);
			m_Light.gameObject.SetActive(true);
		}
	}


	public void OnMouseHoverEnter() => m_Outline.DisplayOutline(true);

	public void OnMouseHoverExit() => m_Outline.DisplayOutline(false);

	public void OnMouseButtonDrag() { } 

	public void OnMouseButtonDragEnded() { } 

	#endregion

}
