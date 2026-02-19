using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static string clickedButton1Tag;
    public static string clickedButton2Tag;
    // 👉 Her yerden erişilecek Hard Mode
    public static bool HardMode =>
        PlayerPrefs.GetInt("HardMode", 0) == 1;

    public void LoadScene(string sceneName)
    {
        ButtonClicked();

        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }

    public void LoadScene2()
    {
        ButtonClicked();

        if (clickedButton1Tag == "Unlimited Game Button" && clickedButton2Tag == "4 Letter Button")
            SceneManager.LoadScene("4 Letter Unlimited Game Scene");

        else if (clickedButton1Tag == "TimeLimit Game Button" && clickedButton2Tag == "4 Letter Button")
            SceneManager.LoadScene("4 Letter TimeLimit Game Scene");

        else if (clickedButton1Tag == "Daily Game Button" && clickedButton2Tag == "4 Letter Button")
            SceneManager.LoadScene("4 Letter DailyWord Game Scene");

        else if (clickedButton1Tag == "Unlimited Game Button" && clickedButton2Tag == "5 Letter Button")
            SceneManager.LoadScene("5 Letter Unlimited Game Scene");

        else if (clickedButton1Tag == "TimeLimit Game Button" && clickedButton2Tag == "5 Letter Button")
            SceneManager.LoadScene("5 Letter TimeLimit Game Scene");

        else if (clickedButton1Tag == "Daily Game Button" && clickedButton2Tag == "5 Letter Button")
            SceneManager.LoadScene("5 Letter DailyWord Game Scene");

        else if (clickedButton1Tag == "Unlimited Game Button" && clickedButton2Tag == "6 Letter Button")
            SceneManager.LoadScene("6 Letter Unlimited Game Scene");

        else if (clickedButton1Tag == "TimeLimit Game Button" && clickedButton2Tag == "6 Letter Button")
            SceneManager.LoadScene("6 Letter TimeLimit Game Scene");

        else if (clickedButton1Tag == "Daily Game Button" && clickedButton2Tag == "6 Letter Button")
            SceneManager.LoadScene("6 Letter DailyWord Game Scene");
    }

    void ButtonClicked()
    {
        var clicked = EventSystem.current.currentSelectedGameObject;
        if (clicked == null) return;

        if (SceneManager.GetActiveScene().name == "Main Scene")
            clickedButton1Tag = clicked.tag;

        else if (SceneManager.GetActiveScene().name == "Word Length Selection Scene")
            clickedButton2Tag = clicked.tag;
    }
}
