using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 포함합니다.

public class PlayerController : MonoBehaviour
{
    // === Public Variables (Inspector에서 설정) ===
    public float moveSpeed = 4f;     // 이동 속도
    public float jumpForce = 5f;     // 점프 힘
    public Collider2D attackCollider; // 공격 판정을 위한 콜라이더
    public BoxCollider2D mapBoundary; // 맵 경계 콜라이더

    // === Private Variables ===
    private Rigidbody2D rb;          // Rigidbody2D 컴포넌트
    private Animator animator;       // Animator 컴포넌트
    private bool isGrounded;         // 땅에 닿았는지 확인 플래그
    private bool isAttacking = false; // 공격 중인지 확인하는 플래그 (이중 공격 방지)
    private PlayerHealth playerHealth;

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
    }

    void Update()
    {
        HandleMovement();

        // === 점프 ===
        if (Input.GetKeyDown(KeyCode.C) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // Animator의 'IsJumping' 파라미터 업데이트
        animator.SetBool("IsJumping", !isGrounded);

        // === 공격 ===
        // isAttacking이 false일 때만 공격 허용
        if (Input.GetKeyDown(KeyCode.X) && !isAttacking)
        {
            animator.SetTrigger("Attack");
            isAttacking = true; // 공격 시작과 동시에 true로 설정하여 추가 입력 차단
        }

        // === 맵 경계 내에서 플레이어 위치 제한 ===
        if (mapBoundary != null)
        {
            float minX = mapBoundary.bounds.min.x;
            float maxX = mapBoundary.bounds.max.x;

            Vector3 currentPos = transform.position;
            currentPos.x = Mathf.Clamp(currentPos.x, minX, maxX);
            transform.position = currentPos;
        }

        if (Input.GetKeyDown(KeyCode.Q) && !isAttacking && !playerHealth.IsParrying()) // Q 키를 패링 입력으로 사용
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
                        enemyHealth.TakeDamage(20f);
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
}