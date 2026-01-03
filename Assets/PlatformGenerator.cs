using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    [Header("物件設定")]
    [SerializeField] GameObject platformPrefab;
    [SerializeField] Transform lastPlatform; 

    [Header("隨機寬度設定")]
    [Range(0.5f, 5f)] [SerializeField] float minWidth = 1.0f;
    [Range(0.5f, 5f)] [SerializeField] float maxWidth = 3.0f;

    [Header("隨機距離設定")]
    [SerializeField] float minDistance = 4f;
    [SerializeField] float maxDistance = 8f;

    [Header("隨機高度設定")]
    [SerializeField] float minHeight = -4f;
    [SerializeField] float maxHeight = -1f;

    // --- 關鍵修正 1：加入 Start 讓遊戲開始就先生成平台 ---
    void Start() {
        // 如果你忘了在 Inspector 拖入 lastPlatform，腳本會自動抓取場景中現有的平台
        if (lastPlatform == null) {
            GameObject firstPlatform = GameObject.FindWithTag("Platform");
            if (firstPlatform != null) lastPlatform = firstPlatform.transform;
        }

        // 剛開始先預生 3 塊，這樣你才看得到「後面有東西」
        if (lastPlatform != null && platformPrefab != null) {
            for (int i = 0; i < 3; i++) {
                SpawnNewPlatform();
            }
        }
    }

    public void SpawnNewPlatform() {
        // --- 關鍵修正 2：防呆機制 ---
        if (platformPrefab == null || lastPlatform == null) {
            Debug.LogError("PlatformGenerator 缺少 Prefab 或起始平台參考！");
            return;
        }

        float randomDist = Random.Range(minDistance, maxDistance);
        float randomY = Random.Range(minHeight, maxHeight);
        
        // 根據最後一塊平台的位置來計算新位置
        Vector3 newPos = new Vector3(lastPlatform.position.x + randomDist, randomY, 0);

        GameObject nextPlatform = Instantiate(platformPrefab, newPos, Quaternion.identity);
        
        float randomWidth = Random.Range(minWidth, maxWidth);
        nextPlatform.transform.localScale = new Vector3(randomWidth, nextPlatform.transform.localScale.y, 1);

        // 更新參考點
        lastPlatform = nextPlatform.transform;
        Debug.Log("成功生成一個新平台！目前最後位置在：" + lastPlatform.position.x);
    }
}