using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필요

public class EnemyAI : MonoBehaviour
{
   
    public float moveSpeed = 3f;           // 이동 속도
    public float sightRange = 8f;          // 플레이어 인식 거리
    public float attackRange = 1.5f;       // 공격을 시작하는 거리
    public float attackDamage = 10f;       // 적의 공격력
    public float attackCooldown = 2f;      // 다음 공격까지의 쿨타임
    public Collider2D attackCollider;       // 적의 공격 판정 콜라이더 (Inspector에서 연결)

    [Header("Jump Settings")]
    public float jumpForce = 7f; // 점프에 가해지는 힘
    public float minJumpCooldown = 2f; // 최소 쿨타임 (예: 2초)
    public float maxJumpCooldown = 6f; // 최대 쿨타임 (예: 6초)

    [Header("Ground Check")]
    public Transform groundCheck; // Inspector에서 GroundCheck 오브젝트 연결
    public LayerMask whatIsGround; // Ground 레이어를 Inspector에서 선택

    private Transform player;              // 추격 대상 (플레이어)
    private Rigidbody2D rb;
    private Animator animator;
    private bool canAttack = true;         // 현재 공격 가능한 상태인지 확인
    private bool isStunned = false;        // 현재 기절 상태인지 확인 (패링용)
    private float lastJumpTime; // 마지막 점프 시간
    private bool isGrounded; // 현재 땅에 닿았는지 여부
    private float nextJumpCooldown; // 다음 점프까지 필요한 무작위 쿨타임 값

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

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
        // 1. target이 없거나, 기절 상태라면 아무것도 하지 않음
        if (player == null || isStunned)
        {
            rb.velocity = Vector2.zero;
            animator.SetFloat("speed", 0);
            return;
        }
        //Ground Check 함수를 매 프레임 호출
        CheckIfGrounded();

        //  진단용 로그 추가: isGrounded와 CanJump의 상태를 출력
        Debug.Log($"Grounded: {isGrounded} | CanJump: {CanJump()}");

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < sightRange)
        {
            // 1-1. 공격 범위 내: 공격 실행
            if (distanceToPlayer <= attackRange)
            {
                rb.velocity = Vector2.zero;
                animator.SetFloat("speed", 0);

                // 쿨타임이 지났을 때만 공격
                if (canAttack)
                {
                    animator.SetTrigger("Attack"); // 'Attack' Trigger 실행
                    canAttack = false; // 공격 시작, 쿨타임 시작
                    StartCoroutine(AttackCooldownRoutine()); // 쿨타임 관리 코루틴 시작
                }
            }
            // 1-2. 인식 범위 내: 플레이어 추격
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            // 2. 인식 범위 밖: 멈춤
            rb.velocity = Vector2.zero;
            animator.SetFloat("speed", 0);
        }

        // 점프 로직: 쿨타임이 지났고, 땅에 닿아있을 때만 점프 가능
        if (CanJump() && isGrounded)
        {
            Jump();
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
        animator.SetTrigger("Jump"); 

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

    // === 애니메이션 이벤트로 호출되는 함수 (플레이어에게 피해를 줌) ===
    public void DealDamageToPlayer()
    {
        if (player == null || isStunned) return;
        if (attackCollider == null)
        {
            Debug.LogError("Attack Collider가 Inspector에 연결되지 않았습니다!");
            return;
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
                    playerHealth.TakeDamage(attackDamage, this.gameObject);
                    Debug.Log("플레이어가 피해를 입었습니다! (적 AI)");
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

    // === 스턴 관련 함수 (패링 성공 시 PlayerHealth가 호출) ===
    public void StunEnemy(float duration)
    {
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        animator.SetTrigger("Hurt"); // 피격 애니메이션(Hurt)을 스턴 모션으로 재활용

        // 지정된 시간(duration)만큼 대기
        yield return new WaitForSeconds(duration);

        isStunned = false;
    }
}