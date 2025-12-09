using UnityEngine;
using System.Collections;

public class BossAI : MonoBehaviour
{
    // === Enum: 보스의 상태와 페이즈 정의 ===
    public enum BossPhase { PhaseOne = 1, PhaseTwo = 2 } // 1, 2페이즈 명시
    public BossPhase currentPhase = BossPhase.PhaseOne;
    public Collider2D bossAttackCollider;
    public float attackDamage = 15f;

    // === Public Variables (Inspector에서 설정) ===
    public float moveSpeed = 3f;
    public float phaseTwoSpeedMultiplier = 1.5f;
    public float attackRange = 2f;
    public int comboCount = 3;               // 1페이즈 기본 콤보 횟수 (A, B, C)

    // === Private Variables ===
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isAttacking = false;
    private float currentMoveSpeed;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        player = FindObjectOfType<PlayerController>()?.transform;

        currentMoveSpeed = moveSpeed;

        // 보스가 씬에 생성될 때 애니메이터에 초기 페이즈를 전달합니다.
        if (animator != null)
        {
            animator.SetInteger("Phase", (int)currentPhase);
        }
        Debug.Log("BossAI 초기화 완료. 현재 페이즈: " + currentPhase.ToString());
    }

    void Update()
    {
        if (player == null || isAttacking)
        {
            // 공격 중이거나 플레이어가 없으면 이동 중지 및 IDLE 애니메이션
            rb.velocity = Vector2.zero;
            if (animator != null) animator.SetFloat("Speed", 0f);
            return;
        }

        // 플레이어 추적 및 이동 (달리기 애니메이션 제어)
        HandleMovement();

        // 공격 범위 체크 및 공격
        CheckAttackRange();
    }

    // === 이동 로직 (달리기 애니메이션 통합) ===
    void HandleMovement()
    {
        Vector2 targetDirection = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(targetDirection.x * currentMoveSpeed, rb.velocity.y);

        // 애니메이터에 이동 속도 전달 (IDLE ↔ RUN 전환)
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(targetDirection.x * currentMoveSpeed));
        }

        // 스프라이트 방향 전환
        if (targetDirection.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (targetDirection.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    // === 공격 로직 ===
    void CheckAttackRange()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            rb.velocity = Vector2.zero;
            StartCoroutine(PerformAttack());
        }
    }

    // === 공격 실행 코루틴 (4가지 공격 패턴 및 페이즈 관리) ===
    IEnumerator PerformAttack()
    {
        isAttacking = true;

        if (currentPhase == BossPhase.PhaseOne)
        {
            // 1페이즈: 3연속 기본 공격 콤보 (Attack A, B, C)
            for (int i = 1; i <= comboCount; i++)
            {
                if (animator != null)
                {
                    // 1, 2, 3 중 랜덤 선택 가능하나, 여기선 순차적으로 (Attack A, B, C에 매핑)
                    animator.SetInteger("AttackIndex", i);
                    animator.SetTrigger("AttackTrigger");
                }
                yield return new WaitForSeconds(0.6f); // 각 공격 간의 딜레이
            }
        }
        else // Phase Two (2페이즈 이상)
        {
            // 2페이즈: 특수 공격 (DASH ATTACK) 실행
            if (animator != null)
            {
                // AttackIndex를 4로 설정하거나, Phase 파라미터를 사용하여 특수 공격 애니메이션으로 직접 전환
                animator.SetInteger("Phase", (int)currentPhase); // 2페이즈 조건을 애니메이터에 전달
                animator.SetTrigger("AttackTrigger");
            }
            yield return new WaitForSeconds(1.2f); // 특수 공격 애니메이션 시간
        }

        isAttacking = false;
        yield return new WaitForSeconds(1.0f); // 패턴 종료 후 재시작 딜레이
    }

    // === 보스 페이즈 전환 함수 (BossHealth.cs에서 호출) ===
    public void ChangePhase(int phaseNumber)
    {
        if (phaseNumber == 2 && currentPhase == BossPhase.PhaseOne)
        {
            currentPhase = BossPhase.PhaseTwo;

            // 2페이즈 전환 시 능력치/속도 변경
            currentMoveSpeed = moveSpeed * phaseTwoSpeedMultiplier;

            // 애니메이터에 새로운 페이즈 전달 (특수 공격 전환 조건으로 사용)
            if (animator != null)
            {
                animator.SetInteger("Phase", (int)currentPhase);
            }

            // 패턴 재시작
            StopAllCoroutines();
            isAttacking = false;
            Debug.Log("BossAI: Phase Two로 전환! 속도: " + currentMoveSpeed);
        }
    }

    // ⭐️ 1. 애니메이션 이벤트에서 호출 (공격 판정 시작 시) ⭐️
    public void EnableHitbox()
    {
        if (bossAttackCollider != null)
        {
            bossAttackCollider.enabled = true;
            Debug.Log("Boss Hitbox Enabled!");
        }
    }

    // ⭐️ 2. 애니메이션 이벤트에서 호출 (공격 판정 종료 시) ⭐️
    public void DisableHitbox()
    {
        if (bossAttackCollider != null)
        {
            bossAttackCollider.enabled = false;
            Debug.Log("Boss Hitbox Disabled!");
        }
    }


    // ⭐️ 3. 공격 콜라이더가 플레이어와 충돌했을 때 (Is Trigger = true일 때) ⭐️
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 태그 확인
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // 플레이어의 TakeDamage 함수 호출
                playerHealth.TakeDamage(attackDamage, gameObject);

                // 한 번 공격했으면 콜라이더를 비활성화 (다중 타격 방지)
                DisableHitbox();
            }
        }
    }
}