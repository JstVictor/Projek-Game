using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRepairSimulator.CableWiring
{
    /// <summary>
    /// Mengelola satu panel cable wiring: urutan port yang harus diisi,
    /// validasi warna & urutan, reset otomatis kalau salah, dan membuka akses
    /// laser scan akhir setelah semua port terisi benar.
    /// </summary>
    public class CablePanel : MonoBehaviour
    {
        [Header("Urutan Port (index 0 = harus diisi PERTAMA)")]
        [SerializeField] private List<CablePort> portsInOrder;

        [Header("Reset")]
        [SerializeField] private float resetDelay = 1f;

        [Header("Aktivasi Akhir (Laser Scan)")]
        [Tooltip("GameObject yang punya DamageGrid/RepairableObject, dibuka aksesnya setelah semua kabel benar")]
        [SerializeField] private GameObject finalScanTarget;
        [SerializeField] private string repairableLayerName = "Repairable";

        private int _nextExpectedIndex;

        public bool IsSequenceComplete { get; private set; }

        /// <summary>Dipicu saat kabel berhasil dicolok dengan benar (index port, total port)</summary>
        public event Action<int, int> OnCablePlugged;

        /// <summary>Dipicu saat pemain salah colok (urutan/warna salah), panel akan reset</summary>
        public event Action OnWrongOrder;

        /// <summary>Dipicu sekali saat seluruh urutan kabel benar</summary>
        public event Action OnSequenceComplete;

        private void Start()
        {
            // Kunci akses laser scan sampai kabel selesai dipasang semua
            if (finalScanTarget != null)
                finalScanTarget.layer = LayerMask.NameToLayer("Default");
        }

        /// <summary>
        /// Coba colokkan kabel yang sedang dipegang pemain ke port tertentu.
        /// Dipanggil dari PlayerInteract. Return true kalau berhasil.
        /// </summary>
        public bool TryPlugCable(CablePort port, CableItem cable)
        {
            if (IsSequenceComplete) return false;

            int portIndex = portsInOrder.IndexOf(port);
            if (portIndex < 0) return false; // port bukan bagian dari panel ini
            if (port.IsFilled) return false; // port sudah keisi

            bool correctOrder = portIndex == _nextExpectedIndex;
            bool correctColor = port.ExpectedColor == cable.Color;

            if (correctOrder && correctColor)
            {
                port.IsFilled = true;
                port.PluggedCable = cable;
                cable.PlugIntoPort(port.transform);

                _nextExpectedIndex++;
                OnCablePlugged?.Invoke(_nextExpectedIndex, portsInOrder.Count);

                if (_nextExpectedIndex >= portsInOrder.Count)
                {
                    IsSequenceComplete = true;
                    UnlockFinalScan();
                    OnSequenceComplete?.Invoke();
                }

                return true;
            }
            else
            {
                // Salah urutan atau salah warna -> kabel yang dipegang balik ke lantai,
                // dan seluruh panel di-reset setelah jeda singkat
                cable.ReturnToFloor();
                OnWrongOrder?.Invoke();
                StartCoroutine(ResetAfterDelay());
                return false;
            }
        }

        private IEnumerator ResetAfterDelay()
        {
            yield return new WaitForSeconds(resetDelay);
            ResetPanel();
        }

        private void ResetPanel()
        {
            foreach (var port in portsInOrder)
            {
                if (port.IsFilled && port.PluggedCable != null)
                    port.PluggedCable.ReturnToFloor();

                port.IsFilled = false;
                port.PluggedCable = null;
            }

            _nextExpectedIndex = 0;
        }

        private void UnlockFinalScan()
        {
            if (finalScanTarget == null) return;
            finalScanTarget.layer = LayerMask.NameToLayer(repairableLayerName);
        }
    }
}