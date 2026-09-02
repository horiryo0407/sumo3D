using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public TextMeshProUGUI winnerText;

    void Start()
    {
        string winner = PlayerPrefs.GetString("Winner");
        winnerText.text = winner + " Win!";
    }

    void Update()
    {
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            if (gamepad.buttonEast.wasPressedThisFrame)
            {
                Retry();
            }

            if (gamepad.buttonWest.wasPressedThisFrame)
            {
                GoToTitle();
            }
        }

        // キーボード操作
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Retry();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoToTitle();
        }
    }

    public void Retry()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene("Title");
    }
}