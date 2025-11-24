using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요합니다.

public class GameManager : MonoBehaviour
{
    // 다른 스크립트에서 쉽게 접근할 수 있도록 하는 싱글톤 인스턴스
    public static GameManager instance;

    // Inspector 창에서 연결할 게임 오버 UI 패널
    public GameObject gameOverPanel;

    void Awake()
    {
        // 싱글톤 패턴 설정
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 게임 오버를 처리하는 함수
    public void GameOver()
    {
        Debug.Log("게임 오버!");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // 게임 오버 패널을 화면에 표시
        }
        Time.timeScale = 0f; // 게임 시간을 멈춤
    }

    // 재시작 버튼에 연결할 함수
    public void RestartStage()
    {
        Time.timeScale = 1f; // 게임 시간을 다시 원래대로 되돌림
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 현재 씬을 다시 로드
    }
}