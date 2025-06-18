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
        private float _width = 64;

        public float Width => _width;

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
    }
}
