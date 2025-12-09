using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Custom/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType; // 획득 로직에서 사용되진 않지만, 데이터 구분을 위해 유지합니다.

    [Header("Stats")]
    public float healthIncreaseAmount; // 체력 회복량
    public float attackIncreaseAmount; // 공격력 증가량
    public float maxHealthIncreaseAmount; // 최대 체력 증가량

    // 아이템의 타입을 정의합니다.
    public enum ItemType
    {
        Consumable, // 소모성 (예: 포션)
        Equipment,  // 장비 (예: 검)
        Passive     // 영구 효과 (예: 능력치 증가)
    }

    // 아이템 사용 시 효과를 적용하는 핵심 함수
    // TreasureChest에서 플레이어를 대상으로 호출됩니다.
    public void Use(GameObject target)
    {
        Debug.Log(itemName + " 아이템 효과를 적용합니다.");

        PlayerController playerController = target.GetComponent<PlayerController>();
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            if (healthIncreaseAmount > 0)
            {
                playerHealth.Heal(healthIncreaseAmount);
            }

            if (maxHealthIncreaseAmount > 0)
            {
                playerHealth.IncreaseMaxHealth(maxHealthIncreaseAmount);
            }
        }

        if (playerController != null)
        {
            if (attackIncreaseAmount > 0)
            {
                playerController.IncreaseAttack(attackIncreaseAmount);
            }
        }
    }
}