#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion



public class JailcellSceneManager : MonoBehaviour
{

	#region Private Variables 

	[SerializeField] private GameObject m_PlayerPrefab;
	[SerializeField] private Transform m_PlayerSpawnPoint;

	[Header("Debug")]
	[SerializeField][Range(0.1f, 3f)] private float m_GizmoRadius = 1f;
	[SerializeField] private Color m_SpawnPointColor = Color.red;
	[SerializeField] private bool debugging = false, useWireSphere = false;
	[SerializeField] protected Transform m_CurrentPlayer;
	[SerializeField] protected Transform m_Camera;

	#endregion

	protected Coroutine m_SpawnRoutine;

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

	private void OnEnable()
	{
		GameManager.OnGameEndedEvent.AddListener(DespawnPlayer);
	}

	private void OnDisable()
	{
		GameManager.OnGameEndedEvent.RemoveListener(DespawnPlayer);
	}
	#endregion

	private void Start()
	{
		if (m_SpawnRoutine != null)
			StopCoroutine(m_SpawnRoutine);

		m_Camera = Camera.main.transform;
		m_SpawnRoutine = StartCoroutine(SpawnPlayer());
	}

	private void DespawnPlayer()
	{
		m_Camera.GetComponentInChildren<Camera>().enabled = true;
		Destroy(m_CurrentPlayer.gameObject);
	}

	private IEnumerator SpawnPlayer()
	{
		yield return new WaitForSeconds(0.25f);

			if (m_Camera)
			{ 
				var l_SpawnedPlayer = Instantiate(m_PlayerPrefab, m_PlayerSpawnPoint.position, m_PlayerPrefab.transform.rotation);
				m_Camera.GetComponentInChildren<Camera>().enabled = false;
				if (l_SpawnedPlayer != null)
			    { 
					m_CurrentPlayer = l_SpawnedPlayer.transform;
			    }
			}

		yield return null;
	}

	private void Update()
	{
		debugging = GameManager.Debugging;
	}
}
