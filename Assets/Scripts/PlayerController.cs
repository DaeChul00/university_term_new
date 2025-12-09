using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // === Public Variables (Inspector에서 설정) ===
    public float moveSpeed = 4f;
    public float jumpForce = 5f;
    public Collider2D attackCollider;
    public BoxCollider2D mapBoundary;


    // === Private Variables ===
    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool isAttacking = false;
    private PlayerHealth playerHealth;
    private bool canMove = true;

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

        // UI에 저장된 공격력 표시
        if (UIManager.instance != null && PlayerStatsManager.Instance != null)
        {
            // 매니저의 공격력을 가져와서 UI를 업데이트합니다.
            UIManager.instance.UpdateAttack(PlayerStatsManager.Instance.attackDamage);
        }
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
        if (Input.GetKeyDown(KeyCode.Q) && !isAttacking && playerHealth != null && !playerHealth.IsParrying())
        {
            playerHealth.AttemptParry();
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
                    if (enemyHealth != null && PlayerStatsManager.Instance != null)
                    {
                        // ⭐️ PlayerStatsManager의 공격력을 사용합니다. ⭐️
                        enemyHealth.TakeDamage(PlayerStatsManager.Instance.attackDamage);
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

        isAttacking = false;
        Debug.Log("EndAttack 호출. 다음 공격 가능.");
    }

    // 아이템이 호출하는 공격력 증가 함수
    public void IncreaseAttack(float amount)
    {
        if (PlayerStatsManager.Instance == null) return;

        // PlayerStatsManager의 공격력을 업데이트합니다.
        PlayerStatsManager.Instance.attackDamage += amount;

        // UI 매니저가 있다면 업데이트
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateAttack(PlayerStatsManager.Instance.attackDamage);
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
        canMove = true;  // 이동 허용
    }

}