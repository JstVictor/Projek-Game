using System;
using System.Collections.Generic;
using DigitalRepairSimulator.Core;
using DigitalRepairSimulator.Data;
using UnityEngine;

namespace DigitalRepairSimulator.Managers
{
    /// <summary>
    /// Menyimpan seluruh LoreEntrySO dan menampilkannya sebagai popup
    /// saat RepairableObject dengan objectId yang cocok selesai diperbaiki.
    /// Hubungkan event OnLorePopupRequested ke UI popup kamu.
    /// </summary>
    public class LoreManager : MonoBehaviour
    {
        public static LoreManager Instance { get; private set; }

        [SerializeField] private List<LoreEntrySO> allLoreEntries;

        private readonly Dictionary<string, LoreEntrySO> _loreById = new();

        /// <summary>Dipicu saat lore harus ditampilkan ke UI: (title, bodyText, hasSpecialAnimation)</summary>
        public event Action<string, string, bool> OnLorePopupRequested;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            foreach (var entry in allLoreEntries)
            {
                if (!string.IsNullOrEmpty(entry.objectId))
                    _loreById[entry.objectId] = entry;
            }

            Debug.Log($"[LoreManager] Loaded {_loreById.Count} lore entries: {string.Join(", ", _loreById.Keys)}");
        }

        /// <summary>
        /// Panggil ini dari handler OnRepairCompleted milik setiap RepairableObject.
        /// </summary>
        public void ShowLoreFor(string objectId)
        {
            if (string.IsNullOrEmpty(objectId))
            {
                Debug.LogWarning("[LoreManager] ShowLoreFor dipanggil dengan objectId kosong.");
                return;
            }

            if (!_loreById.TryGetValue(objectId, out LoreEntrySO entry))
            {
                Debug.LogWarning($"[LoreManager] Tidak ada Lore Entry dengan objectId '{objectId}'. " +
                    $"Cek typo, atau pastikan asset Lore Entry-nya sudah di-drag ke list 'All Lore Entries'.");
                return;
            }

            Debug.Log($"[LoreManager] Menampilkan lore untuk '{objectId}': {entry.title}");
            OnLorePopupRequested?.Invoke(entry.title, entry.bodyText, entry.hasSpecialAnimation);
        }

        /// <summary>
        /// Helper untuk auto-subscribe: panggil sekali di GameManager saat sebuah
        /// RepairableObject di-spawn, supaya lore otomatis muncul saat selesai.
        /// </summary>
        public void RegisterRepairable(RepairableObject repairable)
        {
            Debug.Log($"[LoreManager] RegisterRepairable dipanggil untuk objectId '{repairable.ObjectId}'.");
            repairable.OnRepairCompleted += (coins, accuracy, grade, objectId) => ShowLoreFor(objectId);
        }
    }
}