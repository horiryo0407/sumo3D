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
    // アクション制御
    // =========================

    private bool isActionPlaying = false;
    private float actionTimer = 0f;


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
        // 入力
        // =========================

        float h = 0f;
        float v = 0f;

        bool isDashPressed = false;
        bool punchPressed = false;
        bool goshoChargePressed = false;
        bool goshoFirePressed = false;



        // =========================
        // 2Pコントローラー
        // =========================

        if (Gamepad.all.Count > 1)
        {
            var gamepad = Gamepad.all[1];

            // 左スティック
            Vector2 stick = gamepad.leftStick.ReadValue();

            h = stick.x;
            v = stick.y;


            // A → ダッシュ
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                isDashPressed = true;
            }


            // B → パンチ
            if (gamepad.buttonEast.wasPressedThisFrame)
            {
                punchPressed = true;
            }


            // Y → 剛掌波ため
            if (gamepad.buttonNorth.wasPressedThisFrame)
            {
                goshoChargePressed = true;
            }


            // X → 剛掌波発射
            if (gamepad.buttonWest.wasPressedThisFrame)
            {
                goshoFirePressed = true;
            }
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



        // コントローラー優先
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


        // K → 剛掌波ため
        if (Input.GetKeyDown(KeyCode.K))
        {
            goshoChargePressed = true;
        }


        // J → 剛掌波発射
        if (Input.GetKeyDown(KeyCode.J))
        {
            goshoFirePressed = true;
        }



        // =========================
        // パンチ
        // =========================

        if (punchPressed)
        {
            Punch();
        }



        // =========================
        // 剛掌波ため
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
        // ダッシュ開始
        // =========================

        if (!isDashing &&
            moveInput.magnitude > 0.1f &&
            isDashPressed)
        {
            StartDash(moveInput);
        }



        // =========================
        // ダッシュ中
        // =========================

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
    // パンチ
    // =========================

    void Punch()
    {
        // 他のアクション中はパンチしない
        if (animator == null)
            return;

        if (isActionPlaying)
            return;

        if (isGoshoCharging)
            return;


        animator.SetTrigger("Attack");
    }



    // =========================
    // 剛掌波ため開始
    // =========================

    void StartGosho()
    {
        // すでにため中なら無視
        if (isGoshoCharging)
            return;


        // 他のアクション中なら無視
        if (isActionPlaying)
            return;


        isActionPlaying = true;

        actionTimer = goshoChargeLimit;


        isGoshoCharging = true;

        goshoTimer = goshoChargeLimit;



        // 剛掌波アニメーション
        if (animator != null)
        {
            animator.SetTrigger("Gosho");
        }



        // ため玉表示
        if (chargeBall != null)
        {
            chargeBall.SetActive(true);
        }



        // ため音
        if (audioSource != null &&
            goshoChargeSE != null)
        {
            audioSource.PlayOneShot(goshoChargeSE);
        }
    }



    // =========================
    // 剛掌波発射
    // =========================

    void FireGosho()
    {
        // ためていないなら発射しない
        if (!isGoshoCharging)
            return;


        // ビーム発射
        FireGoshoBeam();


        // 状態解除
        isGoshoCharging = false;

        isActionPlaying = false;


        // ため玉非表示
        if (chargeBall != null)
        {
            chargeBall.SetActive(false);
        }
    }



    // =========================
    // チャージキャンセル
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
            audioSource.PlayOneShot(goshoFireSE);
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
        // アニメーション用移動量
        // =========================

        float h = 0f;
        float v = 0f;


        // 2Pコントローラー
        if (Gamepad.all.Count > 1)
        {
            Vector2 stick =
                Gamepad.all[1].leftStick.ReadValue();

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



        // コントローラー優先
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



        // Animator
        if (animator != null)
        {
            animator.SetFloat(
                "Speed",
                moveAmount
            );
        }



        // =========================
        // 移動
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


        rb.MovePosition(
            rb.position +
            moveInput *
            speed *
            Time.fixedDeltaTime
        );
    }



    // =========================
    // Animation Event用
    // =========================

    public void EndAction()
    {
        isActionPlaying = false;
    }
}