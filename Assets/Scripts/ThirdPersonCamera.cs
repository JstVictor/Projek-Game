using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Posisi")]
    public float distance      = 5f;
    public float heightOffset  = 1.8f;
    public float shoulderOffset = 0.5f;

    [Header("Mouse")]
    public float sensitivity = 2f;
    public float minY = -15f;
    public float maxY =  60f;

    private float yaw;
    private float pitch = 10f;

    void Start()
    {
        yaw   = transform.eulerAngles.y;
        pitch = 10f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void LateUpdate()
    {
        // Kalau target kosong, stop — ini penyebab kamera diam
        if (target == null)
        {
            Debug.LogWarning("ThirdPersonCamera: Target belum diassign!");
            return;
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw   += mouseX * sensitivity;
        pitch -= mouseY * sensitivity;
        pitch  = Mathf.Clamp(pitch, minY, maxY);

        Debug.Log($"[ThirdPersonCamera] MouseX: {mouseX:F3}, MouseY: {mouseY:F3}, Yaw: {yaw:F1}, Pitch: {pitch:F1}, CamPos: {transform.position}");

        Vector3 focusPoint = target.position + Vector3.up * heightOffset;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 finalPos    = focusPoint + rotation * new Vector3(0f, 0f, -distance);

        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * 15f);
        transform.LookAt(focusPoint);
    }
}