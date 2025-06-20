using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace VLDefenderArcade
{
    /// <summary>
    /// Pool gameobjects instantiated from a prefab.
    /// </summary>
    public class GameObjectPool : MonoBehaviour
    {
        private static GameObjectPool _instance;

        [SerializeField]
        private bool _enablePooling = true;

        private Dictionary<GameObject, List<PooledGameObject>> _poolPerPrefab = new();

        // Internal component used for tracking objects spawned from a pool
        private class PooledGameObject : MonoBehaviour
        {
            public GameObject FromPrefab;

            private void OnParticleSystemStopped()
            {
                GameObjectPool.Destroy(gameObject);
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
                DestroyImmediate(gameObject);
            else
                _instance = this;
        }

        private static bool TryGetPool(GameObject prefab, out List<PooledGameObject> pool)
        {
            pool = default;

            if (_instance == null)
                return false;
            if (!_instance._enablePooling)
                return false;
            if (prefab == null)
                return false;

            if (!_instance._poolPerPrefab.TryGetValue(prefab, out pool))
            {
                _instance._poolPerPrefab[prefab] = pool = new();
            }
            return true;
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, bool setActive = true)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            GameObject go = null;

            if (TryGetPool(prefab, out var pool))
            {
                // Loop in case pool has destroyed objects
                while (pool.Count > 0 && go == null)
                {
                    var pgo = pool[^1];
                    if (pgo != null)
                        go = pgo.gameObject;
                    pool.RemoveAt(pool.Count - 1);
                }
            }

            if (go == null)
            {
                go = UObject.Instantiate(prefab, position, rotation);
                var pgo = go.AddComponent<PooledGameObject>();
                pgo.FromPrefab = prefab;

                if (go.TryGetComponent<ParticleSystem>(out var ps))
                {
                    var main = ps.main;
                    if (main.stopAction == ParticleSystemStopAction.Destroy)
                        main.stopAction = ParticleSystemStopAction.Callback;
                }
            }
            else
            {
                go.transform.SetPositionAndRotation(position, rotation);
            }

            if (setActive)
                go.SetActive(true);

            return go;
        }

        /// <summary>
        /// If <paramref name="go"/> is from a <see cref="GameObjectPool"/>, deactivates and returns it to the pool. Otherwise it's destroyed.
        /// </summary>
        /// <param name="go"></param>
        public static void Destroy(GameObject go)
        {
            if (go.TryGetComponent<PooledGameObject>(out var pgo) && TryGetPool(pgo.FromPrefab, out var pool))
            {
                go.SetActive(false);
                pool.Add(pgo);

                // Needed to fix the effect when pooled
                if (go.TryGetComponent<TrailRenderer>(out var tr))
                    tr.Clear();
            }
            else
            {
                UObject.Destroy(go);
            }
        }
    }
}
