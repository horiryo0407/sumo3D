using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class EnemyController : MonoBehaviour
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
    // パンチ判定用
    // =========================

    public PunchHitbox punchHitbox;


    // =========================
    // アクション制御
    // =========================

    private bool isActionPlaying = false;
    private float actionTimer = 0f;

    public float punchDuration = 0.4f;


    // =========================
    // 剛掌波
    // =========================

    private bool isGoshoCharging = false;

    public float goshoChargeLimit = 3f;

    private float goshoTimer = 0f;

    public GameObject goshoBeamPrefab;
    public Transform goshoPoint;

    // チャージ玉
    public GameObject chargeBall;

    // チャージ中エフェクト
    public GameObject goshoChargeEffect;


    // =========================
    // 剛掌波 クールダウン
    // =========================

    public float goshoCooldown = 3f;

    private float goshoCooldownTimer = 0f;


    // =========================
    // 剛掌波 クールダウン表示
    // =========================

    public TextMeshProUGUI goshoCooldownText;


    // =========================
    // SE
    // =========================

    public AudioSource audioSource;

    public AudioClip goshoChargeSE;
    public AudioClip goshoFireSE;


    // =========================
    // エモート設定
    // =========================

    public float emoteDuration = 1.5f; // エモートの再生時間



    void Start()
    {
        rb = GetComponent<Rigidbody>();

        animator = GetComponentInChildren<Animator>();


        // =========================
        // 最初はチャージ玉を非表示
        // =========================

        if (chargeBall != null)
        {
            chargeBall.SetActive(false);
        }


        // =========================
        // 最初はチャージエフェクトを非表示
        // =========================

        if (goshoChargeEffect != null)
        {
            goshoChargeEffect.SetActive(false);
        }


        // =========================
        // AudioSource
        // =========================

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }


        // =========================
        // クールダウン表示
        // =========================

        if (goshoCooldownText != null)
        {
            goshoCooldownText.text = "剛掌波：使用可能";
        }
    }



    void Update()
    {
        // =========================
        // アクション解除タイマー
        // =========================

        if (isActionPlaying && !isGoshoCharging)
        {
            actionTimer -= Time.deltaTime;

            if (actionTimer <= 0f)
            {
                isActionPlaying = false;
            }
        }



        // =========================
        // 剛掌波クールダウン
        // =========================

        if (goshoCooldownTimer > 0f)
        {
            goshoCooldownTimer -= Time.deltaTime;

            if (goshoCooldownTimer < 0f)
            {
                goshoCooldownTimer = 0f;
            }
        }


        UpdateGoshoCooldownText();



        // =========================
        // 入力
        // =========================

        float h = 0f;
        float v = 0f;

        bool isDashPressed = false;
        bool punchPressed = false;
        bool goshoChargeHeld = false;
        bool goshoChargeReleased = false;

        string emoteTrigger = "";



        // =========================
        // 2P コントローラー（Xbox配置対応）
        // =========================

        if (Gamepad.all.Count > 1)
        {
            var gamepad = Gamepad.all[1];

            Vector2 stick = gamepad.leftStick.ReadValue();

            h = stick.x;
            v = stick.y;


            // A → ダッシュ
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                isDashPressed = true;
            }


            // X → パンチ（Xbox配置対応）
            if (gamepad.buttonWest.wasPressedThisFrame)
            {
                punchPressed = true;
            }


            // Y → 剛掌波ため（長押し判定）
            if (gamepad.buttonNorth.isPressed)
            {
                goshoChargeHeld = true;
            }

            if (gamepad.buttonNorth.wasReleasedThisFrame)
            {
                goshoChargeReleased = true;
            }


            // B → 剛掌波発射（単体押し時の予備）
            if (gamepad.buttonEast.wasPressedThisFrame)
            {
                goshoChargeReleased = true;
            }


            // D-Pad (十字キー) → エモートTrigger呼び出し
            if (gamepad.dpad.up.wasPressedThisFrame) emoteTrigger = "EmoteUp";
            if (gamepad.dpad.down.wasPressedThisFrame) emoteTrigger = "EmoteDown";
            if (gamepad.dpad.left.wasPressedThisFrame) emoteTrigger = "EmoteLeft";
            if (gamepad.dpad.right.wasPressedThisFrame) emoteTrigger = "EmoteRight";
        }



        // =========================
        // キーボード移動
        // =========================

        float h_kb = 0f;
        float v_kb = 0f;


        if (Input.GetKey(KeyCode.LeftArrow))
        {
            h_kb = -1f;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            h_kb = 1f;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            v_kb = 1f;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            v_kb = -1f;
        }



        // =========================
        // 入力優先
        // =========================

        float finalH =
            Mathf.Abs(h) > 0.1f
            ? h
            : h_kb;


        float finalV =
            Mathf.Abs(v) > 0.1f
            ? v
            : v_kb;


        moveInput =
            new Vector3(
                -finalH,
                0f,
                -finalV
            ).normalized;



        // =========================
        // キーボード アクション
        // =========================

        // Space → ダッシュ
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isDashPressed = true;
        }


        // L → パンチ
        if (Input.GetKeyDown(KeyCode.L))
        {
            punchPressed = true;
        }


        // K → 剛掌波ため（長押し・解放）
        if (Input.GetKey(KeyCode.K))
        {
            goshoChargeHeld = true;
        }

        if (Input.GetKeyUp(KeyCode.K))
        {
            goshoChargeReleased = true;
        }


        // J → 剛掌波即時発射
        if (Input.GetKeyDown(KeyCode.J))
        {
            goshoChargeReleased = true;
        }


        // 数字キー 1~4 → エモートTrigger呼び出し
        if (Input.GetKeyDown(KeyCode.Alpha1)) emoteTrigger = "EmoteUp";
        if (Input.GetKeyDown(KeyCode.Alpha2)) emoteTrigger = "EmoteDown";
        if (Input.GetKeyDown(KeyCode.Alpha3)) emoteTrigger = "EmoteLeft";
        if (Input.GetKeyDown(KeyCode.Alpha4)) emoteTrigger = "EmoteRight";



        // =========================
        // エモート再生
        // =========================

        if (!string.IsNullOrEmpty(emoteTrigger))
        {
            PlayEmote(emoteTrigger);
        }



        // =========================
        // パンチ
        // =========================

        if (punchPressed)
        {
            Punch();
        }



        // =========================
        // 剛掌波ため・発射ロジック
        // =========================

        if (goshoChargeHeld && !isGoshoCharging)
        {
            StartGosho();
        }

        if (isGoshoCharging)
        {
            goshoTimer -= Time.deltaTime;

            if (goshoChargeReleased || goshoTimer <= 0f)
            {
                FireGosho();
            }
        }



        // =========================
        // ダッシュ
        // =========================

        if (!isDashing &&
            !isActionPlaying &&
            moveInput.magnitude > 0.1f &&
            isDashPressed)
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
    // エモート処理
    // =========================

    void PlayEmote(string triggerName)
    {
        if (isActionPlaying || isGoshoCharging || isDashing)
        {
            return;
        }


        // 移動停止
        moveInput = Vector3.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }


        // アクション状態の開始
        isActionPlaying = true;

        actionTimer = emoteDuration;


        // Animator制御（画像のTrigger名をセット）
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }



    // =========================
    // パンチ
    // =========================

    void Punch()
    {
        if (animator == null)
        {
            return;
        }


        // 剛掌波チャージ中はパンチできない
        if (isGoshoCharging)
        {
            return;
        }


        // 他のアクション中
        if (isActionPlaying)
        {
            return;
        }


        // 移動停止
        isDashing = false;

        moveInput = Vector3.zero;


        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }


        // アクション開始
        isActionPlaying = true;

        actionTimer = punchDuration;


        // パンチモーション
        animator.SetTrigger("Attack");


        // Hitbox
        if (punchHitbox != null)
        {
            punchHitbox.EnableHitbox();

            CancelInvoke(nameof(DisablePunchHitbox));

            Invoke(
                nameof(DisablePunchHitbox),
                0.3f
            );
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
    // 剛掌波開始
    // =========================

    void StartGosho()
    {
        // クールダウン中
        if (goshoCooldownTimer > 0f)
        {
            return;
        }


        // すでにチャージ中
        if (isGoshoCharging)
        {
            return;
        }


        // 他のアクション中
        if (isActionPlaying)
        {
            return;
        }


        // =========================
        // アクション開始
        // =========================

        isActionPlaying = true;


        // =========================
        // チャージ開始
        // =========================

        isGoshoCharging = true;

        goshoTimer = goshoChargeLimit;



        // =========================
        // 剛掌波モーション
        // =========================

        if (animator != null)
        {
            animator.SetTrigger("Gosho");
        }



        // =========================
        // チャージ玉表示
        // =========================

        if (chargeBall != null)
        {
            chargeBall.SetActive(true);
        }



        // =========================
        // チャージエフェクト表示
        // =========================

        if (goshoChargeEffect != null)
        {
            goshoChargeEffect.SetActive(true);
        }



        // =========================
        // チャージSE
        // =========================

        if (audioSource != null &&
            goshoChargeSE != null)
        {
            audioSource.PlayOneShot(
                goshoChargeSE
            );
        }
    }



    // =========================
    // 剛掌波発射
    // =========================

    void FireGosho()
    {
        // チャージしていない
        if (!isGoshoCharging)
        {
            return;
        }


        // =========================
        // ビーム発射
        // =========================

        FireGoshoBeam();


        // =========================
        // チャージ終了
        // =========================

        isGoshoCharging = false;

        isActionPlaying = false;


        // =========================
        // クールダウン開始
        // =========================

        goshoCooldownTimer = goshoCooldown;


        // =========================
        // チャージ玉を消す
        // =========================

        if (chargeBall != null)
        {
            chargeBall.SetActive(false);
        }


        // =========================
        // チャージエフェクトを消す
        // =========================

        if (goshoChargeEffect != null)
        {
            goshoChargeEffect.SetActive(false);
        }
    }



    // =========================
    // ビーム生成
    // =========================

    void FireGoshoBeam()
    {
        if (goshoBeamPrefab != null &&
            goshoPoint != null)
        {
            Instantiate(
                goshoBeamPrefab,
                goshoPoint.position,
                goshoPoint.rotation
            );
        }


        // 発射音
        if (audioSource != null &&
            goshoFireSE != null)
        {
            audioSource.PlayOneShot(
                goshoFireSE
            );
        }
    }



    // =========================
    // クールダウン表示
    // =========================

    void UpdateGoshoCooldownText()
    {
        if (goshoCooldownText == null)
        {
            return;
        }


        if (goshoCooldownTimer > 0f)
        {
            goshoCooldownText.text =
                "剛掌波：あと " +
                goshoCooldownTimer.ToString("F1") +
                " 秒";
        }
        else
        {
            goshoCooldownText.text =
                "剛掌波：使用可能";
        }
    }



    // =========================
    // ダッシュ
    // =========================

    void StartDash(Vector3 direction)
    {
        isDashing = true;

        dashTimer = dashDuration;

        dashDirection = direction;
    }



    // =========================
    // FixedUpdate
    // =========================

    void FixedUpdate()
    {
        // =========================
        // アクション中は移動停止
        // =========================

        if (isActionPlaying)
        {
            if (animator != null)
            {
                animator.SetFloat(
                    "Speed",
                    0f
                );
            }

            return;
        }



        // =========================
        // 移動入力
        // =========================

        float h = 0f;
        float v = 0f;


        if (Gamepad.all.Count > 1)
        {
            Vector2 stick =
                Gamepad.all[1]
                .leftStick
                .ReadValue();

            h = stick.x;
            v = stick.y;
        }



        // キーボード
        float h_kb = 0f;
        float v_kb = 0f;


        if (Input.GetKey(KeyCode.LeftArrow))
        {
            h_kb = -1f;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            h_kb = 1f;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            v_kb = 1f;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            v_kb = -1f;
        }



        float finalH =
            Mathf.Abs(h) > 0.1f
            ? h
            : h_kb;


        float finalV =
            Mathf.Abs(v) > 0.1f
            ? v
            : v_kb;



        float moveAmount =
            Mathf.Abs(finalH) +
            Mathf.Abs(finalV);



        if (animator != null)
        {
            animator.SetFloat(
                "Speed",
                moveAmount
            );
        }



        // =========================
        // ダッシュ
        // =========================

        if (isDashing)
        {
            rb.MovePosition(
                rb.position +
                dashDirection *
                dashSpeed *
                Time.fixedDeltaTime
            );

            return;
        }



        // =========================
        // 通常移動
        // =========================

        rb.MovePosition(
            rb.position +
            moveInput *
            speed *
            Time.fixedDeltaTime
        );
    }



    // =========================
    // Animation Event
    // =========================

    public void EndAction()
    {
        isActionPlaying = false;
    }
}