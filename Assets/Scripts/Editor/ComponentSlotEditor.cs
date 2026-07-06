using UnityEngine;
using UnityEditor;

namespace DigitalRepairSimulator.HardwareLab
{
    [CustomEditor(typeof(ComponentSlot))]
    public class ComponentSlotEditor : Editor
    {
        private ComponentPart _testPart;
        private GameObject _previewInstance;

        public override void OnInspectorGUI()
        {
            // Draw default inspector fields
            DrawDefaultInspector();

            ComponentSlot slot = (ComponentSlot)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Visual Alignment Helper", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use this utility to align components in real-time within the Editor without entering Play Mode.", MessageType.Info);

            _testPart = (ComponentPart)EditorGUILayout.ObjectField("Test Component Part", _testPart, typeof(ComponentPart), true);

            if (_testPart != null)
            {
                if (_previewInstance == null)
                {
                    if (GUILayout.Button("Create Visual Preview", GUILayout.Height(30)))
                    {
                        CreatePreview(slot);
                    }
                }
                else
                {
                    if (GUILayout.Button("Destroy Visual Preview", GUILayout.Height(30)))
                    {
                        DestroyPreview();
                    }
                }
            }
            else
            {
                DestroyPreview();
            }

            if (_previewInstance != null)
            {
                // Real-time update in Editor
                UpdatePreviewPosition(slot);

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(slot);
                }
            }
        }

        private void OnDisable()
        {
            DestroyPreview();
        }

        private void CreatePreview(ComponentSlot slot)
        {
            DestroyPreview();

            if (_testPart == null) return;

            // Instantiate prefab or clone scene object
            if (PrefabUtility.IsPartOfPrefabAsset(_testPart.gameObject))
            {
                _previewInstance = (GameObject)PrefabUtility.InstantiatePrefab(_testPart.gameObject);
            }
            else
            {
                _previewInstance = Instantiate(_testPart.gameObject);
            }

            _previewInstance.name = "[ALIGNMENT PREVIEW] " + _testPart.name;
            _previewInstance.hideFlags = HideFlags.DontSave;

            // Disable components to prevent interference
            var col = _previewInstance.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            
            var rb = _previewInstance.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            UpdatePreviewPosition(slot);
            SceneView.RepaintAll();
        }

        private void UpdatePreviewPosition(ComponentSlot slot)
        {
            if (_previewInstance == null) return;

            var part = _previewInstance.GetComponent<ComponentPart>();
            if (part == null) return;

            // Reset parent temporarily to do world space scale and rotations correctly
            _previewInstance.transform.SetParent(null);
            _previewInstance.transform.localScale = _testPart.transform.lossyScale;
            _previewInstance.transform.rotation = slot.transform.rotation * slot.PlacementRotation;

            if (part.UseBoundsCenter)
            {
                Renderer[] renderers = _previewInstance.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds bounds = renderers[0].bounds;
                    foreach (var r in renderers)
                        bounds.Encapsulate(r.bounds);

                    Vector3 pivotToCenterOffset = bounds.center - _previewInstance.transform.position;

                    _previewInstance.transform.SetParent(slot.transform);
                    _previewInstance.transform.position = slot.transform.position - pivotToCenterOffset;
                    _previewInstance.transform.localPosition += slot.PlacementPositionOffset + part.CustomPositionOffset;
                }
                else
                {
                    _previewInstance.transform.SetParent(slot.transform);
                    _previewInstance.transform.localPosition = slot.PlacementPositionOffset + part.CustomPositionOffset;
                }
            }
            else
            {
                _previewInstance.transform.SetParent(slot.transform);
                _previewInstance.transform.localPosition = slot.PlacementPositionOffset + part.CustomPositionOffset;
            }
        }

        private void DestroyPreview()
        {
            if (_previewInstance != null)
            {
                DestroyImmediate(_previewInstance);
                _previewInstance = null;
            }
        }
    }
}
