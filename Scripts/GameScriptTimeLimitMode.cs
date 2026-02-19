using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameScriptTimeLimitMode : MonoBehaviour, IGameResult
{
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
    // ================= JSON =================
    private LetterGroupList wordData;
    private string correctWord;
    private HashSet<string> validGuesses;

    [Header("JSON Files")]
    [SerializeField] private TextAsset answersFile;
    [SerializeField] private TextAsset validWordsFile;

    // ================= UI =================
    public TMP_Text[][] allRows;
    public TMP_Text[] tiles1;
    public TMP_Text[] tiles2;
    public TMP_Text[] tiles3;
    public TMP_Text[] tiles4;
    public TMP_Text[] tiles5;
    public TMP_Text[] tiles6;

    [Header("End Screens")]
    public TMP_Text gameOverWordText;
    public TMP_Text congratsWordText;
    public GameObject gameOverScreen;
    public GameObject congractsScreen;

    [Header("Timer UI")]
    public TMP_Text timerText;

    // ================= GAME STATE =================
    public int wordLength = 5;
    public int timeLimitSeconds = 60;

    private float timeLeft;
    private Coroutine timerCoroutine;
    private bool gameEnded = false;

    private int currentRow = 0;
    private int currentIndex = 1;
    private int numGuess = 0;
    // public Toggle HardModeToggle;
    public bool win = false;
    public bool Win => win;
    [SerializeField] public List<char> grayLetters = new List<char>();

    private string currentGuess = "";
    private string theFirstLetter;

    CultureInfo tr = new CultureInfo("tr-TR");

    // ================= START =================
    void Start()
    {
        allRows = new TMP_Text[][]
        {
            tiles1, tiles2, tiles3, tiles4, tiles5, tiles6
        };

        LoadAnswerWords();
        LoadValidWords();
        SelectRandomWord();
        
        StartTimer();
    }

    void Update()
    {
        if (gameEnded) return;
        //if (HardModeToggle.isOn)
        //{
        //    hardMode = true;
        //}
        HandleInput();
    }

    // ================= TIMER =================
    void StartTimer()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timeLeft = timeLimitSeconds;
        timerCoroutine = StartCoroutine(CountdownTimer());
    }

    IEnumerator CountdownTimer()
    {
        while (timeLeft > 0 && !gameEnded)
        {
            timerText.text = Mathf.Ceil(timeLeft).ToString();
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        if (!gameEnded)
        {
            timerText.text = "0";
            TimeUp();
        }
    }

    void TimeUp()
    {
        EndGame();
        gameOverWordText.text = correctWord;
        gameOverScreen.SetActive(true);
    }

    // ================= END GAME =================
    void EndGame()
    {
        gameEnded = true;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    // ================= LOAD JSON =================
    void LoadAnswerWords()
    {
        string wrappedJson = "{ \"groups\": " + answersFile.text + " }";
        wordData = JsonUtility.FromJson<LetterGroupList>(wrappedJson);
    }

    void LoadValidWords()
    {
        string wrappedJson = "{ \"groups\": " + validWordsFile.text + " }";
        LetterGroupList data = JsonUtility.FromJson<LetterGroupList>(wrappedJson);

        validGuesses = new HashSet<string>();

        foreach (var group in data.groups)
            foreach (string word in group.Kelimeler)
                validGuesses.Add(word.ToUpper(tr));
    }

    // ================= WORD SELECTION =================
    void SelectRandomWord()
    {
        int total = 0;
        foreach (var g in wordData.groups)
            total += g.Kelimeler.Length;

        int index = Random.Range(0, total);

        foreach (var g in wordData.groups)
        {
            if (index < g.Kelimeler.Length)
            {
                correctWord = g.Kelimeler[index].ToUpper(tr);

                theFirstLetter = correctWord[0].ToString();
                allRows[0][0].text = theFirstLetter;
                allRows[0][0].color = Color.blue;

                currentGuess = theFirstLetter;
                currentIndex = 1;
                return;
            }

            index -= g.Kelimeler.Length;
        }
    }

    // ================= INPUT =================
    void HandleInput()
    {
        foreach (char c in Input.inputString)
        {
            if (char.IsLetter(c) && currentIndex < wordLength)
            {
                char upper = char.ToUpper(c, tr);
                currentGuess += upper;
                allRows[currentRow][currentIndex].text = upper.ToString();
                currentIndex++;
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace) && currentIndex > 1)
        {
            currentIndex--;
            currentGuess = currentGuess[..^1];
            allRows[currentRow][currentIndex].text = "";
        }

        if (Input.GetKeyDown(KeyCode.Return) && currentGuess.Length == wordLength)
        {
            CheckWord(currentGuess);
        }
    }

    // ================= WORD CHECK =================

    void CheckWord(string guess)
    {
        List<char> tempCorrectLetters = new List<char>(correctWord);

        bool includesGrayLetter = guess.Any(c => grayLetters.Contains(c));

        Debug.Log("IncludesGrayLetters: " + includesGrayLetter);
        if (!validGuesses.Contains(guess))
        {
            Debug.Log("Invalid word.");
            ClearRow();
            return;
        }
        else if (includesGrayLetter && SceneLoader.HardMode)
        {
            Debug.Log("Hard Mode does not allow you to use gray letters again.");
            ClearRow();
            return;

        }

        List<char> temp = new List<char>(correctWord);
        bool[] green = new bool[wordLength];

        for (int i = 0; i < wordLength; i++)
        {
            Image img = allRows[currentRow][i].GetComponentInParent<Image>();

            if (guess[i] == correctWord[i])
            {
                img.color = Color.green;
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
                {
                    grayLetters.Add(guess[i]);
                }
                
            }
        }
        
        numGuess++;

        CheckWin(guess);

        if (gameEnded)
        {
            grayLetters.Clear();
            return;
        }
            
            

        currentRow++;
        allRows[currentRow][0].text = theFirstLetter;
        allRows[currentRow][0].color = Color.blue;

        currentGuess = theFirstLetter;
        currentIndex = 1;
        StartTimer();

    }

    void ClearRow()
    {
        for (int i = 1; i < wordLength; i++)
            allRows[currentRow][i].text = "";

        currentGuess = theFirstLetter;
        currentIndex = 1;
    }

    void CheckWin(string guess)
    {
        if (guess == correctWord)
        {
            win = true;
            EndGame();
            congratsWordText.text = correctWord;
            congractsScreen.SetActive(true);
        }
        else if (numGuess == 6)
        {
            EndGame();
            gameOverWordText.text = correctWord;
            gameOverScreen.SetActive(true);
        }
}
}
