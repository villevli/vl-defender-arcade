using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VLDefenderArcade
{
    /// <summary>
    /// Map information.
    /// </summary>
    public class Map : MonoBehaviour
    {
        [SerializeField]
        private Vector2 _size = new(64, 16);

        public float Width => _size.x;
        public float Height => _size.y;

        public Rect Rect => new(-_size / 2, _size);

        // Support different map per scene
        private static Dictionary<Scene, Map> _sceneDict = new();

        private void Awake()
        {
            _sceneDict[gameObject.scene] = this;
        }

        /// <summary>
        /// Find the <see cref="Map"/> where the gameobject is in.
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static Map Find(GameObject go)
        {
            if (_sceneDict.TryGetValue(go.scene, out var map))
                return map;
            return null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position, _size);
        }
    }
}
