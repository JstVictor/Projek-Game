using UnityEngine;

namespace DigitalRepairSimulator.Data
{
    /// <summary>
    /// Satu entri konten edukatif yang muncul sebagai lore popup setelah
    /// pemain berhasil memperbaiki sebuah objek (lihat tabel bab 5).
    /// </summary>
    [CreateAssetMenu(fileName = "NewLoreEntry", menuName = "DigitalRepairSimulator/Lore Entry")]
    public class LoreEntrySO : ScriptableObject
    {
        [Tooltip("Harus sama persis dengan Object Id di komponen RepairableObject terkait")]
        public string objectId;

        public string title;

        [TextArea(3, 6)]
        public string bodyText;

        [Tooltip("Opsional: animasi/VFX tambahan, contoh: server menyala")]
        public bool hasSpecialAnimation;
    }
}