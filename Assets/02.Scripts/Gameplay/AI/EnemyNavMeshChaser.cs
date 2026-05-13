using UnityEngine;
using UnityEngine.AI;

namespace SystemicOverload.AI
{
    /// <summary>
    /// 한국어 주석: NavMeshAgent로 플레이어(이름 기준 탐색)를 추격하는 단순 AI입니다. Behavior Tree 전 단계의 스모크 검증용입니다.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyAiBlackboard))]
    public sealed class EnemyNavMeshChaser : MonoBehaviour
    {
        [SerializeField] private string playerObjectName = "Player";
        [SerializeField] private float repathIntervalSeconds = 0.35f;
        [SerializeField] private float stoppingDistance = 1.25f;

        private NavMeshAgent navMeshAgent;
        private EnemyAiBlackboard blackboard;
        private float nextRepathTime;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            blackboard = GetComponent<EnemyAiBlackboard>();
            navMeshAgent.stoppingDistance = stoppingDistance;
        }

        private void Start()
        {
            GameObject playerObject = GameObject.Find(playerObjectName);
            if (playerObject != null)
            {
                blackboard.BindChaseTarget(playerObject.transform);
            }
            else
            {
                Debug.LogWarning($"[EnemyNavMeshChaser] '{playerObjectName}' 오브젝트를 찾지 못했습니다.", this);
            }
        }

        private void Update()
        {
            if (blackboard == null || navMeshAgent == null || !blackboard.HasChaseTarget)
            {
                return;
            }

            if (Time.time < nextRepathTime)
            {
                return;
            }

            nextRepathTime = Time.time + Mathf.Max(0.05f, repathIntervalSeconds);
            navMeshAgent.SetDestination(blackboard.LastKnownTargetPosition);
        }
    }
}
