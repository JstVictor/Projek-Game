using System;
using DigitalRepairSimulator.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DigitalRepairSimulator.UI
{
    /// <summary>
    /// Menampilkan popup lore edukatif saat LoreManager memicu OnLorePopupRequested.
    /// Popup muncul dengan tombol "Lanjut" untuk ditutup manual oleh pemain
    /// (bukan auto-hide, supaya pemain punya waktu baca).
    /// </summary>
    public class LorePopupUI : MonoBehaviour
    {
        [Header("Referensi UI")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button continueButton;

        [Header("Opsional")]
        [Tooltip("Jika dicentang, waktu game (Time.timeScale) di-pause selama popup tampil")]
        [SerializeField] private bool pauseGameWhileShowing = false;

        /// <summary>True selagi popup lore sedang tampil di layar.</summary>
        public bool IsPopupOpen => popupPanel != null && popupPanel.activeSelf;

        /// <summary>Dipicu setelah pemain menutup popup lewat tombol Lanjut.</summary>
        public event Action OnPopupClosed;

        private void Start()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);

            if (continueButton != null)
                continueButton.onClick.AddListener(HidePopup);

            if (LoreManager.Instance != null)
                LoreManager.Instance.OnLorePopupRequested += ShowPopup;
            else
                Debug.LogWarning("[LorePopupUI] LoreManager.Instance masih null. Pastikan LoreManager ada di scene dan aktif.");
        }

        private void OnDestroy()
        {
            if (LoreManager.Instance != null)
                LoreManager.Instance.OnLorePopupRequested -= ShowPopup;
        }

        private void ShowPopup(string title, string body, bool hasSpecialAnimation)
        {
            if (popupPanel == null) return;

            if (titleText != null) titleText.text = title;
            if (bodyText != null) bodyText.text = body;

            popupPanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (pauseGameWhileShowing)
                Time.timeScale = 0f;
        }

        private void HidePopup()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (pauseGameWhileShowing)
                Time.timeScale = 1f;

            OnPopupClosed?.Invoke();
        }
    }
}