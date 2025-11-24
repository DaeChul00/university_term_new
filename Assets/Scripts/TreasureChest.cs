using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [Header("설정")]
    public GameObject itemToDrop; // 상자에서 나올 아이템 프리팹
    public Sprite openedSprite;   // 열린 상자 이미지

    private SpriteRenderer spriteRenderer;
    private bool isPlayerNear = false;
    private bool isOpen = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 플레이어 접근 감지
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNear = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNear = false;
    }

    void Update()
    {
        // 플레이어가 근처에 있고, E키를 눌렀고, 아직 안 열렸다면
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && !isOpen)
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        isOpen = true;
        Debug.Log("상자가 열렸습니다!");

        // 1. 스프라이트 변경 (열린 모습)
        if (openedSprite != null)
        {
            spriteRenderer.sprite = openedSprite;
        }

        // 2. 아이템 생성 (상자보다 약간 위쪽)
        if (itemToDrop != null)
        {
            Vector3 dropPos = transform.position + new Vector3(0, 1f, 0);
            Instantiate(itemToDrop, dropPos, Quaternion.identity);
        }
    }
}