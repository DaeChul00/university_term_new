using UnityEngine;
using System.Collections.Generic;

public class TreasureChest : MonoBehaviour
{
    // === 아이템 드랍 설정 ===
    // 무작위로 드랍될 수 있는 아이템들의 목록입니다.
    public List<ItemData> possibleDrops = new List<ItemData>();
    public KeyCode openKey = KeyCode.E; // 상자를 열어 아이템을 획득할 키

    // === 내부 상태 변수 ===
    private bool isPlayerInRange = false;
    private bool hasBeenOpened = false; // 이미 열린 상자인지 확인
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 플레이어가 범위 내에 있고, 상자 열림 키를 눌렀으며, 아직 열리지 않은 상자일 때
        if (isPlayerInRange && Input.GetKeyDown(openKey) && !hasBeenOpened)
        {
            AttemptToOpen();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            if (!hasBeenOpened)
            {
                Debug.Log("상자를 열어 아이템을 획득할 수 있습니다. [" + openKey + "] 키를 누르세요.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    // 상자 열기 및 획득 로직
    private void AttemptToOpen()
    {
        // 1. 랜덤 아이템 선택
        if (possibleDrops.Count == 0)
        {
            Debug.LogWarning("TreasureChest에 드랍할 아이템이 설정되어 있지 않습니다!");
            return;
        }

        int randomIndex = Random.Range(0, possibleDrops.Count);
        ItemData droppedItem = possibleDrops[randomIndex];

        if (droppedItem != null)
        {
            // 상자 열림 애니메이션
            if (animator != null)
            {
                animator.SetTrigger("Open");
            }

            // 2. 아이템 효과 즉시 적용 (인벤토리 로직 완전 제거)
            droppedItem.Use(FindObjectOfType<PlayerController>().gameObject); // Player 오브젝트를 찾아 효과 적용

            hasBeenOpened = true;
            Debug.Log("상자에서 " + droppedItem.itemName + "을(를) 획득하고 즉시 사용했습니다.");

            // 상자 오브젝트를 파괴합니다.
            Destroy(gameObject, 0.5f); // 0.5초 딜레이 후 파괴하여 애니메이션을 볼 시간을 줍니다.
        }
    }
}