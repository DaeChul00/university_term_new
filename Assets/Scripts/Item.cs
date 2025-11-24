using UnityEngine;

public class Item : MonoBehaviour
{
    // 아이템 획득 시 실행될 가상 함수 (자식들이 덮어쓰기 가능)
    public virtual void OnPickup(GameObject player)
    {
        Debug.Log("아이템 획득: " + gameObject.name);
        Destroy(gameObject); // 획득 후 오브젝트 파괴
    }

    // 플레이어와 닿았을 때 감지
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPickup(other.gameObject);
        }
    }
}