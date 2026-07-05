using UnityEngine;
using UnityEngine.UI;

namespace DigitalRepairSimulator.Core
{
    /// <summary>
    /// Mengubah warna crosshair saat kamera mengarah ke objek repairable,
    /// supaya pemain punya feedback jelas sebelum menembak laser.
    /// Attach ke GameObject Image crosshair di Canvas.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class CrosshairFeedback : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private LayerMask repairableLayer;
        [SerializeField] private float maxRange = 5f;

        [SerializeField] private Color neutralColor = Color.white;
        [SerializeField] private Color targetingColor = Color.green;

        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void Update()
        {
            if (playerCamera == null) return;

            bool onTarget = Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
                out _, maxRange, repairableLayer);

            _image.color = onTarget ? targetingColor : neutralColor;
        }
    }
}