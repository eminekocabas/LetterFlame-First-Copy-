using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameScript_DailyWord : MonoBehaviour, IGameResult
{
    #region JSON

    [System.Serializable]
    public class LetterGroup
    {
        public string Harf;
        public string[] Kelimeler;
    }

    [System.Serializable]
    public class LetterGroupList
    {
        public LetterGroup[] groups;
    }

    #endregion

    #region DATA

    private LetterGroupList wordData;
    private string correctWord;
    private HashSet<string> validGuesses;

    [Header("JSON Files")]
    [SerializeField] private TextAsset answersFile;
    [SerializeField] private TextAsset validWordsFile;

    #endregion

    #region UI

    public TMP_Text[][] allRows;
    public TMP_Text[] tiles1, tiles2, tiles3, tiles4, tiles5, tiles6;

    public TMP_Text gameOverWordText;
    public TMP_Text congratsWordText;

    public GameObject gameOverScreen;
    public GameObject congractsScreen;

    #endregion

    #region GAME STATE

    private bool gameEnded = false;
    private int currentRow = 0;
    private int currentIndex = 0;
    private int numGuess = 0;
    private string currentGuess = "";

    public int wordLength;
    public bool win = false;
    public bool Win => win;

    public List<char> grayLetters = new List<char>();
    CultureInfo tr = new CultureInfo("tr-TR");

    #endregion

    #region UNITY

    void Start()
    {
        allRows = new TMP_Text[][]
        {
            tiles1, tiles2, tiles3, tiles4, tiles5, tiles6
        };

        LoadAnswerWords();
        LoadValidWords();
        StartDailyMode();
    }

    void Update()
    {
        if (gameEnded) return;
        HandleInput();
    }

    #endregion

    #region DAILY SYSTEM

    string TodayKey => System.DateTime.UtcNow.ToString("yyyyMMdd");
    string ModeKey => $"L{wordLength}";

    string WordKey => $"DailyWord_{TodayKey}_{ModeKey}";
    string GuessKey => $"DailyGuesses_{TodayKey}_{ModeKey}";
    string PlayedKey => $"DailyPlayed_{TodayKey}_{ModeKey}";
    string WinKey => $"DailyWin_{TodayKey}_{ModeKey}";

    void StartDailyMode()
    {
        // 1️⃣ Günlük kelime
        if (PlayerPrefs.HasKey(WordKey))
            correctWord = PlayerPrefs.GetString(WordKey);
        else
        {
            SelectDailyWord();
            PlayerPrefs.SetString(WordKey, correctWord);
            PlayerPrefs.Save();
        }

        // 2️⃣ Önceki tahminleri yükle
        LoadDailyGuesses();

        // 3️⃣ Bugün bu mod oynanmış mı?
        if (PlayerPrefs.HasKey(PlayedKey))
        {
            gameEnded = true;
            ShowDailyResult();
        }
    }

    void FinishDailyGame(bool isWin)
    {
        PlayerPrefs.SetInt(PlayedKey, 1);
        PlayerPrefs.SetInt(WinKey, isWin ? 1 : 0);
        PlayerPrefs.Save();
    }

    void ShowDailyResult()
    {
        bool isWin = PlayerPrefs.GetInt(WinKey, 0) == 1;

        if (isWin)
        {
            congratsWordText.text = correctWord;
            congractsScreen.SetActive(true);
        }
        else
        {
            gameOverWordText.text = correctWord;
            gameOverScreen.SetActive(true);
        }
    }

    #endregion

    #region WORD SELECTION

    void SelectDailyWord()
    {
        int seed = int.Parse(TodayKey + wordLength);
        System.Random rng = new System.Random(seed);

        int total = wordData.groups.Sum(g => g.Kelimeler.Length);
        int index = rng.Next(total);

        foreach (var group in wordData.groups)
        {
            if (index < group.Kelimeler.Length)
            {
                correctWord = group.Kelimeler[index].ToUpper(tr);
                return;
            }
            index -= group.Kelimeler.Length;
        }
    }

    #endregion

    #region INPUT

    void HandleInput()
    {
        foreach (char c in Input.inputString)
        {
            if (char.IsLetter(c) && currentIndex < wordLength)
            {
                char u = char.ToUpper(c, tr);
                currentGuess += u;
                allRows[currentRow][currentIndex].text = u.ToString();
                currentIndex++;
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace) && currentIndex > 0)
        {
            currentIndex--;
            currentGuess = currentGuess[..^1];
            allRows[currentRow][currentIndex].text = "";
        }

        if (Input.GetKeyDown(KeyCode.Return) && currentGuess.Length == wordLength)
        {
            CheckWord(currentGuess, false);
        }
    }

    #endregion

    #region WORD LOGIC

    void CheckWord(string guess, bool fromLoad)
    {
        List<char> temp = new List<char>(correctWord);

        if (!fromLoad)
        {
            if (!validGuesses.Contains(guess)) return;
            if (SceneLoader.HardMode && guess.Any(x => grayLetters.Contains(x))) return;
        }

        bool[] green = new bool[wordLength];

        for (int i = 0; i < wordLength; i++)
        {
            if (guess[i] == correctWord[i])
            {
                allRows[currentRow][i].GetComponentInParent<Image>().color = Color.green;
                green[i] = true;
                temp.Remove(guess[i]);
            }
        }

        for (int i = 0; i < wordLength; i++)
        {
            if (green[i]) continue;

            Image img = allRows[currentRow][i].GetComponentInParent<Image>();

            if (temp.Contains(guess[i]))
            {
                img.color = Color.yellow;
                temp.Remove(guess[i]);
            }
            else
            {
                img.color = Color.gray;
                if (!correctWord.Contains(guess[i]))
                    grayLetters.Add(guess[i]);
            }
        }

        numGuess++;
        CheckWin(guess,correctWord, numGuess);

        if (!fromLoad)
            SaveDailyGuess(guess);

        if (guess == correctWord)
        {
            gameEnded = true;
            win = true;
            FinishDailyGame(true);
            ShowDailyResult();
        }
        else if (numGuess == 6)
        {
            gameEnded = true;
            FinishDailyGame(false);
            ShowDailyResult();
        }

        currentRow++;
        currentIndex = 0;
        currentGuess = "";
    }

    void CheckWin(string guess, string correctWord, int numGuess)
    {
        if (guess == correctWord)
        {
            Debug.Log("Tebrikler! Kazandınız!");
            gameEnded = true;
            win = true;
            congratsWordText.text = correctWord;
            congractsScreen.SetActive(true);
        }
        else if (numGuess == 6)
        {
            Debug.Log("Oyun Bitti :( Kelime: " + correctWord);
            gameEnded = true;
            gameOverWordText.text = correctWord;
            gameOverScreen.SetActive(true);

        }
    }

    #endregion

    #region SAVE / LOAD

    void SaveDailyGuess(string guess)
    {
        string data = PlayerPrefs.GetString(GuessKey, "");
        PlayerPrefs.SetString(GuessKey, data + guess + "|");
        PlayerPrefs.Save();
    }

    void WriteGuessToRow(string guess)
    {
        for (int i = 0; i < guess.Length; i++)
            allRows[currentRow][i].text = guess[i].ToString();
    }

    void LoadDailyGuesses()
    {
        if (!PlayerPrefs.HasKey(GuessKey)) return;

        string[] guesses = PlayerPrefs.GetString(GuessKey).Split('|');

        foreach (string g in guesses)
        {
            if (string.IsNullOrEmpty(g)) continue;
            WriteGuessToRow(g);
            CheckWord(g, true);
        }
    }

    #endregion

    #region LOAD JSON

    void LoadAnswerWords()
    {
        string json = "{ \"groups\": " + answersFile.text + " }";
        wordData = JsonUtility.FromJson<LetterGroupList>(json);
    }

    void LoadValidWords()
    {
        string json = "{ \"groups\": " + validWordsFile.text + " }";
        LetterGroupList data = JsonUtility.FromJson<LetterGroupList>(json);

        validGuesses = new HashSet<string>();
        foreach (var g in data.groups)
            foreach (string w in g.Kelimeler)
                validGuesses.Add(w.ToUpper(tr));
    }

    #endregion
}
