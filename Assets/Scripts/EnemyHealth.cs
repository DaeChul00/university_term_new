using UnityEngine;
using UnityEngine.UI;
using System.Collections; // 이제 이 using 문과 코루틴은 필요 없습니다.

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public Slider healthSlider; // 체력 바 Slider를 연결할 변수

    private float currentHealth;
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        // 체력 바 초기 설정
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // 체력 바 업데이트
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("적이 죽었습니다.");
        Destroy(gameObject);
    }
}