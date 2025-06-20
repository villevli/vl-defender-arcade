using Unity.Collections;
using UnityEngine;

namespace VLDefenderArcade
{
    /// <summary>
    /// Wraps the object to the other side when it goes outside the play area around the main camera.
    /// </summary>
    public class WrapTransform : MonoBehaviour
    {
        private Map _map;

        private void Start()
        {
            _map = Map.Find(gameObject);
        }

        private void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null)
                return;

            // Shift
            var pos = transform.position;
            var camPos = cam.transform.position;
            var area = _map.Area;
            if (pos.x - camPos.x > area.xMax)
            {
                pos.x -= area.width;
                Shift(-area.width);
            }
            if (pos.x - camPos.x < area.xMin)
            {
                pos.x += area.width;
                Shift(area.width);
            }
            transform.position = pos;
        }

        private void Shift(float amount)
        {
            if (TryGetComponent<TrailRenderer>(out var tr) && tr.positionCount > 0)
            {
                using var buffer = new NativeArray<Vector3>(tr.positionCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                tr.GetPositions(buffer);
                var span = buffer.AsSpan();
                for (int i = 0; i < span.Length; i++)
                {
                    span[i].x += amount;
                }
                tr.SetPositions(buffer);
            }
        }
    }
}
