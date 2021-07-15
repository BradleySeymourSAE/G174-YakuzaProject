#region Namespaces
using UnityEngine;
#endregion

[CreateAssetMenu(fileName = "Outline Settings", menuName = "Shaders/Outline/Create")]
public class OutlineData : ScriptableObject
{ 
	public Material outlineMaterial;
	[Range(1.0f, 1.3f)] public float outlineScale = 1.05f;
	public Color outlineColor = Color.grey;
}
