using UnityEngine;
using UnityEngine.UI;

public class HardModeToggle : MonoBehaviour
{
    public Toggle toggle;

    void Start()
    {
        toggle.isOn = PlayerPrefs.GetInt("HardMode", 0) == 1;
    }

    public void OnToggleChanged(bool value)
    {
        PlayerPrefs.SetInt("HardMode", value ? 1 : 0);
        PlayerPrefs.Save();
    }
}
