using SystemicOverload.Combat;
using UnityEngine;

namespace SystemicOverload.Wave
{
    /// <summary>
    /// 한국어 주석: 풀에서 재활성화될 때 체력을 최대로 맞춥니다.
    /// </summary>
    public sealed class HealthResetOnSpawn : MonoBehaviour
    {
        [SerializeField] private HealthComponent healthComponent;

        private void Awake()
        {
            healthComponent ??= GetComponent<HealthComponent>();
        }

        private void OnEnable()
        {
            ApplyFullHealth();
        }

        /// <summary>
        /// 한국어 주석: 외부 스폰 로직에서 명시적으로 호출할 수 있습니다.
        /// </summary>
        public void ApplyFullHealth()
        {
            if (healthComponent == null)
            {
                return;
            }

            healthComponent.ResetHealthToFull();
        }
    }
}
