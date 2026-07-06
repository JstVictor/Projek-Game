using UnityEngine;

namespace DigitalRepairSimulator.HardwareLab
{
    /// <summary>
    /// Komponen hardware fisik (CPU, Heatsink, RAM, GPU) yang berserakan di meja kerja,
    /// bisa di-drag pemain pakai mouse dan dipasang ke ComponentSlot yang sesuai.
    /// </summary>
    public class ComponentPart : MonoBehaviour
    {
        [SerializeField] private ComponentType type;

        [Header("Pengaturan Penempatan (Snapping)")]
        [Tooltip("Jika true, objek dipasang berdasarkan titik tengah visualnya (Bounds Center). Jika false, dipasang langsung menggunakan Pivot Point model (localPosition = Vector3.zero). Nonaktifkan jika model sudah memiliki pivot yang benar di bagian bawah.")]
        [SerializeField] private bool useBoundsCenter = true;

        [Tooltip("Offset posisi tambahan khusus untuk tipe part ini.")]
        [SerializeField] private Vector3 customPositionOffset = Vector3.zero;

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private Collider _col;
        private Rigidbody _rb;

        public ComponentType Type => type;
        public bool UseBoundsCenter => useBoundsCenter;
        public Vector3 CustomPositionOffset => customPositionOffset;
        public bool IsPlaced { get; private set; }
        public bool IsBeingDragged { get; private set; }

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _rb = GetComponent<Rigidbody>();
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
        }

        public void BeginDrag()
        {
            IsBeingDragged = true;
            if (_rb != null) _rb.isKinematic = true;
        }

        public void UpdateDragPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void EndDragReturnToOriginal()
        {
            IsBeingDragged = false;
            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
        }

        public void SnapToSlot(ComponentSlot slot)
        {
            IsBeingDragged = false;
            IsPlaced = true;

            Transform slotTransform = slot.transform;
            Quaternion localRotation = slot.PlacementRotation;
            Vector3 slotPositionOffset = slot.PlacementPositionOffset;

            // Lepas dari parent lama
            transform.SetParent(null);

            // Apply rotasi dulu
            transform.rotation = slotTransform.rotation * localRotation;

            if (useBoundsCenter)
            {
                // Hitung bounds SEBELUM pindah posisi
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds bounds = renderers[0].bounds;
                    foreach (var r in renderers)
                        bounds.Encapsulate(r.bounds);

                    // Hitung selisih antara pivot object dan center bounds-nya
                    Vector3 pivotToCenterOffset = bounds.center - transform.position;

                    // Set parent
                    transform.SetParent(slotTransform);

                    // Posisikan supaya CENTER bounds pas di posisi slot
                    transform.position = slotTransform.position - pivotToCenterOffset;
                    
                    // Tambahkan offset tambahan
                    transform.localPosition += slotPositionOffset + customPositionOffset;
                }
                else
                {
                    transform.SetParent(slotTransform);
                    transform.localPosition = slotPositionOffset + customPositionOffset;
                    transform.localRotation = localRotation;
                }
            }
            else
            {
                transform.SetParent(slotTransform);
                transform.localPosition = slotPositionOffset + customPositionOffset;
                transform.localRotation = localRotation;
            }

            if (_col != null) _col.enabled = false;
        }
    }
}