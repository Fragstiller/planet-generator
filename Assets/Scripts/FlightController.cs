using UnityEngine;

public class FlightController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float ascentSpeed = 5f;
    public float boostMultiplier = 2f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 70f;
    public float rollSpeed = 70f;

    private Rigidbody rb;

	void Start()
	{
	    rb = GetComponent<Rigidbody>();
	}

	void Update()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
        }
        if (Input.GetKey(KeyCode.Mouse0))
        {
			Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Cursor.lockState == CursorLockMode.Locked)
        {
			HandleMovement();
			HandleRotation();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        }
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
        float moveForward = Input.GetAxis("Vertical");
        float moveSide = Input.GetAxis("Horizontal");

        float moveUp = 0f;
        if (Input.GetKey(KeyCode.Space))
        {
            moveUp = 1f;
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            moveUp = -1f;
        }

        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed *= boostMultiplier;
        }

        Vector3 movement = new Vector3(moveSide, moveUp * ascentSpeed / moveSpeed, moveForward);
        movement = transform.TransformDirection(movement) * currentSpeed * Time.deltaTime;

        rb.AddForce(movement, ForceMode.VelocityChange);
    }

    void HandleRotation()
    {
        float yaw = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float pitch = -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        float roll = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            roll = rollSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            roll = -rollSpeed * Time.deltaTime;
        }

        transform.Rotate(pitch, yaw, roll, Space.Self);
    }
}
