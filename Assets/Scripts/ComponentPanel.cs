using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRepairSimulator.HardwareLab
{
    /// <summary>
    /// Mengelola satu motherboard: urutan slot yang harus diisi, validasi tipe & urutan.
    /// Beda dari CablePanel (Zona 2): salah pasang di sini TIDAK mereset semua,
    /// cukup komponennya ditolak/balik ke posisi asal (lebih sesuai nuansa "component fitting").
    /// </summary>
    public class ComponentPanel : MonoBehaviour
    {
        [Header("Urutan Slot (index 0 = harus diisi PERTAMA)")]
        [SerializeField] private List<ComponentSlot> slotsInOrder;

        private int _nextExpectedIndex;

        public bool IsBoardComplete { get; private set; }

        /// <summary>index slot yang berhasil diisi, total slot</summary>
        public event Action<int, int> OnComponentPlaced;
        public event Action OnBoardComplete;

        private void Start()
        {
            // Hanya slot pertama yang aktif collider-nya di awal.
            // Ini juga menyelesaikan masalah collider slot yang saling menumpuk
            // (misal Heatsink persis di atas CPU) - slot yang belum gilirannya
            // dimatikan collider-nya, jadi tidak bisa "kesenggol" raycast.
            for (int i = 0; i < slotsInOrder.Count; i++)
            {
                SetSlotColliderEnabled(slotsInOrder[i], i == 0);
            }
        }

        private void SetSlotColliderEnabled(ComponentSlot slot, bool isEnabled)
        {
            if (slot == null) return;
            var col = slot.GetComponent<Collider>();
            if (col != null) col.enabled = isEnabled;
        }

        /// <summary>
        /// Cek apakah slot ini valid buat diisi SEKARANG (index & tipe benar).
        /// Dipakai buat highlight real-time saat drag, sebelum dilepas.
        /// </summary>
        public bool IsValidPlacement(ComponentSlot slot, ComponentType partType)
        {
            int index = slotsInOrder.IndexOf(slot);
            if (index < 0) return false;
            if (slot.IsFilled) return false;
            if (index != _nextExpectedIndex) return false;
            return slot.ExpectedType == partType;
        }

        /// <summary>
        /// Coba pasang komponen ke slot. Return true kalau berhasil.
        /// </summary>
        public bool TryPlaceComponent(ComponentSlot slot, ComponentPart part)
        {
            if (IsBoardComplete) return false;
            if (!IsValidPlacement(slot, part.Type)) return false;

            slot.IsFilled = true;
            slot.PlacedPart = part;
            part.SnapToSlot(slot);

            _nextExpectedIndex++;
            OnComponentPlaced?.Invoke(_nextExpectedIndex, slotsInOrder.Count);

            if (_nextExpectedIndex >= slotsInOrder.Count)
            {
                IsBoardComplete = true;
                OnBoardComplete?.Invoke();
            }
            else
            {
                // Buka collider slot berikutnya, giliran sekarang miliknya
                SetSlotColliderEnabled(slotsInOrder[_nextExpectedIndex], true);
            }

            return true;
        }
    }
}