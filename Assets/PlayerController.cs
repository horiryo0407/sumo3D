using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // =========================
    // 移動・ダッシュ設定
    // =========================
    public float speed = 5f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;

    private Rigidbody rb;
    private Animator animator;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private Vector3 dashDirection;
    private Vector3 moveInput;

    // =========================
    // 攻撃・Hitbox設定
    // =========================
    public PunchHitbox punchHitbox; // Hierarchy上のpunchHitboxをセット

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
    // SE
    // =========================
    public AudioSource audioSource;
    public AudioClip goshoChargeSE;
    public AudioClip goshoFireSE;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        // 最初はため玉を非表示
        if (chargeBall != null)
        {
            chargeBall.SetActive(false);
        }

        // AudioSourceがなければ自動追加
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // アクション解除タイマー
        if (isActionPlaying)
        {
            actionTimer -= Time.deltaTime;
            if (actionTimer <= 0f)
            {
                isActionPlaying = false;
            }
        }

        // 入力処理
        float h = 0f;
        float v = 0f;
        bool isDashPressed = false;
        bool punchPressed = false;
        bool goshoChargePressed = false;
        bool goshoFirePressed = false;

        // --- 1P コントローラー（Gamepad 0） ---
        if (Gamepad.all.Count > 0)
        {
            var gamepad = Gamepad.all[0];
            Vector2 stick = gamepad.leftStick.ReadValue();
            h = stick.x;
            v = stick.y;

            if (gamepad.buttonSouth.wasPressedThisFrame) isDashPressed = true;        // A
            if (gamepad.buttonWest.wasPressedThisFrame) punchPressed = true;          // X (パンチ)
            if (gamepad.buttonNorth.wasPressedThisFrame) goshoChargePressed = true;  // Y
            if (gamepad.buttonEast.wasPressedThisFrame) goshoFirePressed = true;      // B
        }

        // --- キーボード移動（WASD） ---
        float h_kb = 0f;
        float v_kb = 0f;

        if (Input.GetKey(KeyCode.A)) h_kb = -1f;
        if (Input.GetKey(KeyCode.D)) h_kb = 1f;
        if (Input.GetKey(KeyCode.W)) v_kb = 1f;
        if (Input.GetKey(KeyCode.S)) v_kb = -1f;

        // 入力の優先度調整
        float finalH = Mathf.Abs(h) > 0.1f ? h : h_kb;
        float finalV = Mathf.Abs(v) > 0.1f ? v : v_kb;

        // カメラの向きに合わせて上下左右の反転を修正（マイナスを付与）
        moveInput = new Vector3(-finalH, 0f, -finalV).normalized;

        // --- キーボード アクション ---
        if (Input.GetKeyDown(KeyCode.Space)) isDashPressed = true; // スペース: ダッシュ
        if (Input.GetKeyDown(KeyCode.F)) punchPressed = true;      // F: パンチ
        if (Input.GetKeyDown(KeyCode.G)) goshoChargePressed = true;// G: 剛掌波ため
        if (Input.GetKeyDown(KeyCode.H)) goshoFirePressed = true;  // H: 剛掌波発射

        // 各アクションの呼び出し（パンチを優先処理）
        if (punchPressed) Punch();
        if (goshoChargePressed) StartGosho();
        if (goshoFirePressed) FireGosho();

        // 剛掌波チャージ制御
        if (isGoshoCharging)
        {
            goshoTimer -= Time.deltaTime;
            if (goshoTimer <= 0f) CancelGosho();
        }

        // ダッシュ開始制御（アクション実行中でない場合のみ）
        if (!isDashing && !isActionPlaying && moveInput.magnitude > 0.1f && isDashPressed)
        {
            StartDash(moveInput);
        }

        // ダッシュ継続時間制御
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f) isDashing = false;
        }
    }

    // =========================
    // パンチ処理（移動強制キャンセル対応）
    // =========================
    void Punch()
    {
        // アニメーターが無い場合や剛掌波チャージ中などは弾く
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

        // トリガーでパンチモーションを割り込み実行
        animator.SetTrigger("Attack");

        // 当たり判定をONにし、0.3秒後にOFFにする
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
    // 剛掌波・移動処理
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
        // パンチやスキルなどのアクション中は物理的な移動を行わない
        if (isActionPlaying)
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
            return;
        }

        float h = 0f;
        float v = 0f;

        if (Gamepad.all.Count > 0)
        {
            Vector2 stick = Gamepad.all[0].leftStick.ReadValue();
            h = stick.x;
            v = stick.y;
        }

        float h_kb = 0f;
        float v_kb = 0f;
        if (Input.GetKey(KeyCode.A)) h_kb = -1f;
        if (Input.GetKey(KeyCode.D)) h_kb = 1f;
        if (Input.GetKey(KeyCode.W)) v_kb = 1f;
        if (Input.GetKey(KeyCode.S)) v_kb = -1f;

        float finalH = Mathf.Abs(h) > 0.1f ? h : h_kb;
        float finalV = Mathf.Abs(v) > 0.1f ? v : v_kb;
        float moveAmount = Mathf.Abs(finalH) + Mathf.Abs(finalV);

        if (animator != null) animator.SetFloat("Speed", moveAmount);

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