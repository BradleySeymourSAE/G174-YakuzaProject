#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Rendering;
#endregion


[RequireComponent(typeof(Collider))]
public class LightSwitchTrigger : MonoBehaviour, IMouseClickable
{

	public static UnityEvent DisplayLightSwitchUIEvent = new UnityEvent();

	private const string PostProcessingTag = "PostProcessing";
	
	[SerializeField] protected Transform m_PostProcessing; 
	[SerializeField] protected Volume m_PostProcessingVolume;
	[SerializeField] private float m_TargetProcessingWeight = 1;
	[SerializeField] protected Transform m_TargetLight;
	protected Transform m_PlayerReference = null;

	[Header("Debug")]
	[SerializeField] protected Light m_Light;
	[SerializeField] protected Outline m_Outline;
	[SerializeField] protected RectTransform m_Canvas;

	[SerializeField] private float m_CurrentLightWeight;
	[SerializeField] protected Collider m_LightSwitchTriggerZone;

	private bool insideTriggerZone = false;

	#region Getters / Setters

	public bool IsLightOn => m_Light.isActiveAndEnabled == true;

	#endregion

	#region Unity References

	private void Awake()
	{
		m_Outline = transform.GetComponentInChildren<Outline>();
		m_Light = m_TargetLight.GetComponent<Light>();
		m_PostProcessing = GameObject.Find(PostProcessingTag).transform;
		m_Canvas = transform.GetComponentInChildren<RectTransform>();
		m_LightSwitchTriggerZone = GetComponent<Collider>();
		m_LightSwitchTriggerZone.isTrigger = true;
		m_LightSwitchTriggerZone.enabled = true;
	}

	private void Start()
	{
		if (GameObject.FindGameObjectWithTag(PostProcessingTag))
			m_PostProcessing = GameObject.FindGameObjectWithTag(PostProcessingTag).transform;

			m_PostProcessingVolume = m_PostProcessing.GetComponent<Volume>();
			m_CurrentLightWeight = m_PostProcessingVolume.weight;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			Debug.Log("Player in trigger zone!");
			m_PlayerReference = other.gameObject.transform;
			insideTriggerZone = true;
			DisplayUI(true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			insideTriggerZone = false;
			Debug.Log("Player left trigger zone!");
			m_PlayerReference = null;
			DisplayUI(false);
		}
	}

	private void DisplayUI(bool p_ShouldDisplayCanvas = false)
	{

		if (m_PlayerReference != null && p_ShouldDisplayCanvas)
		{
			var l_DisplayCanvas = m_Canvas.GetComponent<Canvas>();
			l_DisplayCanvas.worldCamera = m_PlayerReference.GetComponentInChildren<Camera>();
			m_Canvas.gameObject.SetActive(p_ShouldDisplayCanvas);
		}
		else
		{ 
			m_Canvas.gameObject.SetActive(p_ShouldDisplayCanvas);
		}
	}

	#endregion

	#region IMouseClickable Methods 
	
	public void OnLeftClick()
	{
		if (!insideTriggerZone)
			return;


		Debug.Log("LightSwitchTrigger: Interacting!");
		if (IsLightOn)
		{ 
			AudioManager.PlaySound(GameSound.LightSwitchOff);
			m_TargetLight.gameObject.SetActive(false);
			m_Light.gameObject.SetActive(false);
			m_PostProcessingVolume.weight = 0.02f;
		}
		else
		{
			AudioManager.PlaySound(GameSound.LightSwitchOn);
			m_TargetLight.gameObject.SetActive(true);
			m_Light.gameObject.SetActive(true);
			m_PostProcessingVolume.weight = 1f;
		}
	}


	public void OnMouseHoverEnter() => m_Outline.DisplayOutline(true);

	public void OnMouseHoverExit() => m_Outline.DisplayOutline(false);

	public void OnMouseButtonDrag() { } 

	public void OnMouseButtonDragEnded() { } 

	#endregion

}
