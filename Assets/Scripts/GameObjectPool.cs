using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace VLDefenderArcade
{
    /// <summary>
    /// Pool gameobjects instantiated from a prefab.
    /// </summary>
    public class GameObjectPool
    {
        private GameObject _prefab;
        private List<PooledGameObject> _pool = new();

        // Internal component used for tracking objects spawned from a pool
        private class PooledGameObject : MonoBehaviour
        {
            public GameObjectPool FromPool;
            public GameObject FromPrefab;
        }

        private void SetPrefab(GameObject prefab)
        {
            if (_prefab == prefab)
                return;
            _prefab = prefab;

            // Make sure the pool only has objects instantiated from this prefab
            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i] == null)
                {
                    _pool.RemoveAtSwapBack(i--);
                }
                else if (_pool[i].FromPrefab != prefab)
                {
                    UObject.Destroy(_pool[i].gameObject);
                    _pool.RemoveAtSwapBack(i--);
                }
            }
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, bool setActive = true)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            SetPrefab(prefab);

            GameObject go = null;

            // Loop in case pool has destroyed objects
            while (_pool.Count > 0 && go == null)
            {
                var pgo = _pool[^1];
                if (pgo != null)
                    go = pgo.gameObject;
                _pool.RemoveAt(_pool.Count - 1);
            }

            if (go == null)
            {
                go = UObject.Instantiate(prefab, position, rotation);
                var pgo = go.AddComponent<PooledGameObject>();
                pgo.FromPool = this;
                pgo.FromPrefab = prefab;
            }
            else
            {
                go.transform.SetPositionAndRotation(position, rotation);
            }

            if (setActive)
                go.SetActive(true);

            return go;
        }

        private void Return(PooledGameObject gameObject)
        {
            _pool.Add(gameObject);
        }

        /// <summary>
        /// If <paramref name="go"/> is from a <see cref="GameObjectPool"/>, deactivates and returns it to the pool. Otherwise it's destroyed.
        /// </summary>
        /// <param name="go"></param>
        public static void Destroy(GameObject go)
        {
            if (go.TryGetComponent<PooledGameObject>(out var pgo) && pgo.FromPool != null)
            {
                pgo.FromPool.Return(pgo);
                go.SetActive(false);
            }
            else
            {
                UObject.Destroy(go);
            }
        }
    }
}
