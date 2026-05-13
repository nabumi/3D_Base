using UnityEngine;

namespace SystemicOverload.Data
{
    /// <summary>
    /// 한국어 주석: 한 웨이브에서 스폰할 적 프리팹과 개수·간격을 정의합니다. Object Pool과 함께 사용하는 것을 전제로 합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveData", menuName = "Systemic Overload/Data/Wave Data", order = 2)]
    public sealed class WaveData : ScriptableObject
    {
        [Header("Spawn")]
        [SerializeField] private GameObject pooledEnemyPrefab;
        [SerializeField] private int enemyCount = 5;
        [SerializeField] private float spawnIntervalSeconds = 1.25f;
        [SerializeField] private float spawnRadius = 8.0f;
        [SerializeField] private Vector3 spawnCenterWorldOffset = new Vector3(0.0f, 0.0f, 12.0f);

        public GameObject PooledEnemyPrefab => pooledEnemyPrefab;
        public int EnemyCount => Mathf.Max(0, enemyCount);
        public float SpawnIntervalSeconds => Mathf.Max(0.05f, spawnIntervalSeconds);
        public float SpawnRadius => Mathf.Max(0.5f, spawnRadius);
        public Vector3 SpawnCenterWorldOffset => spawnCenterWorldOffset;

        private void OnValidate()
        {
            enemyCount = Mathf.Max(0, enemyCount);
            spawnIntervalSeconds = Mathf.Max(0.05f, spawnIntervalSeconds);
            spawnRadius = Mathf.Max(0.5f, spawnRadius);
        }
    }
}
