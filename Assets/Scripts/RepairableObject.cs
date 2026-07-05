using System;
using UnityEngine;

namespace DigitalRepairSimulator.Core
{
    public enum AccuracyGrade { OK, Good, Great, Perfect }

    /// <summary>
    /// Menempel pada setiap objek yang bisa diperbaiki (Terminal, Monitor, CPU Unit, dst).
    /// Melacak akurasi laser, menghitung grade & reward koin saat DamageGrid selesai,
    /// dan memicu event untuk sistem lore.
    /// </summary>
    [RequireComponent(typeof(DamageGrid))]
    public class RepairableObject : MonoBehaviour
    {
        [Header("Identitas")]
        [Tooltip("ID unik dipakai untuk lookup lore, contoh: 'terminal_zona1'")]
        [SerializeField] private string objectId;

        [Header("Reward")]
        [SerializeField] private int baseReward = 50;

        private DamageGrid _grid;
        private int _accurateHits;
        private int _wasteHits;
        private bool _completed;

        public string ObjectId => objectId;
        public bool IsCompleted => _completed;

        /// <summary>coinsAwarded, accuracyPercent, grade, objectId</summary>
        public event Action<int, float, AccuracyGrade, string> OnRepairCompleted;

        private void Awake()
        {
            _grid = GetComponent<DamageGrid>();
        }

        private void OnEnable()
        {
            _grid.OnGridCompleted += HandleGridCompleted;
        }

        private void OnDisable()
        {
            _grid.OnGridCompleted -= HandleGridCompleted;
        }

        /// <summary>
        /// Dipanggil oleh MultiTool setiap kali laser mengenai objek ini pada satu sel.
        /// </summary>
        public void RegisterHit(int cellX, int cellY)
        {
            if (_completed) return;

            bool accurate = _grid.RepairCell(cellX, cellY);
            if (accurate) _accurateHits++;
            else _wasteHits++;
        }

        /// <summary>
        /// Dipanggil oleh MultiTool saat memperbaiki area (radius upgrade).
        /// </summary>
        public void RegisterAreaHit(int centerX, int centerY, int radiusCells)
        {
            if (_completed) return;

            _grid.RepairArea(centerX, centerY, radiusCells, out int accurate, out int waste);
            _accurateHits += accurate;
            _wasteHits += waste;
        }

        private void HandleGridCompleted()
        {
            _completed = true;

            int totalShots = _accurateHits + _wasteHits;
            float accuracyPercent = totalShots > 0 ? (_accurateHits / (float)totalShots) * 100f : 100f;

            AccuracyGrade grade = GradeFromAccuracy(accuracyPercent);
            float multiplier = MultiplierFromGrade(grade);
            int coinsAwarded = Mathf.RoundToInt(baseReward * multiplier);

            CoinSystem.Instance?.AddCoins(coinsAwarded);

            Debug.Log($"[RepairableObject] '{objectId}' SELESAI! Akurasi: {accuracyPercent:F1}% ({grade}), " +
                $"Koin didapat: {coinsAwarded}, Total koin sekarang: {(CoinSystem.Instance != null ? CoinSystem.Instance.CurrentCoins.ToString() : "CoinSystem belum ada!")}");

            OnRepairCompleted?.Invoke(coinsAwarded, accuracyPercent, grade, objectId);
        }

        public static AccuracyGrade GradeFromAccuracy(float percent)
        {
            if (percent >= 95f) return AccuracyGrade.Perfect;
            if (percent >= 80f) return AccuracyGrade.Great;
            if (percent >= 60f) return AccuracyGrade.Good;
            return AccuracyGrade.OK;
        }

        public static float MultiplierFromGrade(AccuracyGrade grade)
        {
            switch (grade)
            {
                case AccuracyGrade.Perfect: return 3f;
                case AccuracyGrade.Great: return 2f;
                case AccuracyGrade.Good: return 1.5f;
                default: return 1f;
            }
        }
    }
}