using UnityEngine;

namespace DigitalRepairSimulator.CableWiring
{
    /// <summary>
    /// Kabel fisik yang berserakan di lantai, bisa dipungut pemain (lewat PlayerInteract)
    /// dan dicolokkan ke CablePort yang sesuai warnanya.
    /// </summary>
    public class CableItem : MonoBehaviour
    {
        [SerializeField] private CableColor color;

        private Transform _originalParent;
        private Vector3 _originalLocalPos;
        private Quaternion _originalLocalRot;
        private Collider _col;
        private Rigidbody _rb;

        public CableColor Color => color;
        public bool IsHeld { get; private set; }

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _rb = GetComponent<Rigidbody>();
            _originalParent = transform.parent;
            _originalLocalPos = transform.localPosition;
            _originalLocalRot = transform.localRotation;
        }

        /// <summary>Dipanggil PlayerInteract saat kabel dipungut.</summary>
        public void PickUp(Transform holdPoint)
        {
            IsHeld = true;

            if (_col != null) _col.enabled = false;
            if (_rb != null) _rb.isKinematic = true;

            transform.SetParent(holdPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>Dipanggil saat kabel dicolok berhasil ke port.</summary>
        public void PlugIntoPort(Transform portSocket)
        {
            IsHeld = false;

            transform.SetParent(portSocket);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            // Collider dibiarkan nonaktif, kabel sudah "terpasang tetap" di port
        }

        /// <summary>Dipanggil saat panel reset (urutan salah) - kabel balik ke posisi asal di lantai.</summary>
        public void ReturnToFloor()
        {
            IsHeld = false;

            transform.SetParent(_originalParent);
            transform.localPosition = _originalLocalPos;
            transform.localRotation = _originalLocalRot;

            if (_col != null) _col.enabled = true;
            if (_rb != null) _rb.isKinematic = false;
        }
    }
}