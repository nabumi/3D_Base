using System;
using SystemicOverload.Data;
using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// 체력과 사망 상태를 관리하고, <see cref="IDamageable"/> 계약을 구현합니다.
    /// <para>한국어 주석: <see cref="statData"/>가 있으면 최대 체력과 피해 배율을 데이터에서 가져옵니다.</para>
    /// </summary>
    public sealed class HealthComponent : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [Tooltip("설정 시 인스펙터의 maxHealth보다 우선합니다.")]
        [SerializeField] private StatData statData;

        [SerializeField] private float maxHealth = 100.0f;
        [SerializeField] private float currentHealth = 100.0f;

        /// <summary>
        /// 피해 적용 직후(사망 전)에 호출됩니다. UI/VFX 연동에 사용합니다.
        /// </summary>
        public event Action<float, float> Damaged;

        /// <summary>
        /// 사망 시 한 번 호출됩니다.
        /// </summary>
        public event Action Died;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsAlive => currentHealth > 0.0f;

        private void Awake()
        {
            ApplyStatDataIfPresent();
            currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth);
        }

        private void OnValidate()
        {
            ApplyStatDataIfPresent();
            maxHealth = Mathf.Max(1.0f, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth);
        }

        /// <inheritdoc />
        public void ApplyDamage(in DamagePayload payload)
        {
            if (!IsAlive)
            {
                return;
            }

            float appliedAmount = Mathf.Max(0.0f, payload.Amount);
            if (appliedAmount <= 0.0f)
            {
                return;
            }

            float damageMultiplier = statData != null ? statData.DamageTakenMultiplier : 1.0f;
            appliedAmount *= damageMultiplier;

            currentHealth = Mathf.Max(0.0f, currentHealth - appliedAmount);
            Damaged?.Invoke(appliedAmount, currentHealth);

            if (currentHealth <= 0.0f)
            {
                Died?.Invoke();
            }
        }

        /// <summary>
        /// 에디터/디버그용으로 체력을 초기화합니다.
        /// </summary>
        public void ResetHealthToFull()
        {
            currentHealth = maxHealth;
        }

        /// <summary>
        /// 한국어 주석: StatData가 연결되어 있으면 최대 체력을 동기화합니다.
        /// </summary>
        private void ApplyStatDataIfPresent()
        {
            if (statData == null)
            {
                return;
            }

            maxHealth = statData.MaxHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth);
        }
    }
}
