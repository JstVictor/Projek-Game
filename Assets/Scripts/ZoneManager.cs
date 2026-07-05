using System;
using System.Collections.Generic;
using DigitalRepairSimulator.Core;
using DigitalRepairSimulator.Managers;
using UnityEngine;

namespace DigitalRepairSimulator.Zones
{
    /// <summary>
    /// Melacak seluruh RepairableObject dalam satu zona. Memicu OnZoneCompleted
    /// saat semua objek sudah selesai diperbaiki. Dipakai di semua zona (1-5),
    /// untuk zona dengan timer/virus, tambahkan komponen terpisah untuk itu.
    /// </summary>
    public class ZoneManager : MonoBehaviour
    {
        [Header("Identitas Zona")]
        [SerializeField] private string zoneName = "Zona 1 - Server Room";

        [Header("Objek yang harus diperbaiki di zona ini")]
        [SerializeField] private List<RepairableObject> repairablesInZone;

        [Header("Opsional")]
        [Tooltip("Jika dicentang, lore otomatis didaftarkan ke LoreManager saat zona mulai")]
        [SerializeField] private bool autoRegisterLore = true;

        private readonly HashSet<RepairableObject> _completed = new();

        public int TotalObjects => repairablesInZone.Count;
        public int CompletedCount => _completed.Count;
        public bool IsZoneCompleted => _completed.Count >= repairablesInZone.Count;

        /// <summary>objectId, coinsAwarded, accuracyPercent — dipicu tiap satu objek selesai</summary>
        public event Action<string, int, float> OnObjectRepaired;

        /// <summary>Dipicu sekali saat seluruh objek di zona selesai diperbaiki</summary>
        public event Action OnZoneCompleted;

        private void Start()
        {
            foreach (var repairable in repairablesInZone)
            {
                if (repairable == null) continue;

                if (autoRegisterLore && LoreManager.Instance != null)
                    LoreManager.Instance.RegisterRepairable(repairable);

                repairable.OnRepairCompleted += HandleObjectRepaired;
            }

            Debug.Log($"[ZoneManager] {zoneName} dimulai. Total objek: {TotalObjects}");
        }

        private void OnDestroy()
        {
            foreach (var repairable in repairablesInZone)
            {
                if (repairable != null)
                    repairable.OnRepairCompleted -= HandleObjectRepaired;
            }
        }

        private void HandleObjectRepaired(int coins, float accuracy, AccuracyGrade grade, string objectId)
        {
            var repairable = repairablesInZone.Find(r => r.ObjectId == objectId);
            if (repairable != null) _completed.Add(repairable);

            Debug.Log($"[ZoneManager] {objectId} selesai. Akurasi: {accuracy:F1}%, Grade: {grade}, Koin: {coins}. " +
                $"Progres zona: {CompletedCount}/{TotalObjects}");

            OnObjectRepaired?.Invoke(objectId, coins, accuracy);

            if (IsZoneCompleted)
            {
                Debug.Log($"[ZoneManager] {zoneName} SELESAI!");
                OnZoneCompleted?.Invoke();
            }
        }
    }
}