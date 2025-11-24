using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // 카메라가 따라갈 대상 (플레이어)
    public Transform target; // public에서 private으로 변경

    // 카메라와 대상 간의 거리 (Z축)
    public float zOffset = -10f;

    // 맵 경계 콜라이더를 참조할 변수
    public BoxCollider2D mapBoundary;

    // Start() 함수 추가
    void Start()
    {
        // "Player" 태그를 가진 오브젝트를 찾아서 target에 연결
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    void LateUpdate()
    {
        // target이 null이 아닐 때만 실행
        if (target != null && mapBoundary != null)
        {
            // 대상의 X, Y 위치를 가져와서 카메라의 위치를 업데이트
            Vector3 newPosition = new Vector3(target.position.x, target.position.y, zOffset);

            // === 맵 경계 내에서 카메라 위치 제한 ===
            float camHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
            float camHalfHeight = Camera.main.orthographicSize;

            float minX = mapBoundary.bounds.min.x + camHalfWidth;
            float maxX = mapBoundary.bounds.max.x - camHalfWidth;
            float minY = mapBoundary.bounds.min.y + camHalfHeight;
            float maxY = mapBoundary.bounds.max.y - camHalfHeight;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

            transform.position = newPosition;
        }
    }
}