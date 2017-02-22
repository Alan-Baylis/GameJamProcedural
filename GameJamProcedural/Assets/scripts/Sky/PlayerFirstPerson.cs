using UnityEngine;
using System.Collections;
// using UnityStandardAssets.CrossPlatformInput;

public class PlayerFirstPerson : MonoBehaviour {
    public CharacterController controller;
    public Camera renderCamera;
    public float xSensitivity = 3.5f;
    public float ySensitivity = 3.5f;
    public float walkSpeed = 1.3f;
    public float flyWalkSpeed = 3f;
    public float jogSpeed = 3f;
    public float flyJogSpeed = 10f;
    public float sprintSpeed = 10f;
    public float flySprintSpeed = 30f;
    public float jumpForce = 2f;
    
    public Vector3 velocity;
    
    Vector3 moveDirection;
    float xRotation;
    float yRotation;
    enum MovementSpeed {
        idle,
        walking,
        jogging,
        sprinting
    };
    MovementSpeed currentSpeed;
    bool toggleWalk = false;
    bool doJump = false;
    bool grounded = false;
    bool flymode = false;
    public bool autoMove = false;
    bool autoMoveLockForward = false;
    
    CollisionFlags collisionFlags;
    bool lockInput;

	void Start () {
        moveDirection = new Vector3(0, 0, 0);
        velocity = new Vector3(0, 0, 0);
        // SetCursorLock(true);
        currentSpeed = MovementSpeed.idle;
	}
	
	void Update () {
        if (Input.GetButtonDown("Flymode"))
        {
            flymode = !flymode;
            velocity = new Vector3(0, 0, 0);
        }
        
        if (lockInput)
        {
            yRotation = Input.GetAxis("Mouse X") * xSensitivity;
            xRotation += Input.GetAxis("Mouse Y") * ySensitivity;
            if (xRotation > 90)
                xRotation = 90;
            else if (xRotation < -90)
                xRotation = -90;
            renderCamera.transform.localRotation = Quaternion.Euler(-xRotation, 0, 0);
            transform.Rotate(new Vector3(0, yRotation, 0));
        }
        
        if (Input.GetButtonDown("Jump") && (grounded || flymode))
            doJump = true;
        else if (Input.GetButtonUp("Jump"))
            doJump = false;
        if (Input.GetButton("Sprint"))
            currentSpeed = MovementSpeed.sprinting;
        else if (Input.GetButton("Walk") && !toggleWalk)
            currentSpeed = MovementSpeed.walking;
        else
            currentSpeed = MovementSpeed.jogging;
        if (Input.GetButtonDown("Auto move"))
        {
            autoMove = !autoMove;
            autoMoveLockForward = autoMove;
        }
        
        float forwardInput = Input.GetAxisRaw("Vertical");
        float rightInput = Input.GetAxisRaw("Horizontal");
        if (autoMove)
        {
            if (forwardInput < -.25f)
                autoMove = false;
            if (forwardInput > .25f)
            {
                if (!autoMoveLockForward)
                    autoMove = false;
            }
            else
                autoMoveLockForward = false;
            if (autoMove)
                forwardInput = 1;
        }
        if (flymode)
        {
            moveDirection.z = Mathf.Sin(-yRotation / 180f * Mathf.PI) * rightInput + Mathf.Cos(-yRotation / 180f * Mathf.PI) * forwardInput * Mathf.Cos(xRotation / 180f * Mathf.PI);
            moveDirection.x = Mathf.Cos(yRotation / 180f * Mathf.PI) * rightInput + Mathf.Sin(yRotation / 180f * Mathf.PI) * forwardInput * Mathf.Cos(xRotation / 180f * Mathf.PI);
            moveDirection.y = Mathf.Sin(xRotation / 180f * Mathf.PI) * forwardInput;
        }
        else
        {
            moveDirection.z = forwardInput;
            moveDirection.x = rightInput;
        }
        if (moveDirection.magnitude < .01 && moveDirection.magnitude > -.01 && !doJump)
            currentSpeed = MovementSpeed.idle;
        moveDirection.Normalize();
        
        if (Input.GetKeyUp(KeyCode.Escape))
            SetCursorLock(false);
        else if(Input.GetMouseButtonUp(0))
            SetCursorLock(true);
	}
    
	void FixedUpdate () {
        float step = Time.fixedDeltaTime;
        
        if (!flymode)
            velocity += Physics.gravity*step;
        
        Vector3 movementVelocity = new Vector3(moveDirection.x, moveDirection.y, moveDirection.z);
        float movementSpeed;
        switch (currentSpeed)
        {
            case MovementSpeed.walking:
                if (flymode)
                    movementSpeed = flyWalkSpeed;
                else
                    movementSpeed = walkSpeed;
                break;
            case MovementSpeed.sprinting:
                if (flymode)
                    movementSpeed = flySprintSpeed;
                else
                    movementSpeed = sprintSpeed;
                break;
            case MovementSpeed.jogging:
            default:
                if (flymode)
                    movementSpeed = flyJogSpeed;
                else
                    movementSpeed = jogSpeed;
                break;
        }
        movementVelocity *= movementSpeed;
        
        if (grounded && !doJump && !flymode)
            movementVelocity.y -= controller.stepOffset/step;
        
        if (doJump)
        {
            if (flymode)
            {
                movementVelocity.y += movementSpeed;
            }
            else
            {
                velocity.y += jumpForce;
                doJump = false;
            }
        }
        
        if (flymode)
        {
            controller.transform.Translate(movementVelocity*step);
            collisionFlags = CollisionFlags.None;
        }
        else
        {
            Vector3 absoluteMovement = movementVelocity;
            absoluteMovement = transform.localRotation * absoluteMovement + velocity;
            collisionFlags = controller.Move(absoluteMovement * step);
        }
        if ((collisionFlags & CollisionFlags.Below) != 0)
        {
            velocity *= 0;
            grounded = true;
        }
        else
            grounded = false;
    }
    
    void SetCursorLock(bool lockCursor)
    {
        if (lockCursor && !lockInput)
        {
            lockInput = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!lockCursor && lockInput)
        {
            lockInput = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
