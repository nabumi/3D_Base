using UnityEngine;
using StarterAssets; // 스타터 에셋과 연결하기 위해 필수!

namespace StarterAssets
{
    public class PlayerCombat : MonoBehaviour
    {
        public bool isAttacking = false;
        private Animator anim;
        private StarterAssetsInputs _input; // 새로운 입력 시스템 참조
        private ThirdPersonController _controller;

        [Header("Attack Settings")]
        public Transform attackPoint;
        public float attackRange = 0.5f;
        public LayerMask enemyLayer;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            // 같은 오브젝트에 있는 StarterAssetsInputs 스크립트를 가져옵니다.
            _input = GetComponent<StarterAssetsInputs>();
            _controller = GetComponent<ThirdPersonController>();
        }

        private void Update()
        {
            // [중요] Input.GetMouseButtonDown 대신 _input.attack을 사용합니다!
            if (_input != null && _input.attack && !isAttacking)
            {
                Attack();
            }

            if (_input != null && _input.point && !isAttacking)
            {
                Point();
            }
        }

        void Attack()
        {
            isAttacking = true;

            if (_controller != null)
            {
   
                _controller.LaunchCharacter(6.0f);
            }

            if (anim != null) anim.SetTrigger("Attack1");

            // 공격 버튼을 한 번 눌렀을 때 한 번만 동작하도록 false로 초기화
            _input.attack = false;
        }
        void Point()
        {
            isAttacking = true; // 가리키는 동안 이동 제한을 위해 true 설정
            if (anim != null) anim.SetTrigger("Point"); // 애니메이터의 Trigger 이름
            _input.point = false;
        }

        // 애니메이션 이벤트(EndAttack)에서 호출될 함수
        public void EndAttack()
        {
            isAttacking = false;
        }
    }
}