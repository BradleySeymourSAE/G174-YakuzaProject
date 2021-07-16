#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#endregion



[RequireComponent(typeof (Rigidbody))]
[RequireComponent(typeof (CapsuleCollider))]
public class RigidbodyFirstPersonController : MonoBehaviour
{

    [System.Serializable]
    public class MovementSettings
        {
            public float ForwardSpeed = 2.0f;   // Speed when walking forward
            public float BackwardSpeed = 2.0f;  // Speed when walking backwards
            public float StrafeSpeed = 2.0f;    // Speed when walking sideways
            public float RunMultiplier = 1.0f;   // Speed when sprinting
	        public KeyCode RunKey = KeyCode.LeftShift;
            public float JumpForce = 15f;
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [HideInInspector] public float CurrentTargetSpeed = 8f;

#if !MOBILE_INPUT
            private bool m_Running;
#endif

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
	            if (input == Vector2.zero) return;
				if (input.x > 0 || input.x < 0)
				{
					//strafe
					CurrentTargetSpeed = StrafeSpeed;
				}
				if (input.y < 0)
				{
					//backwards
					CurrentTargetSpeed = BackwardSpeed;
				}
				if (input.y > 0)
				{
					//forwards
					//handled last as if strafing and moving forward at the same time forwards speed should take precedence
					CurrentTargetSpeed = ForwardSpeed;
				}
#if !MOBILE_INPUT
	            if (Input.GetKey(RunKey))
	            {
		            CurrentTargetSpeed *= RunMultiplier;
		            m_Running = true;
	            }
	            else
	            {
		            m_Running = false;
	            }
#endif
            }

#if !MOBILE_INPUT
            public bool Running
            {
                get { return m_Running; }
            }
#endif
        }

    [System.Serializable]
    public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundHelperDistance = 0.5f; // stops the character
            public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
            public bool airControl; // can the user control the direction that is being moved in the air
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
        }

    [System.Serializable]
    public class InputSettings
	{
      public KeyCode interactKeyBinding = KeyCode.E;
      public KeyCode grabKeyBinding = KeyCode.Mouse1;
    }

    public enum UserInput { Interact, Grab }


    [Header("Scene Interaction")]
    [SerializeField] 
    [Min(5f)] 
    protected float m_MaximumViewDistance = 50f;
    [SerializeField]
    protected LayerMask m_InteractableUILayer;
    [SerializeField] 
    private IMouseClickable m_PreviousClickable = null;

    [Header("Movement")]
    public Camera m_Camera;
    public MovementSettings movementSettings = new MovementSettings();
    public MouseLook mouseLook = new MouseLook();
    public AdvancedSettings advancedSettings = new AdvancedSettings();
    public InputSettings inputSettings = new InputSettings();


    protected Rigidbody m_RigidBody;
    protected CapsuleCollider m_Capsule;
    private float m_YRotation;
    private Vector3 m_GroundContactNormal;
    private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;


	#region Getters / Setters 
	public Vector3 Velocity
        {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        public bool Jumping
        {
            get { return m_Jumping; }
        }

        public bool Running
        {
            get
            {
 #if !MOBILE_INPUT
				return movementSettings.Running;
#else
	            return false;
#endif
            }
        }

	#endregion

	#region Unity References

	private void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            mouseLook.Init (transform, m_Camera.transform);
        }

    private void Update()
    {
        RotateView();
        UpdateInput();

        if (Input.GetKeyDown(KeyCode.Space) && !m_Jump)
        {
          m_Jump = true;
        }
    }

    private void FixedUpdate()
        {
            GroundCheck();
            Vector2 input = GetInput();

            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
            {
                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = m_Camera.transform.forward*input.y + m_Camera.transform.right*input.x;
                desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                desiredMove.x = desiredMove.x*movementSettings.CurrentTargetSpeed;
                desiredMove.z = desiredMove.z*movementSettings.CurrentTargetSpeed;
                desiredMove.y = desiredMove.y*movementSettings.CurrentTargetSpeed;
                if (m_RigidBody.velocity.sqrMagnitude <
                    (movementSettings.CurrentTargetSpeed*movementSettings.CurrentTargetSpeed))
                {
                    m_RigidBody.AddForce(desiredMove*SlopeMultiplier(), ForceMode.Impulse);
                }
            }

            if (m_IsGrounded)
            {
                m_RigidBody.drag = 5f;

                if (m_Jump)
                {
                    m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                    m_Jumping = true;
                }

                if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
                {
                    m_RigidBody.Sleep();
                }
            }
            else
            {
                m_RigidBody.drag = 0f;
                if (m_PreviouslyGrounded && !m_Jumping)
                {
                    StickToGroundHelper();
                }
            }
            m_Jump = false;
        }

	#endregion

	#region 
	private void UpdateInput()
    {
        float interactionKeyPressed = ReturnUserInput(UserInput.Interact);
        float grabKeyPressed = ReturnUserInput(UserInput.Grab);

        Ray l_CurrentCameraRay = m_Camera.ScreenPointToRay(Input.mousePosition);
     
        if (interactionKeyPressed > 0)
        {
            Interact(l_CurrentCameraRay);
        }

        if (grabKeyPressed > 0)
        {
            Grab(l_CurrentCameraRay);
        }

 
       m_PreviousClickable = CheckMouseHoveringOverUI(l_CurrentCameraRay);

        if (GameManager.Debugging)
		{
            RaycastHit l_RaycastHit;

            if (Physics.Raycast(l_CurrentCameraRay, out l_RaycastHit, 30f))
            {
                Debug.DrawLine(l_CurrentCameraRay.origin, l_RaycastHit.point, Color.yellow);
            }
            else if (Physics.Raycast(l_CurrentCameraRay, out l_RaycastHit, 30f, m_InteractableUILayer))
            {
                Debug.DrawLine(l_CurrentCameraRay.origin, l_RaycastHit.point, Color.red);
            }
		}
    }

    private void Interact(Ray p_Raycast)
	{
        IMouseClickable l_Clickable;
        RaycastHit l_RaycastHit;

        // Check if mouse pointer is over UI game object 
        if (Utility.IsMousePointerOverUIObject())
		{
            if (GameManager.Debugging)
                Debug.Log("RigidbodyFirstPersonController.Interact: " + "Mouse pointer is over UI object!");

            foreach (RaycastResult result in Utility.GetUIGameObjectsUnderMousePointer())
			{
                // If the game object has IMouseClickable attached
                if (result.gameObject.GetComponent<IMouseClickable>() != null)
				{
                    l_Clickable = result.gameObject.GetComponent<IMouseClickable>();
                    l_Clickable.OnLeftClick();
				}
			}
		}
        // Otherwise, Check through a raycast 
        else if (Physics.Raycast(p_Raycast, out l_RaycastHit, m_MaximumViewDistance, m_InteractableUILayer))
		{
            if (GameManager.Debugging) { Debug.Log("[RigidbodyFirstPersonController.Interact]: " + "Drawing debug line."); Debug.DrawLine(p_Raycast.origin, l_RaycastHit.point, Color.red); }

            if (l_RaycastHit.transform.GetComponent<IMouseClickable>() != null)
			{
                l_Clickable = l_RaycastHit.transform.GetComponent<IMouseClickable>();
                l_Clickable.OnLeftClick();
			}
		}
	}

    private void Grab(Ray p_Raycast)
	{
        IMouseClickable l_Clickable;
        RaycastHit l_RaycastHit;

        if (Physics.Raycast(p_Raycast, out l_RaycastHit, m_MaximumViewDistance, m_InteractableUILayer))
		{
            if (GameManager.Debugging) {  Debug.Log("[RigidbodyFirstPersonController.Grab]: " + "Drawing debug line"); Debug.DrawLine(p_Raycast.origin, l_RaycastHit.point, Color.blue); }

            if (l_RaycastHit.transform.GetComponent<IMouseClickable>() != null)
			{
                l_Clickable = l_RaycastHit.transform.GetComponent<IMouseClickable>();
                l_Clickable.OnMouseButtonDrag();
			}
		}
	}

    private float ReturnUserInput(UserInput UserInputReceived)
	{
        float l_InputValue = 0;
		switch (UserInputReceived)
		{
			case UserInput.Interact:
                {
                    if (Input.GetKeyDown(inputSettings.interactKeyBinding))
					    l_InputValue = 1;
                    else
                        l_InputValue = -1;
                }
				break;
			case UserInput.Grab:
				{
                   if (Input.GetKey(inputSettings.grabKeyBinding))
                    l_InputValue = 1;
                   else
                    l_InputValue = 0;
				}
				break;
		}
        return l_InputValue;
	}

    public IMouseClickable CheckMouseHoveringOverUI(Ray p_Ray)
    {
        RaycastHit l_RaycastHit;

        // If mouse pointer is over UI 
        if (Utility.IsMousePointerOverUIObject())
        {
            IMouseClickable l_Clickable = null;

            foreach (RaycastResult l_Result in Utility.GetUIGameObjectsUnderMousePointer())
            {
                if (l_Result.gameObject.GetComponent<IMouseClickable>() != null)
                {
                    l_Clickable = l_Result.gameObject.GetComponent<IMouseClickable>();
                    break;
                }
            }

            // If mouse clickale item is not equal to the previous clickable item 
            if (l_Clickable != m_PreviousClickable)
            {
                if (l_Clickable != null)
                {
                    l_Clickable.OnMouseHoverEnter();
                }


                if (m_PreviousClickable != null)
                { 
                    m_PreviousClickable.OnMouseHoverExit();
                }

                m_PreviousClickable = l_Clickable;
            }

            return m_PreviousClickable;
        }
        // If the mouse pointer is over a game object 
        else if (Physics.Raycast(p_Ray, out l_RaycastHit))
        {
            // Get the mouse clickable component  
            IMouseClickable l_Clickable = l_RaycastHit.transform.GetComponent<IMouseClickable>();
            // If the clickable item is not the same as the previous 
            if (l_Clickable != m_PreviousClickable)
            {
                if (l_Clickable != null)  // If the clickable is not null 
                {
                    l_Clickable.OnMouseHoverEnter(); // Call on hover enter 
                }

                if (m_PreviousClickable != null) // if previous clickable item is not null 
                { 
                    m_PreviousClickable.OnMouseHoverExit(); // call on mouse hover exit 
                }

                // Set previous mouse clickable to the current clickable game object 
                m_PreviousClickable = l_Clickable;
            }

            // Return the last clickable item 
            return m_PreviousClickable;
        }
        
        // Otherwise, just return null 
        return null;
    }

    #endregion

    #region Private Methods (Movement) 

    private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }


    private void StickToGroundHelper()
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height/2f) - m_Capsule.radius) +
                                   advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
                }
            }
        }


    private Vector2 GetInput()
        {
            
            Vector2 input = new Vector2
                {
                    x = Input.GetAxis("Horizontal"),
                    y = Input.GetAxis("Vertical")
                };
			movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }


    private void RotateView()
        {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;

            mouseLook.LookRotation (transform, m_Camera.transform);

            if (m_IsGrounded || advancedSettings.airControl)
            {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation*m_RigidBody.velocity;
            }
        }

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
    private void GroundCheck()
        {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height/2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }
            if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            {
                m_Jumping = false;
            }
        }

    #endregion

}
