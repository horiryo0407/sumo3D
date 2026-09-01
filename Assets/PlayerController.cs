using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

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

    public PunchHitbox punchHitbox;

    private bool isActionPlaying = false;
    private float actionTimer = 0f;

    public float punchDuration = 0.4f;


    // =========================
    // エモート設定
    // =========================

    private bool isEmoting = false;
    public float emoteDuration = 1.5f; // エモートの持続時間（アニメーション長さに合わせて調整）
    private float emoteTimer = 0f;


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


        // クールダウン表示
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

        if (isActionPlaying)
        {
            actionTimer -= Time.deltaTime;

            if (actionTimer <= 0f)
            {
                isActionPlaying = false;
            }
        }


        // =========================
        // エモート解除タイマー
        // =========================

        if (isEmoting)
        {
            emoteTimer -= Time.deltaTime;

            if (emoteTimer <= 0f)
            {
                isEmoting = false;
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
        // 入力判定用の変数初期化
        // =========================

        float h = 0f;
        float v = 0f;

        bool isDashPressed = false;
        bool punchPressed = false;
        bool goshoChargePressed = false;
        bool goshoFirePressed = false;

        // エモート用フラグ
        bool emoteUpPressed = false;
        bool emoteDownPressed = false;
        bool emoteLeftPressed = false;
        bool emoteRightPressed = false;


        // =========================
        // 1P コントローラー (Xbox等)
        // =========================

        if (Gamepad.all.Count > 0)
        {
            var gamepad = Gamepad.all[0];

            Vector2 stick = gamepad.leftStick.ReadValue();

            h = stick.x;
            v = stick.y;


            // Aボタン → ダッシュ
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                isDashPressed = true;
            }


            // Xボタン → パンチ
            if (gamepad.buttonWest.wasPressedThisFrame)
            {
                punchPressed = true;
            }


            // Yボタン → 剛掌波ため (Xbox)
            if (gamepad.buttonNorth.wasPressedThisFrame)
            {
                goshoChargePressed = true;
            }


            // Bボタン → 剛掌波発射
            if (gamepad.buttonEast.wasPressedThisFrame)
            {
                goshoFirePressed = true;
            }

            // 十字キー → エモート
            if (gamepad.dpad.up.wasPressedThisFrame)
            {
                emoteUpPressed = true;
            }

            if (gamepad.dpad.down.wasPressedThisFrame)
            {
                emoteDownPressed = true;
            }

            if (gamepad.dpad.left.wasPressedThisFrame)
            {
                emoteLeftPressed = true;
            }

            if (gamepad.dpad.right.wasPressedThisFrame)
            {
                emoteRightPressed = true;
            }
        }



        // =========================
        // キーボード移動
        // =========================

        float h_kb = 0f;
        float v_kb = 0f;


        if (Input.GetKey(KeyCode.A))
        {
            h_kb = -1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            h_kb = 1f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            v_kb = 1f;
        }

        if (Input.GetKey(KeyCode.S))
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
        // キーボード アクション・エモート
        // =========================

        // Space → ダッシュ
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isDashPressed = true;
        }

        // F → パンチ
        if (Input.GetKeyDown(KeyCode.F))
        {
            punchPressed = true;
        }

        // G → 剛掌波ため (キーボード)
        if (Input.GetKeyDown(KeyCode.G))
        {
            goshoChargePressed = true;
        }

        // H → 剛掌波発射
        if (Input.GetKeyDown(KeyCode.H))
        {
            goshoFirePressed = true;
        }

        // 矢印キー → エモート
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            emoteUpPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            emoteDownPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            emoteLeftPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            emoteRightPressed = true;
        }



        // =========================
        // パンチ
        // =========================

        if (punchPressed)
        {
            Punch();
        }



        // =========================
        // 剛掌波ため（Xbox Yボタン / キーボード Gキー 共通）
        // =========================

        if (goshoChargePressed)
        {
            StartGosho();
        }



        // =========================
        // 剛掌波発射
        // =========================

        if (goshoFirePressed)
        {
            FireGosho();
        }



        // =========================
        // エモート実行
        // =========================

        if (animator != null && !isActionPlaying && !isGoshoCharging && !isEmoting)
        {
            if (emoteUpPressed)
            {
                TriggerEmote("EmoteUp");
            }
            else if (emoteDownPressed)
            {
                TriggerEmote("EmoteDown");
            }
            else if (emoteLeftPressed)
            {
                TriggerEmote("EmoteLeft");
            }
            else if (emoteRightPressed)
            {
                TriggerEmote("EmoteRight");
            }
        }



        // =========================
        // 剛掌波チャージ中
        // =========================

        if (isGoshoCharging)
        {
            goshoTimer -= Time.deltaTime;


            if (goshoTimer <= 0f)
            {
                CancelGosho();
            }
        }



        // =========================
        // ダッシュ
        // =========================

        if (!isDashing &&
            !isActionPlaying &&
            !isEmoting &&
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
    // エモート共通処理
    // =========================

    void TriggerEmote(string triggerName)
    {
        animator.SetTrigger(triggerName);
        isEmoting = true;
        emoteTimer = emoteDuration;
    }



    // =========================
    // パンチ
    // =========================

    void Punch()
    {
        if (animator == null || isGoshoCharging || isActionPlaying || isEmoting)
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
        // クールダウン中・チャージ中・アクション中・エモート中は剛掌波ため不可
        if (goshoCooldownTimer > 0f || isGoshoCharging || isActionPlaying || isEmoting)
        {
            return;
        }

        // アクション開始
        isActionPlaying = true;

        actionTimer = goshoChargeLimit;


        // チャージ開始
        isGoshoCharging = true;

        goshoTimer = goshoChargeLimit;



        // モーション
        if (animator != null)
        {
            animator.SetTrigger("Gosho");
        }


        // ため玉
        if (chargeBall != null)
        {
            chargeBall.SetActive(true);
        }


        // ため音
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
        if (!isGoshoCharging)
        {
            return;
        }


        // ビーム発射
        FireGoshoBeam();


        // チャージ終了
        isGoshoCharging = false;

        isActionPlaying = false;


        // クールダウン開始
        goshoCooldownTimer = goshoCooldown;


        // ため玉消す
        if (chargeBall != null)
        {
            chargeBall.SetActive(false);
        }
    }



    // =========================
    // 剛掌波キャンセル
    // =========================

    void CancelGosho()
    {
        isGoshoCharging = false;

        isActionPlaying = false;


        if (chargeBall != null)
        {
            chargeBall.SetActive(false);
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

        if (isGoshoCharging)
        {
            goshoCooldownText.text =
                "剛掌波：チャージ中";
        }
        else if (goshoCooldownTimer > 0f)
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
        // アクション中またはエモート中は移動しない
        if (isActionPlaying || isEmoting)
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



        float h = 0f;
        float v = 0f;


        // コントローラー
        if (Gamepad.all.Count > 0)
        {
            Vector2 stick =
                Gamepad.all[0]
                .leftStick
                .ReadValue();

            h = stick.x;
            v = stick.y;
        }



        // キーボード
        float h_kb = 0f;
        float v_kb = 0f;


        if (Input.GetKey(KeyCode.A))
        {
            h_kb = -1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            h_kb = 1f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            v_kb = 1f;
        }

        if (Input.GetKey(KeyCode.S))
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



        // ダッシュ
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



        // 通常移動
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
        isEmoting = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Enemy")
        {
            Debug.Log("敵に当たった！");
        }
    }
}