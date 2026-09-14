using UnityEngine;

public class SRPGCameraController : MonoBehaviour
{
    [Header("移動・ズーム速度")]
    public float dragSpeed = 1.5f;
    public float zoomSpeed = 5.0f;
    public float smoothTime = 0.15f; // 値が小さいほどピタッと止まり、大きいほど滑らかになります

    [Header("マップの移動限界 (X / Z)")]
    public float minX = 0f;
    public float maxX = 32f;
    public float minZ = 0f;
    public float maxZ = 32f;

    [Header("高さ制限 (Y)")]
    public float minY = 1.0f; // 1より下に行かないように制限
    public float maxY = 20.0f;

    private Vector3 dragOrigin;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;

    void Start()
    {
        // 起動時の初期位置をターゲットにする
        targetPosition = transform.position;
    }

    void Update()
    {
        // 1. マウスドラッグによる移動計算（右クリックドラッグを想定、左なら0に変更）
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 mouseDelta = Input.mousePosition - dragOrigin;

            // ★ひし形対策：カメラの正面（Forward）と右（Right）の向きを取得する
            // Y軸の影響を無視して平面（X, Z）のベクトルにする
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            // マウスの移動方向とカメラの向きを同期させる（上におろすと正面、横で右に動く）
            Vector3 moveDir = (camRight * -mouseDelta.x + camForward * -mouseDelta.y) * (dragSpeed * 0.01f);
            
            targetPosition += moveDir;
            dragOrigin = Input.mousePosition;
        }

        // 2. マウスホイールによる高さ（ズーム）計算
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // カメラの向いている方向に進退させる
            Vector3 zoomDir = Camera.main.transform.forward * scroll * zoomSpeed;
            targetPosition += zoomDir;
        }

        // 3. 各種制限（クランプ）
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY); // Y=1以下にならない制限

        // 4. SmoothDampによる滑らかな移動（端に行くときも滑らかに減速します）
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
}
