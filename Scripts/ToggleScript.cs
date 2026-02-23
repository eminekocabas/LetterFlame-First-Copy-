using UnityEngine;
using UnityEngine.UI;

public class ToggleManager : MonoBehaviour
{
    public static ToggleManager Instance;

    [Header("Toggles")]
    public Toggle hardModeToggle;
   // public Toggle musicToggle;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Hard Mode
        bool hardModeOn = PlayerPrefs.GetInt("HardMode", 0) == 1;
        hardModeToggle.isOn = hardModeOn;

        // Music
        bool musicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
       // musicToggle.isOn = musicOn;

        ApplyMusicState(musicOn);
    }

    public void OnHardModeToggleChanged(bool value)
    {
        PlayerPrefs.SetInt("HardMode", value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnMusicToggleChanged(bool value)
    {
        ApplyMusicState(value);
    }

    void ApplyMusicState(bool isOn)
    {
        if (isOn)
            MusicManager.Instance.PlayMusic();
        else
            MusicManager.Instance.StopMusic();

        PlayerPrefs.SetInt("MusicOn", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}
