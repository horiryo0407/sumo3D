using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    public GameObject enemy;
    public float fallY = -1f;

    public GameObject stage;
    public float shrinkSpeed = 0.1f;
    public float minSize = 1f;

    void Update()
    {
        // ステージを縮小
        if (stage.transform.localScale.x > minSize)
        {
            float newSize = stage.transform.localScale.x - shrinkSpeed * Time.deltaTime;
            stage.transform.localScale = new Vector3(newSize, stage.transform.localScale.y, newSize);
        }

        if (player.transform.position.y < fallY)
        {
            PlayerPrefs.SetString("Winner", "2P");
            SceneManager.LoadScene("GameOver");
        }

        if (enemy.transform.position.y < fallY)
        {
            PlayerPrefs.SetString("Winner", "1P");
            SceneManager.LoadScene("GameOver");
        }
    }
}