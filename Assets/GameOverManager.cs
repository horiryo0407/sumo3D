using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public TextMeshProUGUI winnerText;

    void Start()
    {
        string winner = PlayerPrefs.GetString("Winner");
        winnerText.text = winner + " Win!";
    }

    public void Retry()
    {
        SceneManager.LoadScene("SampleScene");
    }
}