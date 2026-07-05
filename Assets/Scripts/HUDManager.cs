using System.Collections;
using DigitalRepairSimulator.Core;
using DigitalRepairSimulator.Zones;
using TMPro;
using UnityEngine;

namespace DigitalRepairSimulator.UI
{
    /// <summary>
    /// HUD utama: menampilkan jumlah koin realtime, progres zona (X/Y objek),
    /// dan popup singkat saat mendapat reward dari satu objek selesai.
    /// Attach ke GameObject di dalam Canvas.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("Referensi")]
        [SerializeField] private ZoneManager zoneManager;

        [Header("Teks Koin")]
        [SerializeField] private TMP_Text coinText;

        [Header("Teks Progres Zona")]
        [SerializeField] private TMP_Text zoneProgressText;

        [Header("Popup Reward")]
        [SerializeField] private CanvasGroup rewardPopupGroup;
        [SerializeField] private TMP_Text rewardPopupText;
        [SerializeField] private float popupShowDuration = 1.5f;
        [SerializeField] private float popupFadeDuration = 0.3f;

        [Header("Panel Zona Selesai")]
        [SerializeField] private GameObject zoneCompletePanel;
        [SerializeField] private UnityEngine.UI.Button zoneCompleteContinueButton;

        [Header("Koordinasi dengan Lore Popup")]
        [Tooltip("Drag LorePopupUI di scene supaya panel Zona Selesai menunggu lore ditutup dulu")]
        [SerializeField] private LorePopupUI lorePopupUI;

        private bool _pendingZoneComplete;

        private Coroutine _popupRoutine;

        private void OnEnable()
        {
            if (zoneCompletePanel != null)
                zoneCompletePanel.SetActive(false);

            if (rewardPopupGroup != null)
                rewardPopupGroup.alpha = 0f;

            if (zoneCompleteContinueButton != null)
                zoneCompleteContinueButton.onClick.AddListener(HideZoneCompletePanel);
        }

        private void Start()
        {
            if (CoinSystem.Instance != null)
            {
                CoinSystem.Instance.OnCoinsChanged += UpdateCoinText;
                UpdateCoinText(CoinSystem.Instance.CurrentCoins);
            }
            else if (coinText != null)
            {
                Debug.LogWarning("[HUDManager] CoinSystem.Instance masih null. Pastikan GameObject dengan CoinSystem ada di scene dan aktif.");
            }

            if (zoneManager != null)
            {
                zoneManager.OnObjectRepaired += HandleObjectRepaired;
                zoneManager.OnZoneCompleted += HandleZoneCompleted;
                UpdateProgressText();
            }
            else if (zoneProgressText != null)
            {
                Debug.LogWarning("[HUDManager] Zone Manager belum di-assign di Inspector.");
            }

            if (lorePopupUI != null)
                lorePopupUI.OnPopupClosed += HandleLorePopupClosed;
        }

        private void OnDisable()
        {
            if (CoinSystem.Instance != null)
                CoinSystem.Instance.OnCoinsChanged -= UpdateCoinText;

            if (zoneManager != null)
            {
                zoneManager.OnObjectRepaired -= HandleObjectRepaired;
                zoneManager.OnZoneCompleted -= HandleZoneCompleted;
            }

            if (lorePopupUI != null)
                lorePopupUI.OnPopupClosed -= HandleLorePopupClosed;
        }

        private void UpdateCoinText(int totalCoins)
        {
            if (coinText != null)
                coinText.text = $"Koin: {totalCoins}";
        }

        private void UpdateProgressText()
        {
            if (zoneProgressText == null || zoneManager == null) return;
            zoneProgressText.text = $"Objek Diperbaiki: {zoneManager.CompletedCount}/{zoneManager.TotalObjects}";
        }

        private void HandleObjectRepaired(string objectId, int coinsAwarded, float accuracy)
        {
            UpdateProgressText();
            ShowRewardPopup($"+{coinsAwarded} Koin ({accuracy:F0}% akurasi)");
        }

        private void HandleZoneCompleted()
        {
            if (lorePopupUI != null && lorePopupUI.IsPopupOpen)
            {
                _pendingZoneComplete = true;
                return;
            }

            ShowZoneCompletePanel();
        }

        private void HandleLorePopupClosed()
        {
            if (_pendingZoneComplete)
            {
                _pendingZoneComplete = false;
                ShowZoneCompletePanel();
            }
        }

        private void ShowZoneCompletePanel()
        {
            if (zoneCompletePanel != null)
                zoneCompletePanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HideZoneCompletePanel()
        {
            if (zoneCompletePanel != null)
                zoneCompletePanel.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void ShowRewardPopup(string message)
        {
            if (rewardPopupGroup == null || rewardPopupText == null) return;

            rewardPopupText.text = message;

            if (_popupRoutine != null) StopCoroutine(_popupRoutine);
            _popupRoutine = StartCoroutine(PopupRoutine());
        }

        private IEnumerator PopupRoutine()
        {
            yield return Fade(rewardPopupGroup, 0f, 1f, popupFadeDuration);
            yield return new WaitForSeconds(popupShowDuration);
            yield return Fade(rewardPopupGroup, 1f, 0f, popupFadeDuration);
        }

        private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            group.alpha = to;
        }
    }
}