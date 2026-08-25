using UnityEngine;

public class PunchHitbox : MonoBehaviour
{
    private Collider col;

    // 吹っ飛ばしの強さ（インスペクターで調整可能）
    public float knockbackForce = 15f;

    void Start()
    {
        col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false; // 最初はオフ
        }
    }

    public void EnableHitbox()
    {
        if (col != null) col.enabled = true;
    }

    public void DisableHitbox()
    {
        if (col != null) col.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // 自分自身（自分の体やパーツ）には当たらないようにする
        if (other.transform.root == transform.root) return;

        // 当たった相手の名前判定
        if (other.name == "PlayerB" || other.name == "PlayeP" || other.transform.root.name == "PlayerB" || other.transform.root.name == "PlayeP")
        {
            Debug.Log(gameObject.name + " のパンチが " + other.name + " に当たった！");

            // ==========================================
            // 【貫通防止】相手のRigidbodyを取得して押し出す
            // ==========================================
            Rigidbody targetRb = other.GetComponentInParent<Rigidbody>();
            if (targetRb != null)
            {
                // 自分から相手への押し出し方向を計算（y軸は0にして水平に飛ばす）
                Vector3 pushDirection = (other.transform.position - transform.position).normalized;
                pushDirection.y = 0f;

                // 瞬時に衝撃を加える
                targetRb.AddForce(pushDirection * knockbackForce, ForceMode.Impulse);
            }

            // 1回のパンチで1回だけ当たるようにオフにする
            if (col != null) col.enabled = false;
        }
    }
}