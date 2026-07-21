using UnityEngine;
using UnityEngine.InputSystem; // 新しいInput Systemをインポート

public class PlayerController : MonoBehaviour
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

    // 剛掌波
    private bool isGoshoCharging = false;
    public float goshoChargeLimit = 3f;
    private float goshoTimer = 0f;

    public GameObject goshoBeamPrefab;
    public Transform goshoPoint;
    public GameObject chargeBall;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        if (chargeBall != null)
            chargeBall.SetActive(false);
    }

    void Update()
    {
    
        // 各種アクション入力のチェック（コントローラー1P vs キーボード）
        
        bool punchPressed = Input.GetKeyDown(KeyCode.F);
        bool goshoChargePressed = Input.GetKeyDown(KeyCode.O);
        bool goshoFirePressed = Input.GetKeyDown(KeyCode.P);
        bool dashPressed = Input.GetKeyDown(KeyCode.LeftShift);

        bool emoteUpPressed = Input.GetKeyDown(KeyCode.UpArrow);
        bool emoteDownPressed = Input.GetKeyDown(KeyCode.DownArrow);
        bool emoteLeftPressed = Input.GetKeyDown(KeyCode.LeftArrow);
        bool emoteRightPressed = Input.GetKeyDown(KeyCode.RightArrow);

        float hPad = 0f;
        float vPad = 0f;

        // コントローラー1（Gamepad.all[0]）の処理
        if (Gamepad.all.Count > 0)
        {
            var gamepad = Gamepad.all[0];

            // 左スティック移動
            Vector2 stick = gamepad.leftStick.ReadValue();
            hPad = stick.x;
            vPad = stick.y;

            // ボタン入力の追加判定
            if (gamepad.buttonEast.wasPressedThisFrame) punchPressed = true;        // Bボタン：パンチ
            if (gamepad.buttonNorth.wasPressedThisFrame) goshoChargePressed = true; // Yボタン：剛掌波チャージ
            if (gamepad.buttonWest.wasPressedThisFrame) goshoFirePressed = true;   // Xボタン：剛掌波発射
            if (gamepad.buttonSouth.wasPressedThisFrame) dashPressed = true;        // Aボタン：ダッシュ

            // 十字キー：エモート
            if (gamepad.dpad.up.wasPressedThisFrame) emoteUpPressed = true;
            if (gamepad.dpad.down.wasPressedThisFrame) emoteDownPressed = true;
            if (gamepad.dpad.left.wasPressedThisFrame) emoteLeftPressed = true;
            if (gamepad.dpad.right.wasPressedThisFrame) emoteRightPressed = true;
        }

        //キーボード（WASD）移動
        float hKb = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
        float vKb = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;

        // コントローラー優先（なければキーボード）
        float h = Mathf.Abs(hPad) > 0.1f ? hPad : hKb;
        float v = Mathf.Abs(vPad) > 0.1f ? vPad : vKb;

        moveInput = new Vector3(-h, 0f, -v).normalized;

       
        // アクション実行ロジック
        
        // パンチ
        if (punchPressed && animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 剛掌波チャージ開始
        if (goshoChargePressed && !isGoshoCharging)
        {
            isGoshoCharging = true;
            goshoTimer = goshoChargeLimit;

            if (animator != null)
                animator.SetTrigger("Gosho");

            if (chargeBall != null)
                chargeBall.SetActive(true);
        }

        // チャージタイマー管理
        if (isGoshoCharging)
        {
            goshoTimer -= Time.deltaTime;

            if (goshoTimer <= 0f)
            {
                isGoshoCharging = false;

                if (chargeBall != null)
                    chargeBall.SetActive(false);
            }
        }

        // 剛掌波発射
        if (goshoFirePressed && isGoshoCharging)
        {
            FireGoshoBeam();
            isGoshoCharging = false;

            if (chargeBall != null)
                chargeBall.SetActive(false);
        }

        // エモート
        if (animator != null)
        {
            if (emoteUpPressed) animator.SetTrigger("EmoteUp");
            if (emoteDownPressed) animator.SetTrigger("EmoteDown");
            if (emoteLeftPressed) animator.SetTrigger("EmoteLeft");
            if (emoteRightPressed) animator.SetTrigger("EmoteRight");
        }

        // ダッシュ開始
        if (!isDashing && moveInput.magnitude > 0.1f && dashPressed)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashDirection = moveInput;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f) isDashing = false;
        }
    }

    void FireGoshoBeam()
    {
        if (goshoBeamPrefab != null && goshoPoint != null)
            Instantiate(goshoBeamPrefab, goshoPoint.position, goshoPoint.rotation);
    }

    void FixedUpdate()
    {
        float hPad = 0f;
        float vPad = 0f;

        if (Gamepad.all.Count > 0)
        {
            Vector2 stick = Gamepad.all[0].leftStick.ReadValue();
            hPad = stick.x;
            vPad = stick.y;
        }

        float hKb = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
        float vKb = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;

        float h = Mathf.Abs(hPad) > 0.1f ? hPad : hKb;
        float v = Mathf.Abs(vPad) > 0.1f ? vPad : vKb;

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(h) + Mathf.Abs(v));
        }

        if (isDashing)
        {
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Enemy")
            Debug.Log("敵に当たった！");
    }
}