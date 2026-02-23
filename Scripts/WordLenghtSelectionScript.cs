using UnityEngine;
using UnityEngine.UI;

public class WordLengthSelector : MonoBehaviour
{
    public Button[] buttons;
    public Color selectedColor;
    public Color normalColor;

    public static int selectedLength;

    void Awake()
    {
        selectedLength = PlayerPrefs.GetInt("WordLength", 5);
    }

    void Start()
    {
        UpdateVisuals();
    }

    public void SelectLength(int length)
    {
        selectedLength = length;

        PlayerPrefs.SetInt("WordLength", length);
        PlayerPrefs.Save();

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        foreach (Button btn in buttons)
        {
            if (!int.TryParse(btn.tag, out int length))
                continue;

            btn.image.color =
                (length == selectedLength) ? selectedColor : normalColor;
        }
    }
}