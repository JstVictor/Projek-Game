using UnityEngine;

namespace DigitalRepairSimulator.Core
{
    /// <summary>
    /// Memutar badan player agar menghadap arah kamera saat sedang menembak laser,
    /// supaya arah visual tool/animasi aiming sinkron dengan arah crosshair yang sebenarnya.
    /// Attach ke root GameObject player (yang punya Transform utama, bukan child model).
    /// </summary>
    public class AimBodyRotation : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private MultiTool multiTool;
        [SerializeField] private float rotationSpeed = 15f;

        private void Update()
        {
            if (multiTool == null || cameraTransform == null) return;
            if (!multiTool.IsFiring) return;

            // Hanya putar di sumbu Y (yaw), biar badan tidak miring naik/turun ikut pitch kamera
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0f;

            if (camForward.sqrMagnitude < 0.0001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}