using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour

{
    [Header("Movement")]
    public float moveForce = 8f;
    public float rotationSpeed = 3f;
    public float artificialGravity = 15f; 
    public float maxVelocity = 6f;
    public float brakingForce = 4f;
    public string floorTag = "pisables"; 

    [Header("Camera")]
    public Transform cameraTransform;
    public float mouseSensitivity = 100f;
    public float maxCameraAngle = 85f;

    private Rigidbody rb;
    private float xRotation = 0f;
    private Vector3 floorNormal = Vector3.up;
    private bool isGrounded;
    private float distanceToFloor;

    void Start()
    {

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.drag = 0.7f; 
        rb.angularDrag = 0.7f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleCamera();
    }

    void FixedUpdate()
    {
        CheckGround();
        //HandleArtificialGravity();
        HandleMovement();
        LimitVelocity();
    }

    void CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 1.5f) && hit.collider.CompareTag(floorTag))
        {
            floorNormal = hit.normal;
            distanceToFloor = hit.distance;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    /*
    void HandleArtificialGravity() 
    {
        if (isGrounded)
        {
            
            float gravityFactor = Mathf.Clamp(1 / (distanceToFloor + 0.1f), 0.5f, 2f);
            rb.AddForce(-floorNormal * artificialGravity * gravityFactor, ForceMode.Acceleration);
        }
    }

    */

    void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDirection += cameraTransform.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection -= cameraTransform.forward;
        if (Input.GetKey(KeyCode.D)) moveDirection += cameraTransform.right;
        if (Input.GetKey(KeyCode.A)) moveDirection -= cameraTransform.right;

        
        // Movimiento solo si está cerca del suelo o con fuerza reducida
        float moveMultiplier = isGrounded ? 1f : 0.3f;
        
        if (moveDirection != Vector3.zero)
        {
            rb.AddForce(moveDirection.normalized * moveForce * moveMultiplier, ForceMode.Force);
        }
        else
        {
            // Frenado más fuerte cuando está en el aire
            float currentBraking = isGrounded ? brakingForce : brakingForce * 0.5f;
            rb.AddForce(-rb.velocity * currentBraking * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }
        

    void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxCameraAngle, maxCameraAngle);
        
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void LimitVelocity()
    {
        if (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag(floorTag))
        {
            // rotación cuando está en contacto
            ContactPoint contact = collision.contacts[0];
            floorNormal = contact.normal;
            
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, floorNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * 2 * Time.deltaTime);
        }
    }
}

    

