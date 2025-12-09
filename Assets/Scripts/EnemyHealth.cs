using UnityEngine;
using UnityEngine.UI;
using System.Collections; // 이제 이 using 문과 코루틴은 필요 없습니다.

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public Slider healthSlider; // 체력 바 Slider를 연결할 변수

    private float currentHealth;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyAI enemyAI;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyAI = GetComponent<EnemyAI>();

        // 체력 바 초기 설정
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        
        if (enemyAI != null && enemyAI.IsExecutingSpecialAttack())
        {
            currentHealth -= damage;

            // UI 업데이트
            if (healthSlider != null) healthSlider.value = currentHealth;

            if (currentHealth <= 0) Die();
            return; // 함수 종료 (Hurt 애니메이션 및 깜빡임 방지)
        }

        currentHealth -= damage;

        // 체력 바 업데이트
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        StartCoroutine(FlickerSprite(0.1f, 3));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlickerSprite(float duration, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // 스프라이트를 끄고 켜서 깜빡이는 효과를 냅니다.
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(duration);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(duration);
        }
    }

    void Die()
    {
        Debug.Log("적이 죽었습니다.");
        Destroy(gameObject);
    }
}