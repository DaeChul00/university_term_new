using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    // === Public Variables ===
    // 씬 이동 시 초기화될 기본 최대 체력 값 (매니저에 최초 전달용)
    public float baseMaxHealth = 100f;

    public float parryWindow = 0.2f;
    public Slider healthSlider;

    // === Private Variables ===
    private Animator animator;
    private bool isParrying = false;
    private bool isInvincible = false;
    private PlayerController playerController;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();

        // ⭐️ PlayerStatsManager 초기화 및 값 로드 ⭐️
        if (PlayerStatsManager.Instance != null)
        {
            // 매니저에 기본 스탯을 전달합니다. (기본 공격력 10f로 가정)
            PlayerStatsManager.Instance.InitializeStats(baseMaxHealth, 10f);
        }

        // 매니저에서 현재 스탯을 로드합니다.
        float current = PlayerStatsManager.Instance.currentHealth;
        float max = PlayerStatsManager.Instance.maxHealth;

        // UI 업데이트
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (UIManager.instance != null)
            UIManager.instance.UpdateHealth(current, max);
    }

    // === 패링, 무적 상태 함수들 유지 ===
    public bool IsParrying() { return isParrying; }
    public bool IsInvincible() { return isInvincible; }

    // === 피격 판정 함수 ===
    public void TakeDamage(float damage, GameObject attacker)
    {
        if (isInvincible || isParrying)
        {
            if (isParrying)
            {
                // --- 패링 성공 로직 ---
                Debug.Log("패링 성공! 적 스턴 효과 발생!");
                EnemyAI enemyAI = attacker.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    // ⭐️ EnemyAI에 스턴 함수 호출 (예: 1.5초 스턴) ⭐️
                    enemyAI.Stun(1.5f);
                }

                StartCoroutine(BecomeTemporarilyInvincible(0.5f));
                return;
            }
            if (isInvincible) return;
        }

        // --- 일반 피격: 매니저의 currentHealth 업데이트 ---
        PlayerStatsManager.Instance.currentHealth -= damage;
        float currentHealth = PlayerStatsManager.Instance.currentHealth;
        float maxHealth = PlayerStatsManager.Instance.maxHealth;

        Debug.Log("일반 피격! 남은 체력: " + currentHealth);

        // 체력 바 업데이트
        if (healthSlider != null) { healthSlider.value = currentHealth; }
        if (UIManager.instance != null) { UIManager.instance.UpdateHealth(currentHealth, maxHealth); }

        // PlayerController에 넉백 시간 전달
        if (playerController != null)
        {
            playerController.ApplyKnockbackTime(0.2f);
        }

        // 피격 애니메이션 및 무적 코루틴 실행
        animator.SetTrigger("Hurt");
        StartCoroutine(BecomeTemporarilyInvincible(1.0f));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (PlayerStatsManager.Instance == null) return;

        PlayerStatsManager.Instance.currentHealth += amount;

        if (PlayerStatsManager.Instance.currentHealth > PlayerStatsManager.Instance.maxHealth)
        {
            PlayerStatsManager.Instance.currentHealth = PlayerStatsManager.Instance.maxHealth;
        }

        Debug.Log("체력 " + amount + " 회복! 현재 체력: " + PlayerStatsManager.Instance.currentHealth);

        if (healthSlider != null) { healthSlider.value = PlayerStatsManager.Instance.currentHealth; }
        if (UIManager.instance != null) { UIManager.instance.UpdateHealth(PlayerStatsManager.Instance.currentHealth, PlayerStatsManager.Instance.maxHealth); }
    }

    // ⭐️ 사망 로직 (Die) ⭐️
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
        animator.SetTrigger("Parry");
        isParrying = true;
        StartCoroutine(ResetParryState());
    }

    private IEnumerator ResetParryState()
    {
        yield return new WaitForSeconds(parryWindow);
        isParrying = false;
    }

    public void IncreaseMaxHealth(float amount)
    {
        if (PlayerStatsManager.Instance == null) return;

        PlayerStatsManager.Instance.maxHealth += amount;
        PlayerStatsManager.Instance.currentHealth += amount;

        Debug.Log("최대 체력 " + amount + " 증가! 새 최대 체력: " + PlayerStatsManager.Instance.maxHealth);

        if (healthSlider != null)
        {
            healthSlider.maxValue = PlayerStatsManager.Instance.maxHealth;
            healthSlider.value = PlayerStatsManager.Instance.currentHealth;
        }

        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHealth(PlayerStatsManager.Instance.currentHealth, PlayerStatsManager.Instance.maxHealth);
        }
    }
}