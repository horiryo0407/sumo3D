using UnityEngine;
using UnityEngine.InputSystem;

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


    // アクション制御
    private bool isActionPlaying = false;
    private float actionTimer = 0f;


    // 剛掌波
    private bool isGoshoCharging = false;
    public float goshoChargeLimit = 3f;
    private float goshoTimer = 0f;

    public GameObject goshoBeamPrefab;
    public Transform goshoPoint;
    public GameObject chargeBall;


    // 剛掌波SE
    public AudioSource audioSource;
    public AudioClip goshoChargeSE;
    public AudioClip goshoFireSE;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        if (chargeBall != null)
            chargeBall.SetActive(false);

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
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



        if (Gamepad.all.Count > 0)
        {
            var gamepad = Gamepad.all[0];

            Vector2 stick = gamepad.leftStick.ReadValue();

            hPad = stick.x;
            vPad = stick.y;


            if (gamepad.buttonEast.wasPressedThisFrame)
                punchPressed = true;

            if (gamepad.buttonNorth.wasPressedThisFrame)
                goshoChargePressed = true;

            if (gamepad.buttonWest.wasPressedThisFrame)
                goshoFirePressed = true;

            if (gamepad.buttonSouth.wasPressedThisFrame)
                dashPressed = true;


            if (gamepad.dpad.up.wasPressedThisFrame)
                emoteUpPressed = true;

            if (gamepad.dpad.down.wasPressedThisFrame)
                emoteDownPressed = true;

            if (gamepad.dpad.left.wasPressedThisFrame)
                emoteLeftPressed = true;

            if (gamepad.dpad.right.wasPressedThisFrame)
                emoteRightPressed = true;
        }



        float hKb = Input.GetKey(KeyCode.A) ? -1f :
                    Input.GetKey(KeyCode.D) ? 1f : 0f;


        float vKb = Input.GetKey(KeyCode.W) ? 1f :
                    Input.GetKey(KeyCode.S) ? -1f : 0f;



        float h = Mathf.Abs(hPad) > 0.1f ? hPad : hKb;
        float v = Mathf.Abs(vPad) > 0.1f ? vPad : vKb;


        moveInput = new Vector3(-h, 0f, -v).normalized;



        // パンチ
        if (punchPressed && animator != null && !isActionPlaying)
        {
            animator.SetTrigger("Attack");
        }



        // 剛掌波チャージ
        if (goshoChargePressed && !isGoshoCharging && !isActionPlaying)
        {
            isActionPlaying = true;
            actionTimer = 3f;

            isGoshoCharging = true;
            goshoTimer = goshoChargeLimit;


            if (animator != null)
                animator.SetTrigger("Gosho");


            if (chargeBall != null)
                chargeBall.SetActive(true);


            if (audioSource != null && goshoChargeSE != null)
            {
                audioSource.PlayOneShot(goshoChargeSE);
            }
        }



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
            isActionPlaying = false;


            if (chargeBall != null)
                chargeBall.SetActive(false);
        }

        // エモート
        if (animator != null && !isActionPlaying)
        {
            if (emoteUpPressed)
            {
                animator.SetTrigger("EmoteUp");
                isActionPlaying = true;
                actionTimer = 2f;
            }

            if (emoteDownPressed)
            {
                animator.SetTrigger("EmoteDown");
                isActionPlaying = true;
                actionTimer = 2f;
            }

            if (emoteLeftPressed)
            {
                animator.SetTrigger("EmoteLeft");
                isActionPlaying = true;
                actionTimer = 2f;
            }

            if (emoteRightPressed)
            {
                animator.SetTrigger("EmoteRight");
                isActionPlaying = true;
                actionTimer = 2f;
            }
        }



        // ダッシュ
        if (!isDashing && moveInput.magnitude > 0.1f && dashPressed)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashDirection = moveInput;
        }


        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
                isDashing = false;
        }
    }



    void FireGoshoBeam()
    {
        if (goshoBeamPrefab != null && goshoPoint != null)
        {
            Instantiate(
                goshoBeamPrefab,
                goshoPoint.position,
                goshoPoint.rotation
            );
        }


        // 発射音
        if (audioSource != null && goshoFireSE != null)
        {
            audioSource.PlayOneShot(goshoFireSE);
        }
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


        float hKb = Input.GetKey(KeyCode.A) ? -1f :
                    Input.GetKey(KeyCode.D) ? 1f : 0f;


        float vKb = Input.GetKey(KeyCode.W) ? 1f :
                    Input.GetKey(KeyCode.S) ? -1f : 0f;



        float h = Mathf.Abs(hPad) > 0.1f ? hPad : hKb;
        float v = Mathf.Abs(vPad) > 0.1f ? vPad : vKb;



        if (animator != null)
        {
            animator.SetFloat(
                "Speed",
                Mathf.Abs(h) + Mathf.Abs(v)
            );
        }



        if (isDashing)
        {
            rb.MovePosition(
                rb.position +
                dashDirection * dashSpeed * Time.fixedDeltaTime
            );
        }
        else
        {
            rb.MovePosition(
                rb.position +
                moveInput * speed * Time.fixedDeltaTime
            );
        }
    }



    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Enemy")
        {
            Debug.Log("敵に当たった！");
        }
    }



    // Animation Event用（後で使える）
    public void EndAction()
    {
        isActionPlaying = false;
    }
}