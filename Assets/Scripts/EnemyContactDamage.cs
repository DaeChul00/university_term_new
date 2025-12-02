using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    public float contactDamage = 5f;    // 접촉 시 줄 피해량
    public float knockbackForce = 600f; // 넉백 힘 (200f ~ 400f 사이로 튜닝 권장)

    // 적의 메인 콜라이더가 다른 오브젝트와 물리적으로 충돌하는 순간 호출
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 충돌한 대상이 플레이어인지 확인
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

            // 플레이어가 무적이 아닐 때만 넉백과 피해를 적용
            // PlayerHealth.IsInvincible() 함수가 반드시 public으로 선언되어 있어야 합니다.
            if (playerHealth != null && playerRb != null && !playerHealth.IsInvincible())
            {
                // 데미지 적용
                playerHealth.TakeDamage(contactDamage, gameObject);


                // 수평 방향 (적이 플레이어의 어느 쪽에 있는지 판단)
                float horizontalDirection = (collision.transform.position.x > transform.position.x) ? 1 : -1;

                // 넉백 속도 (이 값이 넉백의 세기를 결정합니다. 5f ~ 10f 사이로 튜닝)
                float knockbackSpeedX = 8f;
                float knockbackSpeedY = 5f; // 위로 살짝 뜨게 할 고정 속도

                // 넉백 적용 전, 플레이어의 현재 속도를 0으로 초기화 (관성 제거)
                playerRb.velocity = Vector2.zero;

                // 강제로 넉백 속도 설정
                playerRb.velocity = new Vector2(
                    horizontalDirection * knockbackSpeedX,
                    knockbackSpeedY
                );
            }
        }
    }
}