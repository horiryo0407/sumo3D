using UnityEngine;

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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 【超強力なコントローラー入力取得】
        // システムに登録された「すべてのジョイスティック」から、1番目のスティック入力を直接取得します。
        // これにより、Input Managerでの認識番号のズレを完全に無視して1台目を動かせます。
        float h_pad = Input.GetAxisRaw("Horizontal"); // デフォルトの共通Horizontalを使用
        float v_pad = Input.GetAxisRaw("Vertical");   // デフォルトの共通Verticalを使用

        // キーボード(1P)の入力を取得
        float h_kb = 0f;
        float v_kb = 0f;
        if (Input.GetKey(KeyCode.A)) h_kb = -1f;
        if (Input.GetKey(KeyCode.D)) h_kb = 1f;
        if (Input.GetKey(KeyCode.W)) v_kb = 1f;
        if (Input.GetKey(KeyCode.S)) v_kb = -1f;

        // 両方の入力を合成
        float h = (Mathf.Abs(h_pad) > 0.1f) ? h_pad : h_kb;
        float v = (Mathf.Abs(v_pad) > 0.1f) ? v_pad : v_kb;

        moveInput = new Vector3(-h, 0f, -v).normalized; // 元のコードの反転を維持

        // 1台目の「Aボタン（ボタン0）」をどのコントローラーからでも強制検知
        // もしくは キーボードの左Shift
        bool isDashPressed = Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.LeftShift);

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
        float h_pad = Input.GetAxisRaw("Horizontal");
        float v_pad = Input.GetAxisRaw("Vertical");
        float h_kb = 0f;
        float v_kb = 0f;
        if (Input.GetKey(KeyCode.A)) h_kb = -1f;
        if (Input.GetKey(KeyCode.D)) h_kb = 1f;
        if (Input.GetKey(KeyCode.W)) v_kb = 1f;
        if (Input.GetKey(KeyCode.S)) v_kb = -1f;

        float finalH = (Mathf.Abs(h_pad) > 0.1f) ? h_pad : h_kb;
        float finalV = (Mathf.Abs(v_pad) > 0.1f) ? v_pad : v_kb;
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Enemy")
        {
            Debug.Log("敵に当たった！");
        }
    }
}