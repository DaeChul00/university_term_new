using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    // === Public Variables ===
    public float maxHealth = 500f; // 보스의 기본 최대 체력

    //  UI 관리는 이 스크립트가 전담
    public BossHealthBarUI healthBarUI;
    public BossAI bossAI; // BossAI 스크립트 참조

    public string nextSceneName = "Victory_Scene";
    public float sceneLoadDelay = 3.0f;

    // === 페이즈 전환 설정 ===
    private float phaseTwoThreshold = 0.5f; // 50%
    private bool isPhaseTwoStarted = false;

    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;

        // Inspector에 연결하지 않았다면, 같은 오브젝트에서 BossAI를 찾습니다.
        if (bossAI == null)
        {
            bossAI = GetComponent<BossAI>();
        }

        // BossHealthBarUI를 사용하여 체력 바 보이기 및 초기화 
        if (healthBarUI != null)
        {
            healthBarUI.Show();
            healthBarUI.UpdateBar(currentHealth, maxHealth);
        }

        Debug.Log("Boss Spawned with health: " + maxHealth);
    }

    // 플레이어의 공격을 받아 피해를 입는 함수
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        // BossHealthBarUI를 사용하여 체력 바 업데이트
        if (healthBarUI != null)
        {
            healthBarUI.UpdateBar(currentHealth, maxHealth);
        }

        Debug.Log("Boss took " + damage + " damage. Remaining Health: " + currentHealth);

        // 1. 페이즈 전환 조건 확인
        CheckPhaseChange();

        // 2. 사망 확인
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void CheckPhaseChange()
    {
        // 50% 미만으로 떨어졌고, 아직 2페이즈가 시작되지 않았다면
        if (!isPhaseTwoStarted && currentHealth <= maxHealth * phaseTwoThreshold)
        {
            isPhaseTwoStarted = true;

            if (bossAI != null)
            {
                // BossAI에 2 페이즈로 전환하라는 명령을 전달
                bossAI.ChangePhase(2);
                Debug.Log("Boss entered Phase 2!");
            }
        }
    }

    void Die()
    {
        currentHealth = 0;
        Debug.Log("Boss Defeated! Game victory sequence here.");

        // 보스 오브젝트 비활성화 
        gameObject.SetActive(false);

        // BossHealthBarUI 숨기기 
        if (healthBarUI != null)
        {
            healthBarUI.Hide();
        }

    }
}