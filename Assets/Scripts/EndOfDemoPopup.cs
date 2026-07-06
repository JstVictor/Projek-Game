using DigitalRepairSimulator.HardwareLab;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DigitalRepairSimulator.UI
{
    /// <summary>
    /// Menampilkan popup "demo selesai" begitu motherboard Zona 3 berhasil dirakit lengkap.
    /// Menandakan progres game sejauh ini berakhir di Zona 3 (Zona 4-5 masih dikembangkan).
    /// </summary>
    public class EndOfDemoPopup : MonoBehaviour
    {
        [Header("Referensi")]
        [SerializeField] private ComponentPanel zone3Panel;

        [Header("UI")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button backToMenuButton;

        [Header("Pesan")]
        [TextArea]
        [SerializeField] private string message =
            "Terima kasih sudah bermain!\n\nDemo ini baru mencakup Zona 1-3. " +
            "Zona 4 (Infected Core) dan Zona 5 (Mainframe Core) masih dalam tahap pengembangan.";

        [Header("Opsional")]
        [Tooltip("Kosongkan kalau tidak mau tombol ini pindah scene")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Start()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);

            if (zone3Panel != null)
                zone3Panel.OnBoardComplete += ShowPopup;
            else
                Debug.LogWarning("[EndOfDemoPopup] Zone3Panel belum di-assign.");

            if (backToMenuButton != null)
                backToMenuButton.onClick.AddListener(BackToMainMenu);
        }

        private void OnDestroy()
        {
            if (zone3Panel != null)
                zone3Panel.OnBoardComplete -= ShowPopup;
        }

        private void ShowPopup()
        {
            if (popupPanel == null) return;

            if (messageText != null)
                messageText.text = message;

            popupPanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void BackToMainMenu()
        {
            if (string.IsNullOrEmpty(mainMenuSceneName)) return;
            Time.timeScale = 1f; // jaga-jaga kalau ada sistem lain yang pause timeScale
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}