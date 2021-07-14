#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion



public class JailcellSceneManager : MonoBehaviour
{

	#region Private Variables 
	[SerializeField] private Transform m_CurrentPlayer;
	[SerializeField] private GameObject m_PlayerPrefab;
	[SerializeField] private Transform m_PlayerSpawnPoint;

	[Header("Debug")]
	[SerializeField][Range(0.1f, 3f)] private float m_GizmoRadius = 1f;
	[SerializeField] private Color m_SpawnPointColor = Color.red;
	[SerializeField] private bool debugging = false, useWireSphere = false;

	#endregion

	#region Unity References

	private void OnEnable()
	{
		GameManager.OnGameStartEvent.AddListener(() => SpawnPlayer());
	}

	private void OnDisable()
	{
		GameManager.OnGameStartEvent.RemoveListener(() => SpawnPlayer());
	}

	#region Debug 

	private void OnDrawGizmos()
	{
		if (debugging)
		{
			Gizmos.color = m_SpawnPointColor;
			if (useWireSphere)
				Gizmos.DrawWireSphere(m_PlayerSpawnPoint.position, m_GizmoRadius);
			else
				Gizmos.DrawSphere(m_PlayerSpawnPoint.position, m_GizmoRadius);
		}
	}

	#endregion

	#endregion

	private void SpawnPlayer()
	{
		if (m_PlayerPrefab != null)
		{
			GameObject l_SpawnedPlayer = Instantiate(m_PlayerPrefab, m_PlayerSpawnPoint.position, m_PlayerPrefab.transform.rotation);
			m_CurrentPlayer = l_SpawnedPlayer.transform;
			
			if (m_CurrentPlayer.GetComponent<RigidbodyFirstPersonController>())
				m_CurrentPlayer.GetComponent<RigidbodyFirstPersonController>().cam = Camera.main;

		}
	}

	private void Update()
	{
		debugging = GameManager.Debugging;
	}
}
