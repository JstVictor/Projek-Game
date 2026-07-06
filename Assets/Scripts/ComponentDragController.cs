using UnityEngine;

namespace DigitalRepairSimulator.HardwareLab
{
    /// <summary>
    /// Menangani drag & drop komponen hardware pakai mouse: klik-tahan untuk mengangkat
    /// komponen, geser untuk memindahkan (mengikuti bidang horizontal meja kerja),
    /// lepas untuk mencoba memasang ke slot yang sedang disorot.
    /// </summary>
    public class ComponentDragController : MonoBehaviour
    {
        [Header("Referensi")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private LayerMask partLayer;
        [SerializeField] private LayerMask slotLayer;

        [Header("Pengaturan Drag")]
        [SerializeField] private float pickupRange = 5f;
        [SerializeField] private float hoverHeight = 0.15f;

        private ComponentPart _draggedPart;
        private Plane _dragPlane;
        private ComponentSlot _currentHoveredSlot;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                TryStartDrag();
            else if (Input.GetMouseButton(0) && _draggedPart != null)
                UpdateDrag();
            else if (Input.GetMouseButtonUp(0) && _draggedPart != null)
                EndDrag();
        }

        private void TryStartDrag()
        {
            if (playerCamera == null) return;

            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, pickupRange, partLayer)) return;

            var part = hit.collider.GetComponentInParent<ComponentPart>();
            if (part == null || part.IsPlaced) return;

            _draggedPart = part;
            _draggedPart.BeginDrag();

            // Bidang horizontal setinggi titik awal komponen, dipakai sepanjang proses drag
            _dragPlane = new Plane(Vector3.up, hit.point);
        }

        private void UpdateDrag()
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

            if (_dragPlane.Raycast(ray, out float distance))
            {
                Vector3 worldPos = ray.GetPoint(distance) + Vector3.up * hoverHeight;
                _draggedPart.UpdateDragPosition(worldPos);
            }

            UpdateSlotHighlight(ray);
        }

        private void UpdateSlotHighlight(Ray ray)
        {
            ComponentSlot hoveredSlot = null;

            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange * 2f, slotLayer))
                hoveredSlot = hit.collider.GetComponentInParent<ComponentSlot>();

            if (hoveredSlot != _currentHoveredSlot)
            {
                if (_currentHoveredSlot != null) _currentHoveredSlot.ClearHighlight();
                _currentHoveredSlot = hoveredSlot;
            }

            if (_currentHoveredSlot != null)
            {
                var panel = _currentHoveredSlot.GetComponentInParent<ComponentPanel>();
                bool valid = panel != null && panel.IsValidPlacement(_currentHoveredSlot, _draggedPart.Type);
                _currentHoveredSlot.ShowHighlight(valid);
            }
        }

        private void EndDrag()
        {
            if (_currentHoveredSlot != null)
            {
                var panel = _currentHoveredSlot.GetComponentInParent<ComponentPanel>();
                bool success = panel != null && panel.TryPlaceComponent(_currentHoveredSlot, _draggedPart);

                if (!success)
                    _draggedPart.EndDragReturnToOriginal();

                _currentHoveredSlot.ClearHighlight();
                _currentHoveredSlot = null;
            }
            else
            {
                _draggedPart.EndDragReturnToOriginal();
            }

            _draggedPart = null;
        }
    }
}