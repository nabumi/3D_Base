using UnityEngine;
using StarterAssets;

namespace StarterAssets
{
    public class PlayerCombat : MonoBehaviour
    {
        public bool isAttacking = false;
        private Animator anim;
        private StarterAssetsInputs _input;
        private ThirdPersonController _controller;

        [Header("Attack Settings")]
        [SerializeField] private Transform attackPoint;    // 공격 중심점 (플레이어 앞)
        [SerializeField] private float attackRange = 1.5f; // 공격 반지름 (1.5~2m)
        [SerializeField] private LayerMask enemyLayer;     // 대상 레이어 (Enemy)
        [SerializeField] private int damage = 20;          // 대미지 (10~20)

        private void Awake()
        {
            anim = GetComponent<Animator>();
            _input = GetComponent<StarterAssetsInputs>();
            _controller = GetComponent<ThirdPersonController>();
        }

        private void Update()
        {
            if (_input != null && _input.attack && !isAttacking)
            {
                Attack();
            }
        }

        void Attack()
        {
            isAttacking = true;


            if (_controller != null) _controller.LaunchCharacter(6.0f);
            if (anim != null) anim.SetTrigger("Attack1");

            Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayer);

            foreach (Collider enemy in hitEnemies)
            {
                if (enemy.TryGetComponent(out Enemy targetEnemy))
                {
                    targetEnemy.TakeDamage(damage);
                    Debug.Log($"{enemy.name}에게 {damage} 대미지!");
                }
            }

            _input.attack = false;
        }

        public void EndAttack() => isAttacking = false;
        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}