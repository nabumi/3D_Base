using UnityEngine;

namespace SystemicOverload.AI
{
    /// <summary>
    /// 한국어 주석: Behavior/Animator가 공유할 최소 Blackboard입니다. Phase 4에서 상태 전이의 단일 소스로 사용합니다.
    /// </summary>
    public sealed class EnemyAiBlackboard : MonoBehaviour
    {
        [SerializeField] private Transform chaseTarget;

        public Transform ChaseTarget => chaseTarget;
        public Vector3 LastKnownTargetPosition { get; private set; }
        public bool HasChaseTarget => chaseTarget != null;

        private void Update()
        {
            if (chaseTarget != null)
            {
                LastKnownTargetPosition = chaseTarget.position;
            }
        }

        /// <summary>
        /// 한국어 주석: 런타임에 추적 대상을 설정합니다(Validation Scene 등).
        /// </summary>
        public void BindChaseTarget(Transform targetTransform)
        {
            chaseTarget = targetTransform;
        }
    }
}
