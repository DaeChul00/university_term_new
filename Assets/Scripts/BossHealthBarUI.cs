using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    private Slider healthSlider;

    void Start()
    {
        // 이 스크립트가 Slider 컴포넌트에 직접 부착되었다고 가정합니다.
        healthSlider = GetComponent<Slider>();

        // 보스는 등장 전까지 체력 바를 숨깁니다.
        Hide();
    }

    // BossHealth.cs에서 호출되어 체력 바를 업데이트합니다.
    public void UpdateBar(float current, float max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }

    // BossHealth.cs가 초기화될 때 호출됩니다.
    public void Show()
    {
        gameObject.SetActive(true);
        Debug.Log("Boss Health Bar Shown.");
    }

    // BossHealth.cs의 Die() 함수에서 호출됩니다.
    public void Hide()
    {
        gameObject.SetActive(false);
        Debug.Log("Boss Health Bar Hidden.");
    }
}