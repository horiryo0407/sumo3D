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

    // 剛掌波用
    private bool pPressed = false;
    private bool oPressed = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }


    void Update()
    {
        // パンチ
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }


        // Pキー入力
        if (Input.GetKeyDown(KeyCode.P))
        {
            pPressed = true;
        }


        // Oキー入力
        if (Input.GetKeyDown(KeyCode.O))
        {
            oPressed = true;
        }


        // P + Oで剛掌波
        if (pPressed && oPressed)
        {
            if (animator != null)
            {
                animator.SetTrigger("Gosho");
            }

            pPressed = false;
            oPressed = false;
        }


        // キーを離したらリセット
        if (Input.GetKeyUp(KeyCode.P))
        {
            pPressed = false;
        }

        if (Input.GetKeyUp(KeyCode.O))
        {
            oPressed = false;
        }



        // エモート
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (animator != null)
            {
                animator.SetTrigger("EmoteUp");
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (animator != null)
            {
                animator.SetTrigger("EmoteDown");
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (animator != null)
            {
                animator.SetTrigger("EmoteLeft");
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (animator != null)
            {
                animator.SetTrigger("EmoteRight");
            }
        }



        // コントローラー入力
        float h_pad = Input.GetAxisRaw("Horizontal");
        float v_pad = Input.GetAxisRaw("Vertical");


        // キーボード入力
        float h_kb = 0f;
        float v_kb = 0f;

        if (Input.GetKey(KeyCode.A)) h_kb = -1f;
        if (Input.GetKey(KeyCode.D)) h_kb = 1f;
        if (Input.GetKey(KeyCode.W)) v_kb = 1f;
        if (Input.GetKey(KeyCode.S)) v_kb = -1f;


        float h = (Mathf.Abs(h_pad) > 0.1f) ? h_pad : h_kb;
        float v = (Mathf.Abs(v_pad) > 0.1f) ? v_pad : v_kb;


        moveInput = new Vector3(-h, 0f, -v).normalized;



        // ダッシュ
        bool isDashPressed =
            Input.GetKeyDown(KeyCode.JoystickButton0) ||
            Input.GetKeyDown(KeyCode.LeftShift);


        if (!isDashing && moveInput.magnitude > 0.1f && isDashPressed)
        {
            StartDash(moveInput);
        }


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
            rb.MovePosition(
                rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime
            );
            return;
        }


        rb.MovePosition(
            rb.position + moveInput * speed * Time.fixedDeltaTime
        );
    }



    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Enemy")
        {
            Debug.Log("敵に当たった！");
        }
    }
}