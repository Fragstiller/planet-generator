using UnityEngine;

public class FlightController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;            // Speed of movement forward/backward and left/right
    public float ascentSpeed = 5f;           // Speed of moving up and down
    public float boostMultiplier = 2f;       // Multiplier for boosting speed

    [Header("Rotation Settings")]
    public float rotationSpeed = 70f;        // Speed of rotation (yaw and pitch)
    public float rollSpeed = 70f;            // Speed of roll rotation

    private Rigidbody rb;

	void Start()
	{
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
	    rb = GetComponent<Rigidbody>();
	}

	void Update()
    {
        HandleMovement();
        HandleRotation();
    }

	void OnCollisionEnter(Collision collision)
	{
        if (Vector3.Dot(rb.velocity.normalized, collision.contacts[0].normal) < -0.5f)
        {
			rb.velocity = Vector3.zero;	
			rb.angularVelocity = Vector3.zero;
        }
	}

	void HandleMovement()
    {
        // Get input for movement on the horizontal and vertical axes (WASD keys or Arrow keys)
        float moveForward = Input.GetAxis("Vertical");      // Forward/Backward
        float moveSide = Input.GetAxis("Horizontal");       // Left/Right

        // Upward and downward movement (Space for up, Left Ctrl for down)
        float moveUp = 0f;
        if (Input.GetKey(KeyCode.Space))
        {
            moveUp = 1f;
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            moveUp = -1f;
        }

        // Apply boost when Left Shift is held down
        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed *= boostMultiplier;
        }

        // Create movement vector relative to the object's local space
        Vector3 movement = new Vector3(moveSide, moveUp * ascentSpeed / moveSpeed, moveForward);
        movement = transform.TransformDirection(movement) * currentSpeed * Time.deltaTime;

        // Move the character controller or transform
        //rb.velocity = movement;
        rb.AddForce(movement, ForceMode.VelocityChange);
    }

    void HandleRotation()
    {
        // Get mouse input for yaw (left/right) and pitch (up/down)
        float yaw = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float pitch = -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        // Roll with Q and E keys
        float roll = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            roll = rollSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            roll = -rollSpeed * Time.deltaTime;
        }

        // Apply rotations
        transform.Rotate(pitch, yaw, roll, Space.Self);
    }
}
