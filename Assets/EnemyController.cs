using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyController : MonoBehaviour
{
    public float speed = 10f;
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;

    private Rigidbody rb;
    private Animator animator;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private Vector3 dashDirection;
    private Vector3 moveInput;

    // =========================
    // パンチ判定用
    // =========================
    public PunchHitbox punchHitbox;

    // =========================
    // アクション制御
    // =========================
    private bool isActionPlaying = false;
    private float actionTimer = 0f;
    public float punchDuration = 0.4f; // パンチ中の行動不能時間（モーションに合わせて調整）

    // =========================
    // 剛掌波
    // =========================
    private bool isGoshoCharging = false;
    public float goshoChargeLimit = 3f;
    private float goshoTimer = 0f;

    public GameObject goshoBeamPrefab;
    public Transform goshoPoint;
    public GameObject chargeBall;

    // =========================
    // 剛掌波SE
    // =========================
    public AudioSource audioSource;
    public AudioClip goshoChargeSE;
    public AudioClip goshoFireSE;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        if (chargeBall != null)
        {
            chargeBall.SetActive(false);
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (isActionPlaying)
        {
            actionTimer -= Time.deltaTime;
            if (actionTimer <= 0f)
            {
                isActionPlaying = false;
            }
        }

        float h = 0f;
        float v = 0f;

        bool isDashPressed = false;
        bool punchPressed = false;
        bool goshoChargePressed = false;
        bool goshoFirePressed = false;

        // --- 2Pコントローラー ---
        if (Gamepad.all.Count > 1)
        {
            var gamepad = Gamepad.all[1];

            Vector2 stick = gamepad.leftStick.ReadValue();
            h = stick.x;
            v = stick.y;

            if (gamepad.buttonSouth.wasPressedThisFrame) isDashPressed = true;        // A -> ダッシュ
            if (gamepad.buttonEast.wasPressedThisFrame) punchPressed = true;          // B -> パンチ
            if (gamepad.buttonNorth.wasPressedThisFrame) goshoChargePressed = true;  // Y -> 剛掌波ため
            if (gamepad.buttonWest.wasPressedThisFrame) goshoFirePressed = true;      // X -> 剛掌波発射
        }

        // --- キーボード移動 ---
        float h_kb = 0f;
        float v_kb = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) h_kb = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) h_kb = 1f;
        if (Input.GetKey(KeyCode.UpArrow)) v_kb = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v_kb = -1f;

        float finalH = Mathf.Abs(h) > 0.1f ? h : h_kb;
        float finalV = Mathf.Abs(v) > 0.1f ? v : v_kb;

        moveInput = new Vector3(-finalH, 0f, -finalV).normalized;

        // --- キーボード アクション ---
        if (Input.GetKeyDown(KeyCode.Space)) isDashPressed = true;
        if (Input.GetKeyDown(KeyCode.L)) punchPressed = true;
        if (Input.GetKeyDown(KeyCode.K)) goshoChargePressed = true;
        if (Input.GetKeyDown(KeyCode.J)) goshoFirePressed = true;

        // アクション実行（パンチ優先）
        if (punchPressed) Punch();
        if (goshoChargePressed) StartGosho();
        if (goshoFirePressed) FireGosho();

        if (isGoshoCharging)
        {
            goshoTimer -= Time.deltaTime;
            if (goshoTimer <= 0f)
            {
                CancelGosho();
            }
        }

        // ダッシュ開始制御（アクション実行中でない場合のみ）
        if (!isDashing && !isActionPlaying && moveInput.magnitude > 0.1f && isDashPressed)
        {
            StartDash(moveInput);
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
    }

    // =========================
    // パンチ（移動強制キャンセル対応）
    // =========================
    void Punch()
    {
        if (animator == null || isGoshoCharging) return;

        // --- 移動・ダッシュを即座に停止させる ---
        isDashing = false;
        moveInput = Vector3.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // Unity 6表記（旧バージョンなら rb.velocity = Vector3.zero;）
        }

        // アクション再生中に設定して移動入力によるポジション更新をロック
        isActionPlaying = true;
        actionTimer = punchDuration;

        animator.SetTrigger("Attack");

        if (punchHitbox != null)
        {
            punchHitbox.EnableHitbox();
            CancelInvoke(nameof(DisablePunchHitbox));
            Invoke(nameof(DisablePunchHitbox), 0.3f);
        }
    }

    void DisablePunchHitbox()
    {
        if (punchHitbox != null)
        {
            punchHitbox.DisableHitbox();
        }
    }

    // =========================
    // 剛掌波ため開始
    // =========================
    void StartGosho()
    {
        if (isGoshoCharging || isActionPlaying) return;

        isActionPlaying = true;
        actionTimer = goshoChargeLimit;
        isGoshoCharging = true;
        goshoTimer = goshoChargeLimit;

        if (animator != null) animator.SetTrigger("Gosho");
        if (chargeBall != null) chargeBall.SetActive(true);
        if (audioSource != null && goshoChargeSE != null) audioSource.PlayOneShot(goshoChargeSE);
    }

    // =========================
    // 剛掌波発射
    // =========================
    void FireGosho()
    {
        if (!isGoshoCharging) return;

        FireGoshoBeam();
        isGoshoCharging = false;
        isActionPlaying = false;

        if (chargeBall != null) chargeBall.SetActive(false);
    }

    void CancelGosho()
    {
        isGoshoCharging = false;
        isActionPlaying = false;
        if (chargeBall != null) chargeBall.SetActive(false);
    }

    void FireGoshoBeam()
    {
        if (goshoBeamPrefab != null && goshoPoint != null)
        {
            Instantiate(goshoBeamPrefab, goshoPoint.position, goshoPoint.rotation);
        }
        if (audioSource != null && goshoFireSE != null) audioSource.PlayOneShot(goshoFireSE);
    }

    void StartDash(Vector3 direction)
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashDirection = direction;
    }

    void FixedUpdate()
    {
        // アクション実行中は物理移動をロック
        if (isActionPlaying)
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
            return;
        }

        float h = 0f;
        float v = 0f;

        if (Gamepad.all.Count > 1)
        {
            Vector2 stick = Gamepad.all[1].leftStick.ReadValue();
            h = stick.x;
            v = stick.y;
        }

        float h_kb = 0f;
        float v_kb = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) h_kb = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) h_kb = 1f;
        if (Input.GetKey(KeyCode.UpArrow)) v_kb = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v_kb = -1f;

        float finalH = Mathf.Abs(h) > 0.1f ? h : h_kb;
        float finalV = Mathf.Abs(v) > 0.1f ? v : v_kb;

        float moveAmount = Mathf.Abs(finalH) + Mathf.Abs(finalV);

        if (animator != null)
        {
            animator.SetFloat("Speed", moveAmount);
        }

        if (isDashing)
        {
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
            return;
        }

        rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
    }

    public void EndAction()
    {
        isActionPlaying = false;
    }
}