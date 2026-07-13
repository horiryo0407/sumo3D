using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 10f;
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;
    public float doubleTapTime = 0.3f;

    private Rigidbody rb;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private Vector3 dashDirection;

    private float lastTapTimeW, lastTapTimeS, lastTapTimeA, lastTapTimeD;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) { if (Time.time - lastTapTimeW < doubleTapTime) StartDash(Vector3.back); lastTapTimeW = Time.time; }
        if (Input.GetKeyDown(KeyCode.S)) { if (Time.time - lastTapTimeS < doubleTapTime) StartDash(Vector3.forward); lastTapTimeS = Time.time; }
        if (Input.GetKeyDown(KeyCode.A)) { if (Time.time - lastTapTimeA < doubleTapTime) StartDash(Vector3.right); lastTapTimeA = Time.time; }
        if (Input.GetKeyDown(KeyCode.D)) { if (Time.time - lastTapTimeD < doubleTapTime) StartDash(Vector3.left); lastTapTimeD = Time.time; }

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
        if (isDashing)
        {
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
            return;　
        }

        float h = 0f;
        float v = 0f;
        if (Input.GetKey(KeyCode.A)) h = -1f;
        if (Input.GetKey(KeyCode.D)) h = 1f;
        if (Input.GetKey(KeyCode.W)) v = 1f;
        if (Input.GetKey(KeyCode.S)) v = -1f;

        rb.MovePosition(rb.position + new Vector3(-h, 0, -v) * speed * Time.fixedDeltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Enemy")
        {
            Debug.Log("敵に当たった！"); 
        }
    }
}
