#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion



[RequireComponent(typeof(Collider))]
public class Grabbable : MonoBehaviour, IMouseClickable
{
	[Header("Debugging")]
	[SerializeField] protected Rigidbody m_Rigidbody;
	[SerializeField] protected Collider m_Collider;
	[SerializeField] protected SphereCollider m_InteractCollider;
	[SerializeField] protected Outline m_Outline;
	
	[SerializeField] protected Vector3 offset = Vector3.zero;

	private Transform m_PlayerReference;
	private Transform m_WeaponAttach;
	protected Vector3 m_WeaponAttachPosition;

	private bool allowGrabbing = false;

	private void Awake()
	{
		m_Rigidbody = GetComponentInChildren<Rigidbody>();
		m_Outline = GetComponentInChildren<Outline>();
		m_Collider = GetComponentInChildren<Collider>();
		m_InteractCollider = GetComponent<SphereCollider>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			allowGrabbing = true;
			m_PlayerReference = other.gameObject.transform;
			m_WeaponAttach = m_PlayerReference.Find("HandAttach").transform;

			if (m_WeaponAttach)
				m_WeaponAttachPosition = m_WeaponAttach.position;

		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			allowGrabbing = false;
			m_PlayerReference = null;
			m_WeaponAttachPosition = Vector3.zero;
			m_WeaponAttach = null;
		}
	}

	private void Update()
	{
		if (m_WeaponAttach != null && m_PlayerReference)
			m_WeaponAttachPosition = m_WeaponAttach.position;
	}

	private void OnMouseDown()
	{
		if (allowGrabbing && m_PlayerReference)
		{
			Debug.Log("Mouse down");
			m_Rigidbody.useGravity = false;
			m_Collider.enabled = false;
			transform.position = m_WeaponAttachPosition;
			transform.parent = m_PlayerReference;
		}
	}
	private void OnMouseUp()
	{
		m_Rigidbody.useGravity = true;
		m_Collider.enabled = true;
		transform.parent = null;
		m_PlayerReference = null;
	}

	#region IMouseClickable Methods 

	public void OnLeftClick() { } 

	public void OnMouseButtonDrag() { } 

	public void OnMouseButtonDragEnded() { } 

	public void OnMouseHoverEnter() => m_Outline.DisplayOutline(true);
	public void OnMouseHoverExit() => m_Outline.DisplayOutline(false);

	#endregion

}
