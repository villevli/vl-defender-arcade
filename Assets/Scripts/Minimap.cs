using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace VLDefenderArcade
{
    /// <summary>
    /// Draws a minimap into a texture
    /// </summary>
    public class Minimap : MonoBehaviour
    {
        [SerializeField]
        private int2 _size = new(128, 32);
        [SerializeField, Tooltip("8 bits/pixel format (one channel). Must match size")]
        private Texture2D _groundTexture;

        private Map _map;
        private Texture2D _texture;

        private void Start()
        {
            _map = Map.Find(gameObject);

            if (_groundTexture != null)
            {
                _size = new int2(_groundTexture.width, _groundTexture.height);
            }

            _texture = new Texture2D(_size.x, _size.y, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            if (TryGetComponent<RawImage>(out var ri))
            {
                ri.texture = _texture;
            }
        }

        private void OnDestroy()
        {
            Destroy(_texture);
        }

        private void Update()
        {
            DrawMinimapTexture();
        }

        private void DrawMinimapTexture()
        {
            if (_texture == null)
                return;

            var cam = Camera.main;
            if (cam == null)
                return;

            int2 size = new(_texture.width, _texture.height);
            var data = _texture.GetPixelData<Color32>(0);
            var span = data.AsSpan();
            var area = _map.Area;
            Vector2 mapPos = new(cam.transform.position.x - area.width / 2, area.yMin - 1);
            float2 mapScale = new(size.x / area.width, size.y / (area.height + 2));
            Color32 COLOR_CLEAR = new(0, 0, 0, 255);
            Color32 COLOR_GROUND = new(255, 128, 0, 255);
            Color32 COLOR_ENEMY = new(0, 255, 0, 255);
            Color32 COLOR_PLAYER = new(255, 255, 255, 255);

            // Profiler.BeginSample("Clear");
            // for (int i = 0; i < span.Length; i++)
            // {
            //     span[i] = COLOR_CLEAR;
            // }
            // Profiler.EndSample();

            // TODO: Bake ground texture into a 1d heightmap since we'll have one pixel per column

            // Clear with repeating ground texture
            Profiler.BeginSample("Clear ground");
            var groundData = _groundTexture.GetPixelData<byte>(0);
            var groundSpan = groundData.AsReadOnlySpan();
            int offsetX = (int)((cam.transform.position.x - area.xMin) * mapScale.x) + size.x;
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    int ox = (x + offsetX) % size.x;
                    span[size.x * y + x] = groundSpan[size.x * y + ox] == 0 ? COLOR_CLEAR : COLOR_GROUND;
                }
            }

            Profiler.EndSample();

            Profiler.BeginSample("Draw enemies");
            for (int i = 0; i < Enemy.Enemies.Count; i++)
            {
                float2 pos = (Vector2)Enemy.Enemies[i].transform.position - mapPos;
                pos *= mapScale;
                int2 c = (int2)math.floor(pos);
                // Enemy
                // xx
                // xx
                span[size.x * c.y + c.x] = COLOR_ENEMY;
                span[size.x * c.y + (c.x + 1)] = COLOR_ENEMY;
                span[size.x * (c.y + 1) + c.x] = COLOR_ENEMY;
                span[size.x * (c.y + 1) + (c.x + 1)] = COLOR_ENEMY;
            }
            Profiler.EndSample();

            Profiler.BeginSample("Draw players");
            for (int i = 0; i < PlayerShipController.Players.Count; i++)
            {
                float2 pos = (Vector2)PlayerShipController.Players[i].transform.position - mapPos;
                pos *= mapScale;
                int2 c = (int2)math.floor(pos);
                // Player
                //  x 
                // xxx
                //  x 
                span[size.x * c.y + c.x] = COLOR_PLAYER;
                span[size.x * c.y + (c.x + 1)] = COLOR_PLAYER;
                span[size.x * c.y + (c.x - 1)] = COLOR_PLAYER;
                span[size.x * (c.y + 1) + c.x] = COLOR_PLAYER;
                span[size.x * (c.y - 1) + c.x] = COLOR_PLAYER;
            }
            Profiler.EndSample();

            _texture.Apply(true);
        }
    }
}
