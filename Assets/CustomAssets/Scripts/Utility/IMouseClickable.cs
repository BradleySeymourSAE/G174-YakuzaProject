


public interface IMouseClickable
{
	/// <summary>
	///		What happens when on Left Click called 
	/// </summary>
	void OnLeftClick();

	/// <summary>
	///		What happens when the right mouse button has started to drag 
	/// </summary>
	void OnMouseButtonDrag();
	/// <summary>
	///		What happens when the right mouse button has stopped being held down 
	/// </summary>
	void OnMouseButtonDragEnded();
	/// <summary>
	///		What happens when the mouse has entered 
	/// </summary>
	void OnMouseHoverEnter();
	/// <summary>
	///		What happens when the mouse exit's 
	/// </summary>
	void OnMouseHoverExit();
}