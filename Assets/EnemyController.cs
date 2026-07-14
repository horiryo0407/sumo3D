using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 10f;
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;
    public float doubleTapTime = 0.3f;

    private Rigidbody rb;
    private Animator animator;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private Vector3 dashDirection;

    private float lastTapTimeUp, lastTapTimeDown, lastTapTimeLeft, lastTapTimeRight;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 子にある2PモデルのAnimatorを取得
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) { if (Time.time - lastTapTimeUp < doubleTapTime) StartDash(Vector3.back); lastTapTimeUp = Time.time; }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { if (Time.time - lastTapTimeDown < doubleTapTime) StartDash(Vector3.forward); lastTapTimeDown = Time.time; }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { if (Time.time - lastTapTimeLeft < doubleTapTime) StartDash(Vector3.right); lastTapTimeLeft = Time.time; }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { if (Time.time - lastTapTimeRight < doubleTapTime) StartDash(Vector3.left); lastTapTimeRight = Time.time; }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                isDashing = false;
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
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) h = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) h = 1f;
        if (Input.GetKey(KeyCode.UpArrow)) v = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v = -1f;

        // Animatorへ移動量を送る
        float moveAmount = Mathf.Abs(h) + Mathf.Abs(v);

        if (animator != null)
        {
            animator.SetFloat("Speed", moveAmount);
        }

        if (isDashing)
        {
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
            return;
        }

        rb.MovePosition(rb.position + new Vector3(-h, 0, -v) * speed * Time.fixedDeltaTime);
    }
}