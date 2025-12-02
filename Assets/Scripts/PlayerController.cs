using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 포함합니다.

public class PlayerController : MonoBehaviour
{
    // === Public Variables (Inspector에서 설정) ===
    public float moveSpeed = 4f;     // 이동 속도
    public float jumpForce = 5f;     // 점프 힘
    public Collider2D attackCollider; // 공격 판정을 위한 콜라이더
    public BoxCollider2D mapBoundary; // 맵 경계 콜라이더
    public float attackDamage = 10f; // 기본 공격력


    // === Private Variables ===
    private Rigidbody2D rb;          // Rigidbody2D 컴포넌트
    private Animator animator;       // Animator 컴포넌트
    private bool isGrounded;         // 땅에 닿았는지 확인 플래그
    private bool isAttacking = false; // 공격 중인지 확인하는 플래그 (이중 공격 방지)
    private PlayerHealth playerHealth;
    private bool canMove = true; // 이동 가능 여부 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerHealth = GetComponent<PlayerHealth>();

        // 시작 시 공격 콜라이더 비활성화
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }

        // 시작 시 UI에 초기 공격력 표시
        if (UIManager.instance != null)
            UIManager.instance.UpdateAttack(attackDamage);
    }

    void Update()
    {
        // canMove가 true일 때만 이동 및 점프 로직 실행
        if (canMove)
        {
            // 이동
            HandleMovement();

            // === 점프 ===
            if (Input.GetKeyDown(KeyCode.C) && isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        // Animator의 'IsJumping' 파라미터 업데이트
        animator.SetBool("IsJumping", !isGrounded);

        // === 공격 ===
        if (Input.GetKeyDown(KeyCode.X) && !isAttacking)
        {
            animator.SetTrigger("Attack");
            isAttacking = true;
        }

        // === 패링 ===
        if (Input.GetKeyDown(KeyCode.Q) && !isAttacking && !playerHealth.IsParrying())
        {
            if (playerHealth != null)
            {
                playerHealth.AttemptParry();
            }
        }

    }

    // === 캐릭터 이동 및 방향 전환을 처리하는 함수 ===
    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

        animator.SetFloat("speed", Mathf.Abs(horizontalInput));

        // 방향 전환 (Scale을 이용한 스프라이트 반전)
        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    // === 바닥 체크 ===
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    // === 애니메이션 이벤트로 호출되는 함수들 (공격 판정) ===

    // 공격 애니메이션 시작 프레임에서 호출 (공격 판정 시작)
    public void StartAttack()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = true; // 콜라이더 활성화

            // 데미지 로직
            Collider2D[] hitObjects = Physics2D.OverlapBoxAll(attackCollider.bounds.center, attackCollider.bounds.size, 0);

            foreach (Collider2D hit in hitObjects)
            {
                if (hit.CompareTag("Enemy"))
                {
                    EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(attackDamage);
                        Debug.Log("적이 피해를 입었습니다!");
                    }
                }
            }
        }
    }

    // 공격 애니메이션 종료 프레임에서 호출 (공격 판정 종료)
    public void EndAttack()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = false; // 콜라이더 비활성화
        }

        // isAttacking을 false로 설정하여 다음 공격을 허용
        isAttacking = false;
        Debug.Log("EndAttack 호출. 다음 공격 가능.");
    }

    // 아이템이 호출하는 공격력 증가 함수
    public void IncreaseAttack(float amount)
    {
        attackDamage += amount;
        Debug.Log("현재 플레이어 공격력: " + attackDamage);

        // UI 매니저가 있다면 업데이트
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateAttack(attackDamage);
        }
    }

    // PlayerHealth가 넉백 시간을 알리기 위해 호출하는 함수
    public void ApplyKnockbackTime(float duration)
    {
        StartCoroutine(KnockbackRoutine(duration));
    }

    // 넉백 중 이동을 차단하는 코루틴
    private IEnumerator KnockbackRoutine(float duration)
    {
        canMove = false; // 이동 차단
        yield return new WaitForSeconds(duration);
        canMove = true;  // 이동 허용
    }

}