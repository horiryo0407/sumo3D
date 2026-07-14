using UnityEngine;
using UnityEngine.InputSystem; // 新しいInput Systemをインポート

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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        float h = 0f;
        float v = 0f;
        bool isDashPressed = false;

        // PCにゲームパッドが「2台以上」接続されている場合
        if (Gamepad.all.Count > 1)
        {
            // 確実に「2台目（インデックス1）」のコントローラーを取得
            var gamepad = Gamepad.all[1];
            Vector2 stick = gamepad.leftStick.ReadValue();
            h = stick.x;
            v = stick.y;

            // 2台目のAボタン (buttonSouth) が押されたか判定
            isDashPressed = gamepad.buttonSouth.wasPressedThisFrame;
        }

        // キーボード（矢印キー）の入力
        float h_kb = 0f;
        float v_kb = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) h_kb = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) h_kb = 1f;
        if (Input.GetKey(KeyCode.UpArrow)) v_kb = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v_kb = -1f;

        // コントローラーが動いていればそちらを優先、なければキーボード
        float finalH = (Mathf.Abs(h) > 0.1f) ? h : h_kb;
        float finalV = (Mathf.Abs(v) > 0.1f) ? v : v_kb;

        moveInput = new Vector3(-finalH, 0f, -finalV).normalized; // 元の反転を維持

        // スペースキーでもダッシュ可能
        if (Input.GetKeyDown(KeyCode.Space)) isDashPressed = true;

        if (!isDashing && moveInput.magnitude > 0.1f && isDashPressed)
        {
            StartDash(moveInput);
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f) isDashing = false;
        }
    }

    void StartDash(Vector3 direction)
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashDirection = direction;
    }

    void FixedUpdate()
    {
        // アニメーション用の移動量
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

        float finalH = (Mathf.Abs(h) > 0.1f) ? h : h_kb;
        float finalV = (Mathf.Abs(v) > 0.1f) ? v : v_kb;
        float moveAmount = Mathf.Abs(finalH) + Mathf.Abs(finalV);

        if (animator != null)
        {
            animator.SetFloat("Speed", moveAmount);
        }

        if (isDashing)
        {
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);//
            return;
        }

        rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
    }
}