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
        // パンチ
        if (Input.GetKeyDown(KeyCode.F))
            animator.SetTrigger("Attack");

        // 剛掌波チャージ
        if (Input.GetKeyDown(KeyCode.O) && !isGoshoCharging)
        {
            isGoshoCharging = true;
            goshoTimer = goshoChargeLimit;
            animator.SetTrigger("Gosho");

            if (chargeBall != null)
                chargeBall.SetActive(true);
        }

        // チャージ時間
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

        // 発射
        if (Input.GetKeyDown(KeyCode.P) && isGoshoCharging)
        {
            FireGoshoBeam();

            isGoshoCharging = false;

            if (chargeBall != null)
                chargeBall.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow)) animator.SetTrigger("EmoteUp");
        if (Input.GetKeyDown(KeyCode.DownArrow)) animator.SetTrigger("EmoteDown");
        if (Input.GetKeyDown(KeyCode.LeftArrow)) animator.SetTrigger("EmoteLeft");
        if (Input.GetKeyDown(KeyCode.RightArrow)) animator.SetTrigger("EmoteRight");

        float hPad = Input.GetAxisRaw("Horizontal");
        float vPad = Input.GetAxisRaw("Vertical");

        float hKb = Input.GetKey(KeyCode.A) ? -1 : Input.GetKey(KeyCode.D) ? 1 : 0;
        float vKb = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;

        float h = Mathf.Abs(hPad) > 0.1f ? hPad : hKb;
        float v = Mathf.Abs(vPad) > 0.1f ? vPad : vKb;

        moveInput = new Vector3(-h, 0, -v).normalized;

        bool dash = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.JoystickButton0);

        if (!isDashing && moveInput.magnitude > 0.1f && dash)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashDirection = moveInput;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) isDashing = false;
        }
    }

    void FireGoshoBeam()
    {
        if (goshoBeamPrefab != null && goshoPoint != null)
            Instantiate(goshoBeamPrefab, goshoPoint.position, goshoPoint.rotation);
    }

    void FixedUpdate()
    {
        float hPad = Input.GetAxisRaw("Horizontal");
        float vPad = Input.GetAxisRaw("Vertical");
        float hKb = Input.GetKey(KeyCode.A) ? -1 : Input.GetKey(KeyCode.D) ? 1 : 0;
        float vKb = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;

        float h = Mathf.Abs(hPad) > 0.1f ? hPad : hKb;
        float v = Mathf.Abs(vPad) > 0.1f ? vPad : vKb;

        animator.SetFloat("Speed", Mathf.Abs(h) + Mathf.Abs(v));

        if (isDashing)
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
        else
            rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Enemy")
            Debug.Log("敵に当たった！");
    }
}
