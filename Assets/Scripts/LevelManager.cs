using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class StageInfo
{
    public string stageName;              // 스테이지 이름
    public Transform playerSpawnPoint;    // 플레이어 시작 위치
    public BoxCollider2D mapBoundary;     // 맵 경계
    public Transform[] enemySpawnPoints;  // 적 스폰 위치 목록
}

public class LevelManager : MonoBehaviour
{
    [Header("프리팹 (Prefabs)")]
    public GameObject playerPrefab;       // 플레이어 프리팹
    public GameObject enemyPrefab;        // 적 프리팹

    [Header("씬(Scene) 설정")]
    public Camera mainCamera;             // 메인 카메라
    public string nextSceneName;          // 다음 씬 이름

    [Header("스테이지 목록")]
    public List<StageInfo> stages;        // 스테이지 정보 리스트

    // --- Private Variables ---
    private int currentStageIndex = 0;
    private GameObject playerInstance;
    private CameraFollow cameraScript;
    // 현재 스테이지에 소환된 적들을 추적 관리하는 리스트
    private List<GameObject> currentEnemies = new List<GameObject>();

    void Start()
    {
        // 카메라 스크립트 가져오기
        if (mainCamera != null)
        {
            cameraScript = mainCamera.GetComponent<CameraFollow>();
        }

        // 첫 번째 스테이지 로드
        LoadStage(currentStageIndex);
    }

    // 특정 스테이지를 로드
    void LoadStage(int index)
    {
        if (index >= stages.Count) return;

        // 1. 이전 스테이지의 적들 정리 (청소)
        ClearOldEnemies();

        // 현재 로드할 스테이지 정보 가져오기
        StageInfo currentStage = stages[index];
        Debug.Log($"스테이지 {currentStage.stageName} 로드 시작");

        // 2. 플레이어 스폰 또는 위치 이동
        if (playerInstance == null)
        {
            // 게임 처음 시작 시 스폰
            playerInstance = Instantiate(playerPrefab, currentStage.playerSpawnPoint.position, Quaternion.identity);
        }
        else
        {
            // 다음 스테이지로 이동 시 위치만 변경
            playerInstance.transform.position = currentStage.playerSpawnPoint.position;
        }

        // 3. 카메라 타겟 및 경계 설정 업데이트
        if (cameraScript != null)
        {
            cameraScript.target = playerInstance.transform;
            cameraScript.mapBoundary = currentStage.mapBoundary;
        }

        // 4. 해당 스테이지의 적들 스폰
        if (enemyPrefab != null && currentStage.enemySpawnPoints.Length > 0)
        {
            foreach (Transform spawnPoint in currentStage.enemySpawnPoints)
            {
                // 적 생성
                GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                // 관리 리스트에 추가
                currentEnemies.Add(newEnemy);
            }
        }
    }

    // 이전 스테이지의 적들을 모두 삭제하는 함수
    void ClearOldEnemies()
    {
        foreach (GameObject enemy in currentEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        // 리스트 비우기
        currentEnemies.Clear();
    }

    // 다음 스테이지로 이동
    public void GoToNextStage()
    {
        currentStageIndex++; 

        // 아직 남은 스테이지가 있다면 로드
        if (currentStageIndex < stages.Count)
        {
            LoadStage(currentStageIndex);
        }
        // 모든 스테이지를 클리어했다면 다음 씬으로 이동
        else
        {
            Debug.Log("모든 스테이지 클리어! 다음 씬으로 이동합니다.");
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    public bool IsStageClear()
    {
        // 이미 죽은(파괴된) 적들을 리스트에서 제거
        currentEnemies.RemoveAll(enemy => enemy == null);

        // 남은 적이 0명이면 true 반환
        return currentEnemies.Count == 0;
    }
}