#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#endregion



public class Utility
{

	/// <summary>
	///		Checks if Vector2 Position (Input.mousePosition) is hovering over a UI Object 
	/// </summary>
	/// <returns></returns>
	public static bool IsMousePointerOverUIObject()
	{
		PointerEventData l_CurrentPointerData = new PointerEventData(EventSystem.current);
		l_CurrentPointerData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> l_AllRaycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(l_CurrentPointerData, l_AllRaycastResults);

		return l_AllRaycastResults.Count > 0;
	}

	/// <summary>
	///		Gets a list of game objects from a raycast result (Event System - Pointer Event Data) 
	/// </summary>
	/// <returns></returns>
	public static List<RaycastResult> GetUIGameObjectsUnderMousePointer()
	{
		PointerEventData l_EventDataCurrentPosition = new PointerEventData(EventSystem.current);
		l_EventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> l_Results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(l_EventDataCurrentPosition, l_Results);
		return l_Results;
	}

}