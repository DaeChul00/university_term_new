using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager instance;

    [Header("UI Panels")]
    public GameObject gameOverPanel; // Inspector에서 Game Over UI Panel 연결

    [Header("Game Flow")]
    // 재시작 시 로드할 초기 맵 이름을 Inspector에서 설정할 수 있도록 변수 추가
    public string startMapName = "Grassland_Map";

    void Awake()
    {
        // Singleton 초기화 로직
        if (instance == null)
        {
            instance = this;
            // 씬이 변경되어도 파괴되지 않도록 설정
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 중복된 인스턴스가 있다면 파괴
            Destroy(gameObject);
        }

        // 게임 시작 시 패널은 비활성화 상태여야 함
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // 모든 맵에서 호출되어 게임 오버 상태를 처리하는 함수
    public void GameOver()
    {
        Debug.Log("Game Over! Displaying panel.");

        // 게임 시간 정지 (UI만 움직이게 함)
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            // 어떤 씬이든 상관없이 패널 활성화
            gameOverPanel.SetActive(true);
        }
    }

    // Restart 버튼에 연결될 함수: Grassland_Map으로 돌아감
    public void RestartGame()
    {

        if (PlayerStatsManager.Instance != null)
        {
            PlayerStatsManager.Instance.ResetStats();
        }
        // 씬 로드 전 시간을 다시 흐르게 함
        Time.timeScale = 1f;

        if (gameOverPanel != null)
        {
            // 패널 비활성화
            gameOverPanel.SetActive(false);
        }

        // 지정된 시작 맵(Grassland_Map)으로 씬 로드
        SceneManager.LoadScene(startMapName);

    }

    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");
        
        // 씬 로드 전 시간을 다시 흐르게 함 (선택 사항)
        Time.timeScale = 1f; 
        
        // Unity 에디터에서는 Play 모드만 종료
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        // 빌드된 게임에서 종료
        #else
            Application.Quit();
        #endif
    }
}