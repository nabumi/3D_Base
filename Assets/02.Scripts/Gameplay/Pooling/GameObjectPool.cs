using System.Collections.Generic;
using UnityEngine;

namespace SystemicOverload.Pooling
{
    /// <summary>
    /// 한국어 주석: 단일 프리팹 기준의 간단한 GameObject Pool입니다. Validation 및 Phase 3 전투 데이터 검증에 사용합니다.
    /// </summary>
    public sealed class GameObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int prewarmCount = 4;
        [SerializeField] private Transform poolRoot;
        [SerializeField] private bool resetAnimatorOnSpawn = true;

        private readonly Queue<GameObject> availableInstances = new Queue<GameObject>();
        private bool warmedUp;

        public GameObject Prefab => prefab;

        private void Awake()
        {
            if (poolRoot == null)
            {
                GameObject rootObject = new GameObject($"{name}_PoolRoot");
                rootObject.transform.SetParent(transform, false);
                poolRoot = rootObject.transform;
            }
        }

        private void Start()
        {
            PrewarmIfNeeded();
        }

        /// <summary>
        /// 한국어 주석: 미리 인스턴스를 만들어 두어 첫 스폰 시 GC 스파이크를 줄입니다.
        /// </summary>
        public void PrewarmIfNeeded()
        {
            if (warmedUp || prefab == null)
            {
                return;
            }

            warmedUp = true;
            for (int index = 0; index < prewarmCount; index++)
            {
                GameObject instance = Instantiate(prefab, poolRoot);
                instance.name = $"{prefab.name}_Pooled";
                instance.SetActive(false);
                availableInstances.Enqueue(instance);
            }
        }

        /// <summary>
        /// 한국어 주석: 풀에서 꺼내거나 새로 생성해 활성화합니다.
        /// </summary>
        public GameObject Get(Vector3 worldPosition, Quaternion worldRotation)
        {
            PrewarmIfNeeded();
            GameObject instance = TryDequeueExisting();
            if (instance == null)
            {
                if (prefab == null)
                {
                    Debug.LogError("[GameObjectPool] Prefab이 비어 있습니다.", this);
                    return null;
                }

                instance = Instantiate(prefab, poolRoot);
                instance.name = $"{prefab.name}_Pooled";
            }

            instance.transform.SetPositionAndRotation(worldPosition, worldRotation);
            instance.SetActive(true);
            ApplyAnimatorResetIfNeeded(instance);
            return instance;
        }

        /// <summary>
        /// 한국어 주석: 인스턴스를 비활성화하고 풀에 반환합니다.
        /// </summary>
        public void Release(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(poolRoot, false);
            availableInstances.Enqueue(instance);
        }

        private GameObject TryDequeueExisting()
        {
            while (availableInstances.Count > 0)
            {
                GameObject candidate = availableInstances.Dequeue();
                if (candidate != null)
                {
                    return candidate;
                }
            }

            return null;
        }

        private void ApplyAnimatorResetIfNeeded(GameObject instance)
        {
            if (!resetAnimatorOnSpawn || instance == null)
            {
                return;
            }

            Animator animator = instance.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                return;
            }

            // 한국어 주석: 재사용 시 상태가 고착되지 않도록 바인딩을 초기화합니다.
            animator.Rebind();
            animator.Update(0.0f);
        }
    }
}
