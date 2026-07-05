using System;
using UnityEngine;

namespace DigitalRepairSimulator.Core
{
    /// <summary>
    /// Merepresentasikan grid 2D kerusakan pada permukaan sebuah objek repair.
    /// Sel merah = rusak, sel hijau = sudah diperbaiki.
    /// Ditampilkan langsung di permukaan objek via Texture2D yang diupdate real-time.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class DamageGrid : MonoBehaviour
    {
        [Header("Ukuran Grid")]
        [SerializeField] private int width = 12;
        [SerializeField] private int height = 6;

        [Header("Tampilan")]
        [SerializeField] private Color damagedColor = Color.red;
        [SerializeField] private Color repairedColor = Color.green;
        [SerializeField] private int pixelsPerCell = 16; // resolusi texture per sel

        private bool[,] _repaired;
        private Texture2D _texture;
        private Renderer _targetRenderer;

        public int Width => width;
        public int Height => height;
        public int TotalCells => width * height;
        public int RepairedCount { get; private set; }

        /// <summary>Dipanggil setiap satu sel berubah dari merah -> hijau.</summary>
        public event Action<int, int> OnCellRepaired;

        /// <summary>Dipanggil sekali saat seluruh grid selesai (semua sel hijau).</summary>
        public event Action OnGridCompleted;

        private bool _completedFired;

        private void Awake()
        {
            _targetRenderer = GetComponent<Renderer>();
            Initialize(width, height);
        }

        /// <summary>Reset ulang grid, berguna untuk pooling objek atau replay zona.</summary>
        public void Initialize(int gridWidth, int gridHeight)
        {
            width = gridWidth;
            height = gridHeight;
            _repaired = new bool[width, height];
            RepairedCount = 0;
            _completedFired = false;

            _texture = new Texture2D(width * pixelsPerCell, height * pixelsPerCell, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            RedrawAll();
            ApplyTextureToRenderer();
        }

        /// <summary>
        /// Konversi koordinat UV (dari RaycastHit.textureCoord) ke koordinat sel grid.
        /// </summary>
        public bool TryGetCellFromUV(Vector2 uv, out int x, out int y)
        {
            x = Mathf.FloorToInt(Mathf.Clamp01(uv.x) * width);
            y = Mathf.FloorToInt(Mathf.Clamp01(uv.y) * height);
            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);
            return true;
        }

        public bool IsCellRepaired(int x, int y) => _repaired[x, y];

        /// <summary>
        /// Coba perbaiki satu sel. Mengembalikan true jika ini adalah hit akurat
        /// (sel sebelumnya masih rusak), false jika waste (sel sudah hijau).
        /// </summary>
        public bool RepairCell(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return false;

            if (_repaired[x, y])
                return false; // waste: kena sel yang sudah diperbaiki

            _repaired[x, y] = true;
            RepairedCount++;
            PaintCell(x, y, repairedColor);
            _texture.Apply();

            OnCellRepaired?.Invoke(x, y);

            if (!_completedFired && RepairedCount >= TotalCells)
            {
                _completedFired = true;
                OnGridCompleted?.Invoke();
            }

            return true;
        }

        /// <summary>
        /// Perbaiki seluruh sel dalam radius (dalam satuan sel) dari titik pusat.
        /// Mengembalikan jumlah hit akurat dan waste, dipakai oleh MultiTool untuk radius upgrade.
        /// </summary>
        public void RepairArea(int centerX, int centerY, int radiusCells, out int accurateHits, out int wasteHits)
        {
            accurateHits = 0;
            wasteHits = 0;

            for (int x = centerX - radiusCells; x <= centerX + radiusCells; x++)
            {
                for (int y = centerY - radiusCells; y <= centerY + radiusCells; y++)
                {
                    if (x < 0 || x >= width || y < 0 || y >= height) continue;
                    float dist = Vector2.Distance(new Vector2(centerX, centerY), new Vector2(x, y));
                    if (dist > radiusCells) continue;

                    if (RepairCell(x, y)) accurateHits++;
                    else wasteHits++;
                }
            }
        }

        public bool IsFullyRepaired() => RepairedCount >= TotalCells;

        private void PaintCell(int x, int y, Color color)
        {
            int startX = x * pixelsPerCell;
            int startY = y * pixelsPerCell;
            for (int px = 0; px < pixelsPerCell; px++)
            {
                for (int py = 0; py < pixelsPerCell; py++)
                {
                    _texture.SetPixel(startX + px, startY + py, color);
                }
            }
        }

        private void RedrawAll()
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    PaintCell(x, y, _repaired[x, y] ? repairedColor : damagedColor);
            _texture.Apply();
        }

        private void ApplyTextureToRenderer()
        {
            // Asumsi shader standar menggunakan _BaseMap (URP) atau _MainTex (Built-in).
            if (_targetRenderer.material.HasProperty("_BaseMap"))
                _targetRenderer.material.SetTexture("_BaseMap", _texture);
            else if (_targetRenderer.material.HasProperty("_MainTex"))
                _targetRenderer.material.SetTexture("_MainTex", _texture);
        }
    }
}