using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerStretch : MonoBehaviour
{
    [Header("計分與 UI 設定")]
    [SerializeField] Text scoreText;
    [SerializeField] GameObject perfectUI;
    [SerializeField] Text finalScoreText;
    [SerializeField] GameObject gameOverUI;

    [Header("音效設定")]
    [SerializeField] AudioSource bgmAudio;      
    [SerializeField] AudioSource gameOverAudio; 

    [Header("相機震動與律動設定")]
    [SerializeField] float idleShakeSpeed = 1.0f;     // 晃動的速度（風吹的頻率）
    [SerializeField] float idleShakeAmount = 0.1f;    // 晃動的幅度（風吹的大小）
    [SerializeField] float clickShakeIntensity = 0.05f; 
    [SerializeField] float clickShakeDuration = 0.1f;   
    [SerializeField] float explodeShakeIntensity = 0.4f; 
    [SerializeField] float explodeShakeDuration = 0.6f;  

    [Header("點擊伸縮設定")]
    [SerializeField] float lengthPerClick = 0.6f; 
    [SerializeField] float rotateSpeed = 300f;    
    [SerializeField] float timeToAutoFall = 0.8f;   
    [SerializeField] float shrinkSpeed = 15f;    
    
    [Header("限制設定")]
    [SerializeField] int maxClicks = 15; 
    private int currentClickCount = 0;   

    [Header("再來一次設定")]
    [SerializeField] float doubleClickThreshold = 0.5f; 
    private float lastClickTime = 0f;

    [Header("判定區域設定")]
    [SerializeField] float detectionHeight = 15f;    
    [SerializeField] float detectionWidthOffset = 0.5f; 
    [SerializeField] float detectionYOffset = 0f;

    [Header("攝影機設定")]
    [SerializeField] Transform mainCamera; 
    [SerializeField] float camFollowSpeed = 5f;

    private SpriteRenderer sr;
    private Transform dogSprite; 
    private bool isStretching = false;
    private bool isRotating = false;
    private bool isMoving = false; 
    private bool isGameOver = false;
    private float targetRotation = -90f;
    private Vector3 initialCamPos;
    private float fallTimer = 0f; 
    private int score = 0;
    
    // 用於律動計算的偏移值
    private Vector3 noiseOffset = Vector3.zero;

    void Start() {
        dogSprite = transform.GetChild(0); 
        sr = dogSprite.GetComponent<SpriteRenderer>();
        if (mainCamera != null) initialCamPos = mainCamera.position;
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (perfectUI != null) perfectUI.SetActive(false);

        if (bgmAudio != null) bgmAudio.Play();
        if (gameOverAudio != null) gameOverAudio.Stop();

        UpdateScoreDisplay();
        UpdateDogColor();
    }

    void Update() {
        if (isGameOver) {
            HandleGameOverInput();
            return;
        }

        // --- 持續性的風吹律動 ---
        UpdateCameraIdleShake();

        if (isMoving) return;

        if (Input.GetKeyDown(KeyCode.Space) && !isRotating) {
            currentClickCount++;
            if (currentClickCount > maxClicks) {
                Explode();
                return;
            }
            if (!isStretching) {
                PrepareAtEdge();
                isStretching = true;
            }
            AddLength();
            fallTimer = timeToAutoFall;
            StartCoroutine(ShakeCamera(clickShakeDuration, clickShakeIntensity));
        }

        if (isStretching && !isRotating) {
            fallTimer -= Time.deltaTime;
            UpdateCameraPosition();

            if (fallTimer <= 0) {
                isStretching = false;
                isRotating = true;
            }
        }

        if (isRotating) {
            float angle = Mathf.MoveTowardsAngle(transform.localEulerAngles.z, targetRotation, rotateSpeed * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0, 0, angle);
            
            if (Mathf.Abs(angle - targetRotation) < 0.1f) {
                isRotating = false;
                CheckLanding();
            }
        }
    }

    // --- 新增：模擬風吹的律動 ---
    void UpdateCameraIdleShake() {
        if (mainCamera == null) return;

        // 使用 Perlin Noise 產生平滑的隨機偏移
        float seed = Time.time * idleShakeSpeed;
        float x = (Mathf.PerlinNoise(seed, 0f) - 0.5f) * 2f * idleShakeAmount;
        float y = (Mathf.PerlinNoise(0f, seed) - 0.5f) * 2f * idleShakeAmount;
        
        noiseOffset = new Vector3(x, y, 0);
        
        // 這裡不直接改 Camera 位置，而是加上偏移，以免干擾攝影機跟隨
        mainCamera.position += noiseOffset * Time.deltaTime * 60f; 
    }

    void UpdateCameraPosition() {
        if (mainCamera != null && dogSprite.localScale.y > 5f) {
            float targetCamY = initialCamPos.y + (dogSprite.localScale.y - 5f) * 0.5f;
            Vector3 targetPos = new Vector3(mainCamera.position.x, targetCamY, mainCamera.position.z);
            mainCamera.position = Vector3.Lerp(mainCamera.position, targetPos, camFollowSpeed * Time.deltaTime);
        }
    }

    IEnumerator ShakeCamera(float duration, float intensity) {
        if (mainCamera == null) yield break;
        float elapsed = 0.0f;
        while (elapsed < duration) {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            mainCamera.position += new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // --- 其餘邏輯不變 (AddLength, CheckLanding, Explode, ResetDog...) ---
    void AddLength() {
        dogSprite.localScale += new Vector3(0, lengthPerClick, 0);
        dogSprite.localPosition = new Vector3(0, dogSprite.localScale.y / 2f, 0);
        UpdateDogColor();
    }

    void HandleGameOverInput() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            float currentTime = Time.time;
            if (currentTime - lastClickTime < doubleClickThreshold) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            lastClickTime = currentTime;
        }
    }

    void PrepareAtEdge() {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 2f);
        if (hit.collider != null && hit.collider.CompareTag("Platform")) {
            float edgeX = hit.collider.bounds.max.x;
            transform.position = new Vector3(edgeX, transform.position.y, transform.position.z);
        }
    }

    void CheckLanding() {
        float stickLength = dogSprite.localScale.y;
        Collider2D currentPlatform = null;
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, 2f);
        if (groundHit.collider != null) currentPlatform = groundHit.collider;

        Vector2 checkCenter = (Vector2)transform.position + new Vector2(stickLength / 2f, detectionYOffset);
        Vector2 checkSize = new Vector2(stickLength + detectionWidthOffset, detectionHeight); 

        Collider2D[] results = Physics2D.OverlapBoxAll(checkCenter, checkSize, 0f);
        Collider2D targetPlatform = null;

        foreach (var col in results) {
            if (col.CompareTag("Platform") && col != currentPlatform) {
                targetPlatform = col;
                break;
            }
        }

        if (targetPlatform != null) {
            float stickEndX = transform.position.x + stickLength;
            float platformCenterX = targetPlatform.transform.position.x;
            if (Mathf.Abs(stickEndX - platformCenterX) < 0.35f) {
                score += 2;
                StartCoroutine(ShowPerfectUI());
            } else {
                score += 1;
            }
            UpdateScoreDisplay();
            StartCoroutine(ShrinkMoveRoutine(new Vector3(platformCenterX, targetPlatform.bounds.max.y, transform.position.z)));
        } else {
            Explode();
        }
    }

    IEnumerator ShrinkMoveRoutine(Vector3 targetPos) {
        isMoving = true;
        while (Vector3.Distance(transform.position, targetPos) > 0.05f) {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, shrinkSpeed * Time.deltaTime);
            float dist = Vector3.Distance(transform.position, targetPos);
            dogSprite.localScale = new Vector3(1, dist + 1f, 1);
            dogSprite.localPosition = new Vector3(0, dogSprite.localScale.y / 2f, 0);
            
            if (mainCamera != null) {
                Vector3 camTarget = new Vector3(transform.position.x, initialCamPos.y, initialCamPos.z);
                mainCamera.position = Vector3.Lerp(mainCamera.position, camTarget, Time.deltaTime * camFollowSpeed);
            }
            yield return null;
        }
        transform.position = targetPos;
        ResetDog();
        isMoving = false;
        FindObjectOfType<PlatformGenerator>()?.SpawnNewPlatform();
    }

    void UpdateScoreDisplay() { if (scoreText != null) scoreText.text = score.ToString(); }
    IEnumerator ShowPerfectUI() { if (perfectUI != null) { perfectUI.SetActive(true); yield return new WaitForSeconds(0.6f); perfectUI.SetActive(false); } }

    void Explode() {
        isGameOver = true;
        isStretching = false;
        isRotating = false;
        if (bgmAudio != null) bgmAudio.Stop();      
        if (gameOverAudio != null) gameOverAudio.Play(); 
        StartCoroutine(ShakeCamera(explodeShakeDuration, explodeShakeIntensity));
        if (sr != null) sr.color = Color.black;
        if (gameOverUI != null) {
            gameOverUI.SetActive(true);
            if (finalScoreText != null) finalScoreText.text = score.ToString();
        }
    }

    void ResetDog() {
        dogSprite.localScale = Vector3.one;
        dogSprite.localPosition = new Vector3(0, 0.5f, 0);
        transform.localEulerAngles = Vector3.zero;
        UpdateDogColor();
        fallTimer = 0;
        currentClickCount = 0; 
    }

    void UpdateDogColor() {
        if (currentClickCount <= 5) sr.color = new Color(0.4f, 1f, 0.8f);
        else if (currentClickCount <= 10) sr.color = new Color(1f, 0.8f, 0f);
        else sr.color = Color.red;
    }
}