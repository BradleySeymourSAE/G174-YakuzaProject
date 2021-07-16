#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#endregion


/// <summary>
///		Creates an outline around a game object using the Outline Shader 
/// </summary>
public class Outline : MonoBehaviour
{

	/// <summary>
	///		The renderer component for the outline 
	/// </summary>
	[SerializeField] private Renderer m_Renderer;
	[SerializeField] protected OutlineData m_OutlineSettings;
	private bool m_OutlineMaterialHasBeenSet = false;



	/// <summary>
	///		Creates & Sets the Game Object's Outline Material 
	/// </summary>
	/// <param name="p_ShouldDisplayOutlineMaterial"></param>
	public void DisplayOutline(bool p_ShouldDisplayOutlineMaterial)
	{
		if (m_OutlineMaterialHasBeenSet == false)
		{
			m_Renderer = CreateOutline(m_OutlineSettings.outlineMaterial, m_OutlineSettings.outlineScale, m_OutlineSettings.outlineColor);
			m_OutlineMaterialHasBeenSet = true;
		}

		if (m_OutlineMaterialHasBeenSet && p_ShouldDisplayOutlineMaterial)
			m_Renderer.enabled = true;
		else
			m_Renderer.enabled = false;

		Debug.Log("Outline Renderer Enabled: " + m_Renderer.enabled);
	}

	/// <summary>
	///		Creates an outline around the game object, useful for handling the selection of objects 
	/// </summary>
	/// <param name="p_OutlineMaterial">The outline material</param>
	/// <param name="p_ScalingFactor">The scaling factor for the outline</param>
	/// <param name="p_OutlineColor">The primary color of the outline</param>
	/// <returns></returns>
	public Renderer CreateOutline(Material p_OutlineMaterial, float p_ScalingFactor, Color p_OutlineColor)
	{
		GameObject l_ClonedOutlineGameObject = Instantiate(gameObject, transform.position, transform.rotation, transform);
		Renderer l_Renderer = l_ClonedOutlineGameObject.GetComponentInChildren<Renderer>();
		l_ClonedOutlineGameObject.transform.localScale = Vector3.one;

		// Apply the outline material to the cloned outline game object component 
			l_Renderer.material = p_OutlineMaterial;
			l_Renderer.material.SetColor("_OutlineColor", p_OutlineColor);
			l_Renderer.material.SetFloat("_Scale", -p_ScalingFactor);
			l_Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			l_Renderer.enabled = false;

		// Disable the outline game object 
		l_ClonedOutlineGameObject.GetComponentInChildren<Outline>().enabled = false;

		// Then disable the collider aswell
		if (l_ClonedOutlineGameObject.GetComponent<Collider>())
			l_ClonedOutlineGameObject.GetComponent<Collider>().enabled = false;
		else if (l_ClonedOutlineGameObject.GetComponent<MeshCollider>())
			l_ClonedOutlineGameObject.GetComponent<MeshCollider>().enabled = false;
		else if (l_ClonedOutlineGameObject.GetComponent<SphereCollider>())
			l_ClonedOutlineGameObject.GetComponent<SphereCollider>().enabled = false;
		else if (l_ClonedOutlineGameObject.GetComponent<CapsuleCollider>())
			l_ClonedOutlineGameObject.GetComponent<CapsuleCollider>().enabled = false;
		else if (l_ClonedOutlineGameObject.GetComponentInChildren<Collider>())
			l_ClonedOutlineGameObject.GetComponentInChildren<Collider>().enabled = false;
		else if (l_ClonedOutlineGameObject.GetComponentInChildren<MeshCollider>())
			l_ClonedOutlineGameObject.GetComponentInChildren<MeshCollider>().enabled = false;
		else if (l_ClonedOutlineGameObject.GetComponentInChildren<SphereCollider>())
			l_ClonedOutlineGameObject.GetComponentInChildren<SphereCollider>().enabled = false;
		else if (l_ClonedOutlineGameObject.GetComponentInChildren<CapsuleCollider>())
			l_ClonedOutlineGameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;

		if (l_ClonedOutlineGameObject.GetComponentInChildren<Rigidbody>())
			Destroy(l_ClonedOutlineGameObject.GetComponentInChildren<Rigidbody>());

		// Then return the renderer settings 
		return l_Renderer;
	}
}
