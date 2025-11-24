using UnityEngine;

public class SwordItem : Item // Item 클래스를 상속받음
{
    public float attackIncreaseAmount = 5f; // 공격력 증가량

    // Item의 OnPickup 함수를 재정의(Override)
    public override void OnPickup(GameObject player)
    {
        // 1. 플레이어 컨트롤러 찾기
        PlayerController playerController = player.GetComponent<PlayerController>();

        // 2. 공격력 증가 함수 호출
        if (playerController != null)
        {
            playerController.IncreaseAttack(attackIncreaseAmount);
            Debug.Log($"공격력이 {attackIncreaseAmount}만큼 증가했습니다!");
        }

        // 3. 부모 클래스의 기본 기능(오브젝트 파괴) 실행
        base.OnPickup(player);
    }
}