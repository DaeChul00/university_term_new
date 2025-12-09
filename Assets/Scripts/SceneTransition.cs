using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요합니다.

public class SceneTransition : MonoBehaviour
{
    [Header("Scene Settings")]
    // Inspector에서 이동할 씬의 이름을 입력합니다.
    public string sceneToLoad;

    // 플레이어가 콜라이더 영역에 들어왔을 때 호출됩니다.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌한 오브젝트가 플레이어인지 태그를 확인합니다.
        if (other.CompareTag("Player"))
        {
            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogError("Scene to Load 변수가 설정되지 않았습니다!");
                return;
            }

            Debug.Log("플레이어 감지! 씬을 로드합니다: " + sceneToLoad);

            // 지정된 씬을 로드합니다.
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}