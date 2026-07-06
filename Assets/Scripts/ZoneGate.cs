using System.Collections;
using DigitalRepairSimulator.Zones;
using UnityEngine;

namespace DigitalRepairSimulator.Zones
{
    /// <summary>
    /// Penghalang fisik antar zona (misal dinding/pintu energi) yang otomatis
    /// terbuka begitu ZoneManager zona sebelumnya memicu OnZoneCompleted.
    /// Cocok untuk map "all-in-one" tanpa perpindahan scene/teleport.
    /// </summary>
    public class ZoneGate : MonoBehaviour
    {
        [Header("Syarat Membuka Gate")]
        [Tooltip("ZoneManager dari zona SEBELUMNYA yang harus selesai dulu")]
        [SerializeField] private ZoneManager requiredZone;

        [Header("Objek Penghalang")]
        [Tooltip("GameObject dinding/gate fisik yang menghalangi jalan (harus punya Collider)")]
        [SerializeField] private GameObject barrierVisual;

        [Header("Animasi Buka")]
        [SerializeField] private bool useSlideAnimation = true;
        [SerializeField] private Vector3 openOffset = new Vector3(0f, 4f, 0f);
        [SerializeField] private float openDuration = 1.5f;

        [Header("Opsional")]
        [SerializeField] private AudioSource openSfx;

        private bool _opened;

        private void Start()
        {
            if (requiredZone == null)
            {
                Debug.LogWarning($"[ZoneGate] {name}: requiredZone belum di-assign, gate tidak akan pernah terbuka otomatis.");
                return;
            }

            requiredZone.OnZoneCompleted += HandleZoneCompleted;

            // Jaga-jaga: kalau zona ternyata SUDAH selesai duluan sebelum gate ini aktif
            if (requiredZone.IsZoneCompleted)
                OpenGateInstantly();
        }

        private void OnDestroy()
        {
            if (requiredZone != null)
                requiredZone.OnZoneCompleted -= HandleZoneCompleted;
        }

        private void HandleZoneCompleted()
        {
            if (_opened) return;
            _opened = true;

            if (openSfx != null) openSfx.Play();

            if (barrierVisual == null) return;

            if (useSlideAnimation)
                StartCoroutine(SlideOpenRoutine());
            else
                barrierVisual.SetActive(false);
        }

        private void OpenGateInstantly()
        {
            _opened = true;
            if (barrierVisual != null)
                barrierVisual.SetActive(false);
        }

        private IEnumerator SlideOpenRoutine()
        {
            Vector3 startPos = barrierVisual.transform.position;
            Vector3 endPos = startPos + openOffset;

            // Matikan collider dari awal animasi supaya pemain tidak nyangkut
            // di tengah proses gate yang lagi bergerak.
            var col = barrierVisual.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            float elapsed = 0f;
            while (elapsed < openDuration)
            {
                elapsed += Time.deltaTime;
                barrierVisual.transform.position = Vector3.Lerp(startPos, endPos, elapsed / openDuration);
                yield return null;
            }

            barrierVisual.transform.position = endPos;
            barrierVisual.SetActive(false); // sembunyikan total setelah selesai geser
        }
    }
}