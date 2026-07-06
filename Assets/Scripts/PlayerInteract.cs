using UnityEngine;

namespace DigitalRepairSimulator.CableWiring
{
    /// <summary>
    /// Menangani interaksi pemain dengan tombol E: memungut CableItem dari lantai,
    /// dan mencolokkannya ke CablePort saat mengarah ke port yang tepat.
    /// </summary>
    public class PlayerInteract : MonoBehaviour
    {
        [Header("Referensi")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform handHoldPoint;
        [SerializeField] private LayerMask interactableLayer;

        [Header("Pengaturan")]
        [SerializeField] private float interactRange = 3f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private CableItem _heldCable;

        public CableItem HeldCable => _heldCable;

        [Header("Debug")]
        [SerializeField] private bool debugLogs = true;

        private void Update()
        {
            if (Input.GetKeyDown(interactKey))
            {
                if (debugLogs) Debug.Log($"[PlayerInteract] Tombol {interactKey} ditekan.");
                TryInteract();
            }
        }

        private void TryInteract()
        {
            if (playerCamera == null)
            {
                if (debugLogs) Debug.LogError("[PlayerInteract] Player Camera belum di-assign.");
                return;
            }

            bool didHit = Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
                out RaycastHit hit, interactRange, interactableLayer);

            if (debugLogs) Debug.Log($"[PlayerInteract] Raycast Hit: {didHit}" + (didHit ? $" -> {hit.collider.name}" : " -> tidak kena apa-apa (cek jarak/arah/layer)"));

            // Debug tambahan: raycast TANPA filter layer, buat lihat apa aja yang sebenarnya ada di depan
            if (debugLogs && !didHit)
            {
                bool didHitAny = Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
                    out RaycastHit hitAny, interactRange);
                if (didHitAny)
                {
                    Debug.Log($"[PlayerInteract] (Tanpa filter layer) Sebenarnya kena: '{hitAny.collider.name}' di layer '{LayerMask.LayerToName(hitAny.collider.gameObject.layer)}' (index {hitAny.collider.gameObject.layer})");
                }
                else
                {
                    Debug.Log("[PlayerInteract] (Tanpa filter layer) Tetap tidak kena APAPUN - kemungkinan arah raycast salah total atau collider bermasalah.");
                }
            }

            if (!didHit) return;

            if (_heldCable == null)
            {
                if (debugLogs) Debug.Log("[PlayerInteract] Tidak sedang pegang kabel, coba pungut...");
                TryPickUpCable(hit);
            }
            else
            {
                if (debugLogs) Debug.Log($"[PlayerInteract] Sedang pegang kabel {_heldCable.Color}, coba colok...");
                TryPlugHeldCable(hit);
            }
        }

        private void TryPickUpCable(RaycastHit hit)
        {
            var cable = hit.collider.GetComponentInParent<CableItem>();

            if (debugLogs) Debug.Log($"[PlayerInteract] CableItem ditemukan di objek yang di-hit: {cable != null}");

            if (cable == null || cable.IsHeld) return;

            cable.PickUp(handHoldPoint);
            _heldCable = cable;

            if (debugLogs) Debug.Log($"[PlayerInteract] Berhasil pungut kabel {cable.Color}");
        }

        private void TryPlugHeldCable(RaycastHit hit)
        {
            var port = hit.collider.GetComponentInParent<CablePort>();

            if (debugLogs) Debug.Log($"[PlayerInteract] CablePort ditemukan di objek yang di-hit: {port != null}");

            if (port == null) return;

            var panel = port.GetComponentInParent<CablePanel>();

            if (debugLogs) Debug.Log($"[PlayerInteract] CablePanel ditemukan sebagai parent port: {panel != null}");

            if (panel == null) return;

            bool success = panel.TryPlugCable(port, _heldCable);
            if (debugLogs) Debug.Log($"[PlayerInteract] Hasil colok kabel: {(success ? "BERHASIL" : "GAGAL/salah urutan-warna")}");

            _heldCable = null;
        }
    }
}