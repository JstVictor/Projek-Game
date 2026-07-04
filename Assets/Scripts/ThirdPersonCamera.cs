using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("References")]
    public Transform target; // Masukkan objek 'CameraTarget' ke sini di Inspector

    [Header("Camera Settings")]
    public float distance = 4.0f;      // Jarak kamera dari karakter
    public float mouseSensitivity = 3f; // Kecepatan rotasi kamera

    [Header("Limits")]
    public float pitchMin = -10f;     // Batas sudut kamera saat melihat ke atas
    public float pitchMax = 60f;      // Batas sudut kamera saat melihat ke bawah

    private float yaw;   // Rotasi sumbu X (Kiri-Kanan)
    private float pitch; // Rotasi sumbu Y (Atas-Bawah)

    void Start()
    {
        // Mengambil rotasi awal kamera saat game dimulai
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    // Menggunakan LateUpdate adalah standar industri untuk kamera
    // agar kamera bergerak SETELAH karakter selesai bergerak di fungsi Update
    void LateUpdate()
    {
        if (!target) return;

        // 1. Ambil input dari pergerakan mouse
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 2. Batasi rotasi vertikal (pitch) agar kamera tidak berputar terbalik ke bawah lantai
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // 3. Hitung rotasi dan posisi baru kamera
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0);
        
        // Kamera memposisikan diri di belakang target berdasarkan jarak (distance)
        Vector3 targetPosition = target.position - (targetRotation * Vector3.forward * distance);

        // 4. Terapkan posisi dan rotasi secara mulus ke Main Camera
        transform.rotation = targetRotation;
        transform.position = targetPosition;
    }
}