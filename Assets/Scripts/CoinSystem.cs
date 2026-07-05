using System;
using UnityEngine;

namespace DigitalRepairSimulator.Core
{
    /// <summary>
    /// Menyimpan dan mengelola jumlah koin pemain secara global.
    /// Satu instance hidup sepanjang permainan (DontDestroyOnLoad).
    /// </summary>
    public class CoinSystem : MonoBehaviour
    {
        public static CoinSystem Instance { get; private set; }

        [SerializeField] private int startingCoins = 0;

        public int CurrentCoins { get; private set; }

        public event Action<int> OnCoinsChanged; // parameter: total koin terbaru

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentCoins = startingCoins;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            CurrentCoins += amount;
            OnCoinsChanged?.Invoke(CurrentCoins);
        }

        /// <summary>Mengembalikan true jika pembelian berhasil (koin cukup).</summary>
        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0) return true;
            if (CurrentCoins < amount) return false;

            CurrentCoins -= amount;
            OnCoinsChanged?.Invoke(CurrentCoins);
            return true;
        }
    }
}