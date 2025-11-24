using UnityEngine;

public class Portal : MonoBehaviour
{

    private bool playerIsAtPortal = false;
    private LevelManager levelManager; // LevelManager를 참조

    void Start()
    {
        // 씬에 있는 LevelManager를 자동으로 찾아서 연결
        levelManager = FindObjectOfType<LevelManager>();
    }

    // ... (OnTriggerEnter2D, OnTriggerExit2D는 이전과 동일)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerIsAtPortal = true;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerIsAtPortal = false;
    }


    void Update()
    {
        // 플레이어가 포탈에 있고 윗 방향키를 눌렀을 때
        if (playerIsAtPortal && Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (levelManager != null)
            {
                // 스테이지가 클리어되었는지 확인
                if (levelManager.IsStageClear())
                {
                    Debug.Log("스테이지 클리어! 다음으로 이동합니다.");
                    levelManager.GoToNextStage();
                }
                else
                {
                    Debug.Log("포탈이 닫혀있다. 남은 적을 모두 처치해야 한다!");
                }
            }
        }
    }
}