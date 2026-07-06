using UnityEngine;

namespace DigitalRepairSimulator.CableWiring
{
    /// <summary>
    /// Satu soket/port di panel kabel yang mengharapkan warna kabel tertentu.
    /// Urutan pemasangan yang benar diatur lewat urutan port ini di list CablePanel.
    /// </summary>
    public class CablePort : MonoBehaviour
    {
        [SerializeField] private CableColor expectedColor;

        public CableColor ExpectedColor => expectedColor;
        public bool IsFilled { get; set; }
        public CableItem PluggedCable { get; set; }
    }
}