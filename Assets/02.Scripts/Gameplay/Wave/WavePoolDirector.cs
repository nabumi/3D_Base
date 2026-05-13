using System.Collections;
using SystemicOverload.Data;
using SystemicOverload.Pooling;
using UnityEngine;

namespace SystemicOverload.Wave
{
    /// <summary>
    /// 한국어 주석: <see cref="WaveData"/>에 따라 적 인스턴스를 풀에서 꺼내 월드에 배치합니다.
    /// </summary>
    public sealed class WavePoolDirector : MonoBehaviour
    {
        [SerializeField] private WaveData waveData;
        [SerializeField] private GameObjectPool enemyPool;
        [SerializeField] private Transform spawnAnchor;

        private Coroutine runningWaveRoutine;

        private void OnValidate()
        {
            if (spawnAnchor == null)
            {
                spawnAnchor = transform;
            }
        }

        private void Start()
        {
            if (waveData == null || enemyPool == null)
            {
                Debug.LogWarning("[WavePoolDirector] WaveData 또는 GameObjectPool이 비어 있습니다.", this);
                return;
            }

            if (runningWaveRoutine != null)
            {
                StopCoroutine(runningWaveRoutine);
            }

            runningWaveRoutine = StartCoroutine(RunWaveRoutine());
        }

        private IEnumerator RunWaveRoutine()
        {
            Vector3 center = (spawnAnchor != null ? spawnAnchor.position : transform.position) + waveData.SpawnCenterWorldOffset;
            for (int spawnIndex = 0; spawnIndex < waveData.EnemyCount; spawnIndex++)
            {
                Vector2 disk = Random.insideUnitCircle * waveData.SpawnRadius;
                Vector3 spawnPosition = center + new Vector3(disk.x, 0.0f, disk.y);
                Quaternion spawnRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
                GameObject spawned = enemyPool.Get(spawnPosition, spawnRotation);
                if (spawned != null)
                {
                    HealthResetOnSpawn healthReset = spawned.GetComponent<HealthResetOnSpawn>();
                    healthReset?.ApplyFullHealth();
                }

                yield return new WaitForSeconds(waveData.SpawnIntervalSeconds);
            }
        }
    }
}
