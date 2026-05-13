using UnityEngine;

namespace SystemicOverload.Data
{
    /// <summary>
    /// 한국어 주석: 캐릭터/적의 기본 생존·피해 계수를 데이터로 분리합니다. 인스펙터 튜닝과 Phase별 밸런스 조정에 사용합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "StatData", menuName = "Systemic Overload/Data/Stat Data", order = 0)]
    public sealed class StatData : ScriptableObject
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100.0f;

        [Header("Mitigation")]
        [Tooltip("들어오는 최종 피해에 곱합니다. 1이면 기본, 0.5면 절반만 받습니다.")]
        [SerializeField] [Range(0.0f, 2.0f)] private float damageTakenMultiplier = 1.0f;

        public float MaxHealth => Mathf.Max(1.0f, maxHealth);
        public float DamageTakenMultiplier => Mathf.Clamp(damageTakenMultiplier, 0.0f, 2.0f);

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1.0f, maxHealth);
        }
    }
}
