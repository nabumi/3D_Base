using UnityEngine;

namespace SystemicOverload.Pooling
{
    /// <summary>
    /// 한국어 주석: 특정 조건(수명 종료)에서 자동으로 풀에 반환하는 보조 컴포넌트입니다. VFX/프로젝타일 검증에 사용합니다.
    /// </summary>
    public sealed class PooledReturnTrigger : MonoBehaviour
    {
        [SerializeField] private GameObjectPool ownerPool;
        [SerializeField] private float lifetimeSeconds = 2.0f;

        private float despawnTime;

        private void OnEnable()
        {
            despawnTime = Time.time + Mathf.Max(0.05f, lifetimeSeconds);
        }

        private void Update()
        {
            if (Time.time < despawnTime)
            {
                return;
            }

            if (ownerPool != null)
            {
                ownerPool.Release(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 한국어 주석: 스폰 직후 풀 참조를 주입합니다(런타임).
        /// </summary>
        public void Configure(GameObjectPool pool, float lifetime)
        {
            ownerPool = pool;
            lifetimeSeconds = Mathf.Max(0.05f, lifetime);
            despawnTime = Time.time + lifetimeSeconds;
        }
    }
}
