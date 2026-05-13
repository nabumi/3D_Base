using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SystemicOverload.AddressablesSupport
{
    /// <summary>
    /// 한국어 주석: Addressables로 VFX 프리팹을 한 번 로드·인스턴스하고 수명 후 해제합니다. Phase 5 정책의 최소 샘플입니다.
    /// </summary>
    public sealed class AddressablesOneShotVfx : MonoBehaviour
    {
        [SerializeField] private AssetReferenceGameObject vfxReference;
        [SerializeField] private float instanceLifetimeSeconds = 1.5f;
        [SerializeField] private Vector3 localSpawnOffset = new Vector3(0.0f, 1.0f, 0.5f);

        /// <summary>
        /// 한국어 주석: 기본 오프셋 위치에 VFX를 스폰합니다. Address가 유효하지 않으면 경고만 출력합니다.
        /// </summary>
        public void SpawnAtDefaultOffset()
        {
            Vector3 worldPosition = transform.TransformPoint(localSpawnOffset);
            StartCoroutine(SpawnRoutine(worldPosition, Quaternion.identity));
        }

        private IEnumerator SpawnRoutine(Vector3 position, Quaternion rotation)
        {
            if (vfxReference == null || !vfxReference.RuntimeKeyIsValid())
            {
                Debug.LogWarning("[AddressablesOneShotVfx] AssetReference가 비어 있거나 Addressables 키가 유효하지 않습니다.", this);
                yield break;
            }

            AsyncOperationHandle<GameObject> handle = vfxReference.InstantiateAsync(position, rotation);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogWarning("[AddressablesOneShotVfx] 인스턴스 생성에 실패했습니다.", this);
                yield break;
            }

            GameObject instance = handle.Result;
            float lifetime = Mathf.Max(0.05f, instanceLifetimeSeconds);
            yield return new WaitForSeconds(lifetime);

            if (instance != null)
            {
                vfxReference.ReleaseInstance(instance);
            }
        }
    }
}
