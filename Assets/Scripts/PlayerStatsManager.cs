using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    // 싱글톤 패턴으로 씬 이동 시에도 유지되게 합니다.
    public static PlayerStatsManager Instance;

    // 씬 이동 시 유지할 핵심 스탯 값들
    [Header("Persistent Stats")]
    public float currentHealth;
    public float maxHealth;
    public float attackDamage;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 이 오브젝트와 스탯 데이터를 씬 이동 시 파괴하지 않도록 설정
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            // 이미 존재하는 인스턴스 외에는 파괴
            Destroy(gameObject);
        }
    }

    // 플레이어가 처음 시작할 때 호출되어 초기값을 저장합니다.
    public void InitializeStats(float baseMaxHealth, float baseAttack)
    {
        if (maxHealth == 0) // 처음 초기화될 때만 실행
        {
            maxHealth = baseMaxHealth;
            currentHealth = baseMaxHealth;
            attackDamage = baseAttack;
            Debug.Log("PlayerStatsManager: 초기 스탯 저장 완료.");
        }
        // 씬 이동 후 재접속 시에는 이미 저장된 값을 사용합니다.
    }
    public void ResetStats()
    {
        // 현재 체력을 최대 체력으로 되돌립니다.
        currentHealth = maxHealth;

        Debug.Log("플레이어 스탯이 초기화되었습니다. 현재 체력: " + currentHealth);
    }

}