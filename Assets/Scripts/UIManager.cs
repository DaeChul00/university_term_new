using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    // GameManager처럼 싱글톤으로 만듭니다.
    public static UIManager instance;

    [Header("UI Panels")]
    public GameObject statusPanel;

    [Header("상태창 텍스트")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI attackText;

    private bool isStatusPanelActive = false; // 현재 상태창이 켜져 있는지 확인

    void Start()
    {
        if (statusPanel != null)
        {
            statusPanel.SetActive(false); // 처음에 비활성화
            isStatusPanelActive = false;
        }
    }

    void Update()
    {
        // 'Tab' 키 (또는 'I'키 등 원하는 키)를 눌렀을 때
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleStatusPanel();
        }
    }

    public void ToggleStatusPanel()
    {
        // 현재 상태를 반전시킵니다 (true -> false, false -> true)
        isStatusPanelActive = !isStatusPanelActive;

        if (statusPanel != null)
        {
            // 상태창 패널을 활성화/비활성화
            statusPanel.SetActive(isStatusPanelActive);
        }
    }


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // 체력 텍스트를 업데이트하는 함수
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString("F0") + " / " + maxHealth.ToString("F0");
        }
    }

    // 공격력 텍스트를 업데이트하는 함수
    public void UpdateAttack(float attackDamage)
    {
        if (attackText != null)
        {
            attackText.text = attackDamage.ToString("F0");
        }
    }
}