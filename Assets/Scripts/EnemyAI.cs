using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필요

public class EnemyAI : MonoBehaviour
{
   
    public float moveSpeed = 3f;           // 이동 속도
    public float sightRange = 8f;          // 플레이어 인식 거리
    public float attackRange = 1.5f;       // 공격을 시작하는 거리
    public float attackCooldown = 2f;      // 다음 공격까지의 쿨타임
    public Collider2D attackCollider;       // 적의 공격 판정 콜라이더 (Inspector에서 연결)

    [Header("Jump Settings")]
    public float jumpForce = 7f; // 점프에 가해지는 힘
    public float minJumpCooldown = 2f; // 최소 쿨타임 (예: 2초)
    public float maxJumpCooldown = 6f; // 최대 쿨타임 (예: 6초)

    [Header("Ground Check")]
    public Transform groundCheck; // Inspector에서 GroundCheck 오브젝트 연결
    public LayerMask whatIsGround; // Ground 레이어를 Inspector에서 선택

    [Header("Attack Selection")]
    [Range(0f, 1f)]
    public float chargeAttackChance = 0.15f; // 0.15f = 15% 확률로 특수 공격 선택

    [Header("Charge Attack Settings")]
    public float executeJumpForce = 15f; // 실행 시 가해질 폭발적인 점프 힘

    [Header("Damage Values")]
    public float normalAttackDamage = 10f;  // 일반 공격 데미지
    public float chargeAttackDamage = 30f; // 특수 공격 데미지

    [Header("Gravity Settings")]
    public float normalGravityScale = 1.0f; // 일반적인 중력 (점프 상승 중)
    public float fastFallGravityScale = 5.0f; // 빠르게 하강할 때의 중력 배율

    private int currentAttackType = 0; // 0: 대기, 1: 일반, 2: 특수


    private Transform player;              // 추격 대상 (플레이어)
    private Rigidbody2D rb;
    private Animator animator;
    private bool canAttack = true;         // 현재 공격 가능한 상태인지 확인
    private bool isStunned = false;        // 현재 기절 상태인지 확인 (패링용)
    private float lastJumpTime; // 마지막 점프 시간
    private bool isGrounded; // 현재 땅에 닿았는지 여부
    private float nextJumpCooldown; // 다음 점프까지 필요한 무작위 쿨타임 값
    private bool isExecutingSpecialAttack = false; // 특수 공격 실행 중 상태 플래그

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = normalGravityScale; // 시작 시 일반 중력 설정

        lastJumpTime = Time.time; // 초기 쿨타임 설정

        nextJumpCooldown = Random.Range(minJumpCooldown, maxJumpCooldown); // 초기 쿨타임을 무작위로 설정
        // 게임 시작 시 "Player" 태그를 가진 오브젝트를 찾아서 target에 연결
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        // 기절했거나 특수 공격 중이라면 AI 중단
        if (player == null || isStunned || isExecutingSpecialAttack)
        {
            // 특수 공격 중에는 낙하를 허용해야 하므로 Y 속도는 살려둠
            if (isExecutingSpecialAttack)
            {
                rb.velocity = new Vector2(0, rb.velocity.y); // Y 속도는 보존 (낙하)
            }
            else // 기절 또는 플레이어 없을 때
            {
                rb.velocity = Vector2.zero; // 모든 움직임 정지
            }
            animator.SetFloat("speed", 0);
            CheckIfGrounded(); // 땅 감지는 계속 함
            return;
        }

        // 기본 상태 업데이트
        CheckIfGrounded();

        // 착지 시 애니메이션 정리 (IsJumping Bool 끄기)
        if (isGrounded && animator.GetBool("IsJumping"))
        {
            animator.SetBool("IsJumping", false);
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 일반 AI 및 공격 실행
        if (distanceToPlayer < sightRange)
        {
            if (distanceToPlayer <= attackRange)
            {
                // 공격 준비 상태: 수평 이동 정지 (Y는 중력에 맡김)
                rb.velocity = Vector2.zero;
                animator.SetFloat("speed", 0);

                if (isGrounded)
                {
                    rb.AddForce(Vector2.down * 30f, ForceMode2D.Force);
                }

                if (canAttack)
                {
                    PerformAttack(); // 이 함수 내부에서 isExecutingSpecialAttack을 true로 설정
                }
            }
            else // 추격
            {
                ChasePlayer();
            }
        }
        else // 인식 범위 밖: 멈춤
        {
            rb.velocity = Vector2.zero;
            animator.SetFloat("speed", 0);
        }

        // 랜덤 점프 로직
        if (CanJump() && isGrounded)
        {
            Jump();
        }
    }

    void PerformAttack()
    {
        canAttack = false; // 공격 시작과 동시에 쿨타임 시작 플래그

        if (Random.value <= chargeAttackChance)
        {
            animator.SetTrigger("ChargeAttackTrigger");
            currentAttackType = 2;
            isExecutingSpecialAttack = true; // 특수 공격중 랜덤 점프 금지
        }


        // Random.value는 0.0f와 1.0f 사이의 무작위 실수
        if (Random.value <= chargeAttackChance)
        {
            Debug.Log("AI 선택: 특수 공격 (Charge)");
            // 15% 확률로 특수 공격 선택
            animator.SetTrigger("ChargeAttackTrigger");
            currentAttackType = 2;
        }
        else
        {
            Debug.Log("AI 선택: 일반 공격 (Normal)");
            // 85% 확률로 일반 공격 선택
            animator.SetTrigger("NormalAttackTrigger");
            currentAttackType = 1;
        }
    }

    void CheckIfGrounded()
    {
        // OverlapCircle을 사용하여 GroundCheck 위치에서 whatIsGround 레이어와 겹치는지 확인
        // 0.1f는 감지할 원의 반지름
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, whatIsGround);
        }
        else
        {
            isGrounded = true; // GroundCheck 오브젝트가 없으면 기본적으로 땅에 있다고 가정
        }
    }

    private bool CanJump()
    {
        // 1. 점프 쿨타임이 지났는지 확인
        if (Time.time < lastJumpTime + nextJumpCooldown)
        {
            return false;
        }
        return true;
    }

    public void Jump()
    {
        // 애니메이션 실행
        animator.SetBool("IsJumping", true);

        // 위쪽으로 힘 가하기
        rb.velocity = new Vector2(rb.velocity.x, 0f); // 수직 속도 초기화
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // 마지막 점프 시간 업데이트
        lastJumpTime = Time.time;

        // 점프 후, 다음 점프를 위한 새로운 무작위 쿨타임 설정
        nextJumpCooldown = Random.Range(minJumpCooldown, maxJumpCooldown);
    }

    void ChasePlayer()
    {
        // 타겟 방향으로 이동
        Vector3 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        // 애니메이션 및 방향 전환
        animator.SetFloat("speed", Mathf.Abs(direction.x));
        FlipTowardsTarget(direction.x);
    }

    void FlipTowardsTarget(float directionX)
    {
        if (directionX > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (directionX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    
    // 애니메이션 이벤트: 일반 공격 종료 시 호출
    public void EndNormalAttack()
    {
        StartCoroutine(AttackCooldownRoutine());
    }

    // PlayerHealth가 호출하는 스턴 함수
    public void StunEnemy(float duration)
    {
        // PlayerHealth 스크립트가 패링 성공 시 이 함수를 호출
        StartCoroutine(StunRoutine(duration));
    }


    // === 애니메이션 이벤트로 호출되는 함수 (플레이어에게 피해를 줌) ===
    public void DealDamageToPlayer()
    {
        if (player == null || isStunned) return;
        if (attackCollider == null)
        {
            Debug.LogError("Attack Collider가 Inspector에 연결되지 않았습니다!");
            return;
        }

        // 현재 타입에 따른 데미지 값 설정
        float damageToDeal = 0f;

        switch (currentAttackType)
        {
            case 1:
                damageToDeal = normalAttackDamage;
                break;
            case 2:
                damageToDeal = chargeAttackDamage;
                break;
            default:
                Debug.LogWarning("공격 타입이 설정되지 않았습니다! 데미지 0 처리.");
                return; // 타입이 설정 안 되었다면 데미지 없이 종료
        }

        // 공격 범위 내의 모든 콜라이더를 감지합니다.
        Collider2D[] hitObjects = Physics2D.OverlapBoxAll(attackCollider.bounds.center, attackCollider.bounds.size, 0);

        foreach (Collider2D hit in hitObjects)
        {
            // 감지된 오브젝트의 태그가 "Player"인지 확인합니다.
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // TakeDamage 호출 시 자신의 GameObject를 넘겨줍니다.
                    playerHealth.TakeDamage(damageToDeal, this.gameObject);
                    Debug.Log($"플레이어가 {damageToDeal} 데미지를 입었습니다!");
                }
            }
        }
    }

    // === 쿨타임 관리 코루틴 ===
    private IEnumerator AttackCooldownRoutine()
    {
        // Attack 애니메이션이 완전히 끝날 때까지 기다린 후 쿨타임 시작
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // 설정된 쿨타임 시간만큼 대기
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true; // 쿨타임 종료, 다시 공격 가능
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        rb.velocity = Vector2.zero; // 기절 시 움직임 정지
        animator.SetTrigger("Hurt"); // 피격 애니메이션(Hurt)을 스턴 모션으로 재활용

        // 지정된 시간(duration)만큼 대기
        yield return new WaitForSeconds(duration);

        isStunned = false;
    }

    // JumpExecute 애니메이션이 끝났을 때 호출될 함수
    public void EndChargeAttack()
    {
        // 특수 공격의 쿨타임 코루틴을 시작하여 canAttack = true로 만드는 작업을 예약
        StartCoroutine(AttackCooldownRoutine());

        // 중력 스케일을 정상
        rb.gravityScale = normalGravityScale;

        // 특수 공격 상태 플래그 해제 (랜덤 점프 재개 허용)
        isExecutingSpecialAttack = false;

        currentAttackType = 0; // 대기 상태로 리셋

        Debug.Log("특수 공격 패턴 종료 및 쿨타임 시작.");
    }

    private IEnumerator ApplyFastFallRoutine()
    {
        // 점프 힘이 완전히 적용될 때까지 아주 짧게 대기
        yield return new WaitForSeconds(0.1f);

        // 중력을 강하게 설정하여 빠르게 하강하도록
        if (!isGrounded)
        {
            rb.gravityScale = fastFallGravityScale;
        }
    }

    // ExecuteJumpAttack 이벤트가 호출하는 함수
    public void StartJumpAttackExecution()
    {
        // 중복 호출을 막기 위해 코루틴으로 실행
        StartCoroutine(ExecuteJumpAttackRoutine());
    }

    // 점프 실행 및 중력 관리 코루틴
    private IEnumerator ExecuteJumpAttackRoutine()
    {
        // 중력 초기화 및 속도 초기화 (안정적인 점프 준비)
        rb.gravityScale = normalGravityScale;

        // 수직 속도를 0으로 설정하여 이전에 남아있던 속도를 제거
        rb.velocity = new Vector2(rb.velocity.x, 0f);

        // executeJumpForce를 Y 속도로 직접 설정하여 정확한 점프 속도를 부여
        rb.velocity = new Vector2(rb.velocity.x, executeJumpForce);

        // 딜레이(0.05초)
        // 물리 엔진이 점프력을 적용할 수 있도록 아주 짧은 시간만 대기
        yield return new WaitForSeconds(0.05f);

        // Rigidbody 2D가 움직이고 있는지 최종 확인 후, 하강 시작
        if (!isGrounded)
        {
            // 강한 중력 적용
            rb.gravityScale = fastFallGravityScale;
        }
    }

}