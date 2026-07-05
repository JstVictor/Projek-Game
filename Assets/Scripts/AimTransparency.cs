using UnityEngine;

namespace DigitalRepairSimulator.Core
{
    /// <summary>
    /// Membuat model player transparan saat menembak laser (Fire1 ditahan),
    /// supaya badan player tidak menghalangi pandangan ke target di kamera third-person.
    /// Attach ke root GameObject model player (yang punya Renderer/SkinnedMeshRenderer).
    /// </summary>
    public class AimTransparency : MonoBehaviour
    {
        [SerializeField] private Renderer[] playerRenderers;
        [SerializeField] private float transparentAlpha = 0.25f;
        [SerializeField] private float fadeSpeed = 8f;

        private MaterialPropertyBlock _propBlock;
        private float _currentAlpha = 1f;
        private bool _isAiming;

        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            _propBlock = new MaterialPropertyBlock();

            if (playerRenderers == null || playerRenderers.Length == 0)
                playerRenderers = GetComponentsInChildren<Renderer>();
        }

        /// <summary>Panggil dari MultiTool atau input handler saat Fire1 ditahan/dilepas.</summary>
        public void SetAiming(bool aiming)
        {
            _isAiming = aiming;
        }

        private void Update()
        {
            float targetAlpha = _isAiming ? transparentAlpha : 1f;
            _currentAlpha = Mathf.MoveTowards(_currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
            ApplyAlpha(_currentAlpha);
        }

        private void ApplyAlpha(float alpha)
        {
            foreach (var rend in playerRenderers)
            {
                if (rend == null) continue;

                rend.GetPropertyBlock(_propBlock);
                Color c = Color.white;

                if (rend.sharedMaterial != null)
                {
                    if (rend.sharedMaterial.HasProperty(BaseColorProperty))
                        c = rend.sharedMaterial.GetColor(BaseColorProperty);
                    else if (rend.sharedMaterial.HasProperty(ColorProperty))
                        c = rend.sharedMaterial.GetColor(ColorProperty);
                }

                c.a = alpha;
                _propBlock.SetColor(BaseColorProperty, c);
                _propBlock.SetColor(ColorProperty, c);
                rend.SetPropertyBlock(_propBlock);
            }
        }
    }
}