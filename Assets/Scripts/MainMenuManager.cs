using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigitalRepairSimulator.UI
{
    /// <summary>
    /// Mengatur logic tombol-tombol di Main Menu: Play (masuk ke gameplay),
    /// Settings (buka panel pengaturan), About (buka panel info game).
    /// Attach ke GameObject kosong di scene Main Menu.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Nama Scene Gameplay")]
        [Tooltip("Harus PERSIS sama dengan nama scene gameplay di Build Settings")]
        [SerializeField] private string gameplaySceneName = "Gameplay";

        [Header("Panel-Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject aboutPanel;

        private void Start()
        {
            // Pastikan semua panel tertutup di awal
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (aboutPanel != null) aboutPanel.SetActive(false);

            // Main menu selalu pakai cursor normal (tidak locked kayak gameplay)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>Dipanggil dari tombol "Play"</summary>
        public void OnPlayClicked()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }

        /// <summary>Dipanggil dari tombol "Setting"</summary>
        public void OnSettingsClicked()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        /// <summary>Dipanggil dari tombol "About"</summary>
        public void OnAboutClicked()
        {
            if (aboutPanel != null) aboutPanel.SetActive(true);
        }

        /// <summary>Dipanggil dari tombol "Back"/"Close" di dalam panel Settings atau About</summary>
        public void CloseAllPanels()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (aboutPanel != null) aboutPanel.SetActive(false);
        }

        /// <summary>Opsional: dipanggil dari tombol "Quit" kalau mau ditambah</summary>
        public void OnQuitClicked()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}