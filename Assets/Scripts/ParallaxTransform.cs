using UnityEngine;

namespace VLDefenderArcade
{
    /// <summary>
    /// Parallax effect relative to main camera.
    /// </summary>
    public class ParallaxTransform : MonoBehaviour
    {
        [SerializeField]
        private float _modifier = 0.2f;

        private void OnEnable()
        {
        }

        private void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null)
                return;

            var pos = transform.position;
            pos.x = cam.transform.position.x * _modifier;
            transform.position = pos;
        }
    }
}
