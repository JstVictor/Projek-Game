using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed  = 8f;
    public float gravity   = -9.81f;

    [Header("References")]
    public Transform cameraTransform;

    private CharacterController cc;
    private Vector3 velocity;
    private bool isRunning;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void Update()
    {
        CalculateMovement();
    }

    void CalculateMovement()
    {
        // --- 1. HITUNG ARAH & KECEPATAN HORIZONTAL ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight   = cameraTransform.right;
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        // Terapkan kecepatan ke sumbu X dan Z pada vector 'velocity'
        velocity.x = moveDir.x * currentSpeed;
        velocity.z = moveDir.z * currentSpeed;


        // --- 2. HITUNG GRAVITASI (Y) ---
        if (cc.isGrounded && velocity.y < 0f)
        {
            // Reset gravitasi saat menyentuh tanah agar tidak terus menumpuk
            velocity.y = -2f; 
        }
        velocity.y += gravity * Time.deltaTime;


        // --- 3. EKSEKUSI PERGERAKAN (CUKUP 1 KALI) ---
        cc.Move(velocity * Time.deltaTime);
    }

    // Public supaya bisa diakses PlayerAnimator
    public bool IsRunning() => isRunning;
    
    public float GetSpeed() 
    {
        // Sekarang cc.velocity menyimpan kalkulasi yang benar dari X dan Z
        Vector3 horizontalVelocity = new Vector3(cc.velocity.x, 0f, cc.velocity.z);
        return horizontalVelocity.magnitude;
    }
}