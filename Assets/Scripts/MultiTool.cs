using UnityEngine;

namespace DigitalRepairSimulator.Core
{
    /// <summary>
    /// Multi Tool: gadget laser repair utama yang dipegang pemain.
    /// Menangani raycast ke objek repair, cooldown, radius, dan efek overheat
    /// (dipicu virus entity di Zona 4/5).
    /// </summary>
    public class MultiTool : MonoBehaviour
    {
        [Header("Referensi")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private LineRenderer laserVisual;
        [SerializeField] private LayerMask repairableLayer;
        [SerializeField] private AimTransparency aimTransparency; // opsional, boleh dikosongkan
        [SerializeField] private Animator playerAnimator; // opsional, buat trigger animasi scan/repair
        [SerializeField] private string isRepairingParam = "IsRepairing";

        [Header("Stat Dasar (sebelum upgrade)")]
        [SerializeField] private float baseRange = 5f;
        [SerializeField] private int baseRadiusCells = 0; // 0 = hanya 1 sel yang kena
        [SerializeField] private float baseCooldown = 0.15f;
        [SerializeField] private float repairTickRate = 0.1f; // seberapa sering "tick" repair saat laser ditahan

        // Level upgrade saat ini, diisi oleh UpgradeManager
        private int _radiusLevel;
        private int _speedLevel;
        private bool _areaScanUnlocked;
        private int _cooldownLevel;

        private float _cooldownTimer;
        private float _tickTimer;
        private float _overheatTimer;

        public bool IsOverheated => _overheatTimer > 0f;
        public bool IsFiring { get; private set; }

        private void Update()
        {
            if (_overheatTimer > 0f)
            {
                _overheatTimer -= Time.deltaTime;
                SetLaserVisible(false);
                if (playerAnimator != null) playerAnimator.SetBool(isRepairingParam, false);
                return;
            }

            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;

            bool fireHeld = Input.GetButton("Fire1"); // sesuaikan dengan Input System jika dipakai

            if (debugLogs && Input.GetButtonDown("Fire1"))
                Debug.Log("[MultiTool] Fire1 terdeteksi (klik kiri terbaca).");

            IsFiring = fireHeld && _cooldownTimer <= 0f;
            aimTransparency?.SetAiming(fireHeld);
            if (playerAnimator != null) playerAnimator.SetBool(isRepairingParam, fireHeld);

            if (IsFiring)
            {
                _tickTimer -= Time.deltaTime;
                if (_tickTimer <= 0f)
                {
                    _tickTimer = CurrentTickRate;
                    TryRepairTick();
                }
            }

            SetLaserVisible(fireHeld);
        }

        [Header("Debug")]
        [SerializeField] private bool debugLogs = true;

        private void TryRepairTick()
        {
            if (playerCamera == null)
            {
                if (debugLogs) Debug.LogError("[MultiTool] Player Camera belum di-assign di Inspector.");
                return;
            }

            bool didHit = Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
                    out RaycastHit hit, CurrentRange, repairableLayer);

            if (debugLogs) Debug.Log($"[MultiTool] Raycast fired. Hit: {didHit}" + (didHit ? $" -> {hit.collider.name}" : " -> tidak kena apa-apa (cek jarak/arah/layer)"));

            if (!didHit) return;

            var repairable = hit.collider.GetComponentInParent<RepairableObject>();
            var grid = hit.collider.GetComponentInParent<DamageGrid>();

            if (debugLogs) Debug.Log($"[MultiTool] RepairableObject found: {repairable != null}, DamageGrid found: {grid != null}");

            if (repairable == null || grid == null) return;

            bool validCell = grid.TryGetCellFromUV(hit.textureCoord, out int cellX, out int cellY);

            if (debugLogs) Debug.Log($"[MultiTool] textureCoord: {hit.textureCoord}, cell: ({cellX},{cellY}), valid: {validCell}. " +
                "Jika textureCoord selalu (0,0) walau arah tembak berubah-ubah, kemungkinan besar Collider objek masih Box/Sphere Collider — ganti ke Mesh Collider.");

            if (!validCell) return;

            if (CurrentRadiusCells > 0)
                repairable.RegisterAreaHit(cellX, cellY, CurrentRadiusCells);
            else
                repairable.RegisterHit(cellX, cellY);

            _cooldownTimer = CurrentCooldown;
        }

        private void SetLaserVisible(bool visible)
        {
            if (laserVisual == null) return;
            laserVisual.enabled = visible && _overheatTimer <= 0f;
            if (!laserVisual.enabled) return;

            laserVisual.SetPosition(0, laserVisual.transform.position);
            Vector3 endPoint = playerCamera.transform.position + playerCamera.transform.forward * CurrentRange;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
                    out RaycastHit hit, CurrentRange, repairableLayer))
            {
                endPoint = hit.point;
            }
            laserVisual.SetPosition(1, endPoint);
        }

        /// <summary>Dipanggil oleh sistem virus (Zona 4/5) saat pemain tersentuh virus.</summary>
        public void TriggerOverheat(float duration = 3f)
        {
            _overheatTimer = duration;
        }

        // --- Kalkulasi stat berdasarkan level upgrade ---
        private float CurrentRange => baseRange; // Repair Radius+ mempengaruhi radius sel, bukan jarak tembak
        private int CurrentRadiusCells => baseRadiusCells + _radiusLevel; // tiap level += 1 sel radius
        private float CurrentCooldown => Mathf.Max(0.02f, baseCooldown - (_cooldownLevel * 0.04f));
        private float CurrentTickRate => Mathf.Max(0.02f, repairTickRate - (_speedLevel * 0.03f));

        public void SetRadiusLevel(int level) => _radiusLevel = level;
        public void SetSpeedLevel(int level) => _speedLevel = level;
        public void SetCooldownLevel(int level) => _cooldownLevel = level;
        public void SetAreaScanUnlocked(bool unlocked) => _areaScanUnlocked = unlocked;
        public bool IsAreaScanUnlocked => _areaScanUnlocked;
    }
}