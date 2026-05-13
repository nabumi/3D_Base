using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SystemicOverload.AddressablesSupport
{
    /// <summary>
    /// 한국어 주석: Addressables 라벨 또는 키 목록을 미리 로드해 첫 프레임 hitch를 줄입니다.
    /// </summary>
    public sealed class AddressablesWarmupUtility : MonoBehaviour
    {
        [SerializeField] private List<AssetReferenceGameObject> preloadReferences = new List<AssetReferenceGameObject>();
        private readonly List<AsyncOperationHandle> loadedHandles = new List<AsyncOperationHandle>();

        private void Start()
        {
            foreach (AssetReferenceGameObject reference in preloadReferences)
            {
                if (reference == null || !reference.RuntimeKeyIsValid())
                {
                    continue;
                }

                AsyncOperationHandle<GameObject> handle = reference.LoadAssetAsync<GameObject>();
                loadedHandles.Add(handle);
            }
        }

        private void OnDestroy()
        {
            foreach (AsyncOperationHandle handle in loadedHandles)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            loadedHandles.Clear();
        }
    }
}
