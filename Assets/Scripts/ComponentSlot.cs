using UnityEngine;

namespace DigitalRepairSimulator.HardwareLab
{
    /// <summary>
    /// Satu slot di motherboard yang mengharapkan tipe komponen tertentu (CPU, RAM, dst).
    /// Memberi highlight hijau/merah saat komponen di-drag di atasnya.
    /// </summary>
    public class ComponentSlot : MonoBehaviour
    {
        [SerializeField] private ComponentType expectedType;
        [SerializeField] private Renderer highlightRenderer;
        [SerializeField] private Color neutralColor = Color.white;
        [SerializeField] private Color validColor = Color.green;
        [SerializeField] private Color invalidColor = Color.red;

        [Header("Koreksi Posisi & Rotasi (kalau part melayang/tenggelam/miring)")]
        [Tooltip("Rotasi lokal yang diterapkan ke komponen saat terpasang di slot ini. Sesuaikan manual sampai lurus.")]
        [SerializeField] private Vector3 placementRotationOffset = Vector3.zero;

        [Tooltip("Offset posisi lokal yang diterapkan ke komponen saat terpasang di slot ini. Sesuaikan jika melayang/tenggelam.")]
        [SerializeField] private Vector3 placementPositionOffset = Vector3.zero;

        public Quaternion PlacementRotation => Quaternion.Euler(placementRotationOffset);
        public Vector3 PlacementPositionOffset => placementPositionOffset;

        public ComponentType ExpectedType => expectedType;
        public bool IsFilled { get; set; }
        public ComponentPart PlacedPart { get; set; }

        /// <summary>Dipanggil ComponentDragController tiap frame saat komponen di-drag di atas slot ini.</summary>
        public void ShowHighlight(bool isValid)
        {
            if (highlightRenderer == null) return;
            highlightRenderer.material.color = isValid ? validColor : invalidColor;
        }

        public void ClearHighlight()
        {
            if (highlightRenderer == null) return;
            highlightRenderer.material.color = neutralColor;
        }
    }
}