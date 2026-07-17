using UnityEngine;

public class GoshoBeam : MonoBehaviour
{
    public float speed = 30f;
    public float lifeTime = 2f;


    void Start()
    {
        Destroy(gameObject, lifeTime);
    }


    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }
}