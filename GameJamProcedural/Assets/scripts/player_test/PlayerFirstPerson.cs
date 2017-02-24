using UnityEngine;
using System.Collections;

public class PlayerFirstPerson : MonoBehaviour {
    public CharacterController controller;
    public Camera renderCamera;
    public BlinkDistortion blinkDistortionEffect;
    public float xSensitivity = 3.5f;
    public float ySensitivity = 3.5f;
    public float walkSpeed = 1.3f;
    public float flyWalkSpeed = 3f;
    public float jogSpeed = 3f;
    public float flyJogSpeed = 10f;
    public float sprintSpeed = 10f;
    public float flySprintSpeed = 30f;
    public float jumpForce = 2f;
    public float blinkDecreaseRate = 70f;
    public float blinkIncreaseRate = 4f;
    public float blinkVelocityIncreaseRate = .05f;
    public float blinkGroundedDecreaseRate = 20f;
    public float blinkCooldown = 1f;
    public UnityEngine.UI.Image uiBlinkFillBar;
    public UnityEngine.UI.Image uiHealthBar;
    public UnityStandardAssets.ImageEffects.VignetteAndChromaticAberration hitShader;
    float currentHealth = 100;
    
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
    bool blinking = false;
    public bool flymode = false;
    public bool autoMove = false;
    bool autoMoveLockForward = false;
    float blinkFactor = 0;
    float currentBlinkPoints = 100f;
    float defaultBlinkPoints = 100f;
    float blinkCooldownTimer = 0;
    
    CollisionFlags collisionFlags;
    float hitFXTimer = 0;
    float hitFXDuration = .3f;
    public bool lockInput;

    //Debug Menu to appear
    public DayNight dayNight;
    bool debugMenu;

	void Start () {
        moveDirection = new Vector3(0, 0, 0);
        velocity = new Vector3(0, 0, 0);
        // SetCursorLock(true);
        currentSpeed = MovementSpeed.idle;
        
        defaultBlinkPoints = uiBlinkFillBar.fillAmount * 100;
        currentBlinkPoints = defaultBlinkPoints;
        debugMenu = dayNight.debugMenu;
	}
	
	void Update () {
        //DebugMenu
        debugMenu = dayNight.debugMenu;
        //

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
        
        if (Input.GetButtonDown("Fire1") && currentBlinkPoints > .5*defaultBlinkPoints && blinkCooldownTimer < 0 && !debugMenu)
            blinking = true;
        if (Input.GetButtonUp("Fire1") && blinking)
        {
            blinking = false;
            blinkCooldownTimer = blinkCooldown;
        }
        blinkCooldownTimer -= Time.deltaTime;
        if (blinking)
        {
            blinkFactor += Time.deltaTime*8;
            currentBlinkPoints -= blinkFactor * Time.deltaTime * blinkDecreaseRate;
            if (currentBlinkPoints < 0)
                blinking = false;
        }
        else
        {
            blinkFactor -= Time.deltaTime*2;
            if (currentBlinkPoints < defaultBlinkPoints)
            {
                currentBlinkPoints += (1-blinkFactor) * Time.deltaTime * blinkIncreaseRate;
                currentBlinkPoints = Mathf.Clamp(currentBlinkPoints, 0, defaultBlinkPoints);
            }
        }
        if (!grounded && currentBlinkPoints < 100)
        {
            currentBlinkPoints += (1-blinkFactor) * Time.deltaTime * blinkVelocityIncreaseRate * velocity.magnitude;
            currentBlinkPoints = Mathf.Clamp(currentBlinkPoints, 0, 100);
        }
        else if (grounded && currentBlinkPoints > defaultBlinkPoints)
        {
            currentBlinkPoints -= (1-blinkFactor) * Time.deltaTime * blinkGroundedDecreaseRate;
            currentBlinkPoints = Mathf.Clamp(currentBlinkPoints, defaultBlinkPoints, 100);
        }
        blinkFactor = Mathf.Clamp(blinkFactor, 0, 1);
        blinkDistortionEffect.intensity = blinkFactor * 80;
        uiBlinkFillBar.fillAmount = currentBlinkPoints / 100f;
        uiHealthBar.fillAmount = currentHealth / 100;
        if (currentBlinkPoints < defaultBlinkPoints*.5f)
            uiBlinkFillBar.color = Color.red;
        else if (currentBlinkPoints > defaultBlinkPoints*1.01f)
            uiBlinkFillBar.color = Color.green;
        else
            uiBlinkFillBar.color = Color.white;
        
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
        if(debugMenu)
        {
            SetCursorLock(false);
        }
        else if (Input.GetKeyUp(KeyCode.Escape))
            SetCursorLock(false);
        else if(Input.GetMouseButtonUp(0))
            SetCursorLock(true);
        
        hitShader.blur = .5f * (hitFXTimer / hitFXDuration);
        hitShader.chromaticAberration = 15 * (hitFXTimer / hitFXDuration);
        hitFXTimer -= Time.deltaTime;
        if (hitFXTimer < 0)
            hitFXTimer = 0;
	}
    
	void FixedUpdate () {
        float step = Time.fixedDeltaTime;
        
        if (!flymode)
            velocity += Physics.gravity*step * 2;
        velocity *= 1 - blinkFactor * step;
        
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
        
        movementVelocity *= 1-blinkFactor;
        Vector3 blink = Vector3.zero;
        blink.z = Mathf.Cos(-yRotation / 180f * Mathf.PI) * Mathf.Cos(xRotation / 180f * Mathf.PI);
        blink.x = Mathf.Sin(yRotation / 180f * Mathf.PI) * Mathf.Cos(xRotation / 180f * Mathf.PI);
        blink.y = Mathf.Sin(xRotation / 180f * Mathf.PI);
        blink *= blinkFactor * 90;
        
        if (flymode)
        {
            controller.transform.Translate((movementVelocity+blink)*step);
            collisionFlags = CollisionFlags.None;
        }
        else
        {
            Vector3 absoluteMovement = movementVelocity + blink;
            absoluteMovement = transform.localRotation * absoluteMovement + velocity * (1-blinkFactor);
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
    
    public void TakeHit()
    {
        hitFXTimer = hitFXDuration;
        currentHealth -= 10;
    }
}
