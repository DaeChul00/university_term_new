using UnityEngine;
using UnityEngine.UI; // UI 사용을 위해 추가
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    // === Public Variables ===
    public float maxHealth = 100f;
    public float parryWindow = 0.2f; // 패링 가능한 짧은 시간
    public Slider healthSlider; // 플레이어 체력 바 Slider

    // === Private Variables ===
    private float currentHealth;
    private Animator animator;
    private bool isParrying = false;  // 현재 패링 시도 중인지
    private bool isInvincible = false; // 피격 후 무적 상태인지

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        // 체력 바 초기화
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    // === 패링 상태 확인 함수 ===
    // PlayerController가 "이미 패링 중인가?"를 묻기 위해 호출합니다.
    public bool IsParrying()
    {
        return isParrying;
    }

    // === 피격 판정 함수 (핵심 수정) ===
    // 적이 공격할 때 "누가" 공격했는지(attacker)를 받도록 수정합니다.
    public void TakeDamage(float damage, GameObject attacker)
    {
        // 1. 무적 상태 체크
        if (isInvincible) return;

        // 2. 패링 상태 체크 (핵심 로직)
        if (isParrying)
        {
            // --- 패링 성공 ---
            Debug.Log("패링 성공! 적 스턴 효과 발생!");

            // ⭐️ 추가된 부분: 공격한 적(attacker)을 스턴시킵니다.
            if (attacker != null)
            {
                EnemyAI enemy = attacker.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.StunEnemy(2.0f); // 2초간 스턴
                }
            }

            // 패링 성공 시에도 짧은 무적 시간 부여 (선택 사항)
            StartCoroutine(BecomeTemporarilyInvincible(0.5f));

            return; // 패링 성공 시 피해를 입지 않고 함수 종료
        }

        // --- 패링 실패 / 일반 피격 ---
        currentHealth -= damage;
        Debug.Log("일반 피격! 남은 체력: " + currentHealth);

        // 체력 바 업데이트
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        // 피격 애니메이션 및 무적 코루틴 실행
        animator.SetTrigger("Hurt");
        StartCoroutine(BecomeTemporarilyInvincible(1.0f)); // 1초간 무적 상태 부여

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("플레이어가 사망했습니다. 게임 오버!");

        // GameManager에 게임 오버를 알림
        if (GameManager.instance != null)
        {
            GameManager.instance.GameOver();
        }

        // 플레이어 오브젝트를 파괴하는 대신 비활성화
        gameObject.SetActive(false);
    }

    // 무적 상태를 관리하는 코루틴
    private IEnumerator BecomeTemporarilyInvincible(float duration)
    {
        isInvincible = true;

        yield return new WaitForSeconds(duration);

        isInvincible = false;
    }

    // PlayerController에서 호출: 패링 입력이 들어왔을 때
    public void AttemptParry()
    {
        // 패링 애니메이션 트리거 (Animator에 "Parry" Trigger 필요)
        animator.SetTrigger("Parry");
        isParrying = true;

        // 패링 가능 시간(parryWindow)이 지나면 isParrying을 false로 리셋
        StartCoroutine(ResetParryState());
    }

    private IEnumerator ResetParryState()
    {
        // 지정된 짧은 시간 동안만 패링이 가능하도록 대기
        yield return new WaitForSeconds(parryWindow);

        isParrying = false;
    }
}