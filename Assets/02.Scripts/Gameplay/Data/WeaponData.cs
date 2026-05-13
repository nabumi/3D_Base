using UnityEngine;

namespace SystemicOverload.Data
{
    /// <summary>
    /// 한국어 주석: 히트 스캔 무기의 수치를 ScriptableObject로 정의합니다. <see cref="Combat.CombatComponent"/>가 런타임에 이 값을 적용합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Systemic Overload/Data/Weapon Data", order = 1)]
    public sealed class WeaponData : ScriptableObject
    {
        [Header("Hit Scan")]
        [SerializeField] private float hitScanDamage = 12.0f;
        [SerializeField] private float shotsPerSecond = 4.0f;
        [SerializeField] private float maxRange = 40.0f;
        [SerializeField] private float rayOriginHeight = 1.0f;
        [SerializeField] private float rayStartForwardOffset = 0.35f;
        [SerializeField] private LayerMask hitLayerMask = ~0;

        public float HitScanDamage => Mathf.Max(0.0f, hitScanDamage);
        public float ShotsPerSecond => Mathf.Max(0.01f, shotsPerSecond);
        public float MaxRange => Mathf.Max(0.1f, maxRange);
        public float RayOriginHeight => Mathf.Max(0.0f, rayOriginHeight);
        public float RayStartForwardOffset => rayStartForwardOffset;
        public LayerMask HitLayerMask => hitLayerMask;

        private void OnValidate()
        {
            hitScanDamage = Mathf.Max(0.0f, hitScanDamage);
            shotsPerSecond = Mathf.Max(0.01f, shotsPerSecond);
            maxRange = Mathf.Max(0.1f, maxRange);
            rayOriginHeight = Mathf.Max(0.0f, rayOriginHeight);
        }
    }
}
