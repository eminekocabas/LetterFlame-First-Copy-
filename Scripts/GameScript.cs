using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;




public class GameScript : MonoBehaviour, IGameResult
{
    #region JSON CLASSES

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
    // JSON DATA
    private LetterGroupList wordData;
    private string correctWord;
    private HashSet<string> validGuesses;
    [Header("JSON Files")]
    [SerializeField] private TextAsset answersFile;
    [SerializeField] private TextAsset validWordsFile;

    // UI
    public TMP_Text[][] allRows;
    public TMP_Text[] tiles1;
    public TMP_Text[] tiles2;
    public TMP_Text[] tiles3;
    public TMP_Text[] tiles4;
    public TMP_Text[] tiles5;
    public TMP_Text[] tiles6;
    public TMP_Text gameOverWordText;
    public TMP_Text congratsWordText;
    public GameObject gameOverScreen;
    public GameObject congractsScreen;
    // GAME STATE
    private bool gameEnded = false;
    private int currentRow = 0;
    private int currentIndex = 0;
    private string currentGuess = "";
    private int numGuess = 0;
    public int wordLength;

    public bool win = false;
    public bool Win => win;
    [SerializeField] public List<char> grayLetters = new List<char>();

    CultureInfo tr = new CultureInfo("tr-TR");

    void Start()
    {
        allRows = new TMP_Text[][]
        {
            tiles1,
            tiles2,
            tiles3,
            tiles4,
            tiles5,
            tiles6
        };
       
        LoadAnswerWords();
        LoadValidWords();
        Debug.Log("hard mode is " + SceneLoader.HardMode);
        SelectRandomWord();
    }

    void Update()
    {
        if (gameEnded) return;
        HandleInput();
    }

    #region LOAD JSON

    void LoadAnswerWords()
    {
        if (answersFile == null)
        {
            Debug.LogError("Answers JSON atanmadı!");
            return;
        }

        string wrappedJson = "{ \"groups\": " + answersFile.text + " }";
        wordData = JsonUtility.FromJson<LetterGroupList>(wrappedJson);
    }

    void LoadValidWords()
    {
        if (validWordsFile == null)
        {
            Debug.LogError("ValidWords JSON atanmadı!");
            return;
        }

        string wrappedJson = "{ \"groups\": " + validWordsFile.text + " }";
        LetterGroupList data = JsonUtility.FromJson<LetterGroupList>(wrappedJson);

        validGuesses = new HashSet<string>();

        foreach (var group in data.groups)
        {
            foreach (string word in group.Kelimeler)
            {
                validGuesses.Add(word.ToUpper(tr));
            }
        }

        // Debug.Log("Toplam geçerli kelime: " + validGuesses.Count);
    }

    void SelectRandomWord()
    {
        // 1️⃣ Toplam kelime sayısını hesapla
        int totalWordCount = 0;
        foreach (var group in wordData.groups)
        {
            totalWordCount += group.Kelimeler.Length;
        }

        // 2️⃣ Tüm kelimeler arasından rastgele index
        int randomIndex = Random.Range(0, totalWordCount);

        // 3️⃣ Hangi gruba düştüğünü bul
        foreach (var group in wordData.groups)
        {
            if (randomIndex < group.Kelimeler.Length)
            {
                correctWord = group.Kelimeler[randomIndex].ToUpper(tr);
                //Debug.Log("Seçilen kelime: " + correctWord);
                return;
            }
            else
            {
                randomIndex -= group.Kelimeler.Length;
            }
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
                char upperChar = char.ToUpper(c, tr);

                currentGuess += upperChar;
                allRows[currentRow][currentIndex].text = upperChar.ToString();
                currentIndex++;
            }
        }

        // BACKSPACE
        if (Input.GetKeyDown(KeyCode.Backspace) && currentIndex > 0)
        {
            currentIndex--;
            currentGuess = currentGuess.Substring(0, currentGuess.Length - 1);
            allRows[currentRow][currentIndex].text = "";
        }

        // ENTER
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentGuess.Length == wordLength)
            {
                CheckWord(currentGuess);
                
            }
        }
    }

    #endregion

    #region WORD CHECK

    void CheckWord(string guess)
    {
        List<char> tempCorrectLetters = new List<char>(correctWord);
        bool includesGrayLetter = guess.Any(x => grayLetters.Contains(x));

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

            bool[] isGreen = new bool[wordLength];

        for (int i = 0; i < wordLength; i++)
        {
            if (guess[i] == correctWord[i])
            {
                Image tileImage = allRows[currentRow][i].GetComponentInParent<Image>();
                tileImage.color = Color.green;

                isGreen[i] = true;
                tempCorrectLetters.Remove(guess[i]);
            }
        }

        for (int i = 0; i < wordLength; i++)
        {
            if (isGreen[i])
                continue;

            Image tileImage = allRows[currentRow][i].GetComponentInParent<Image>();

            if (tempCorrectLetters.Contains(guess[i]))
            {
                tileImage.color = Color.yellow;
                tempCorrectLetters.Remove(guess[i]);
            }
            else
            {
                tileImage.color = Color.gray;
                if (!correctWord.Contains(guess[i]))
                {
                    grayLetters.Add(guess[i]);
                }
                
            }
        }

        numGuess++;
        CheckWin(currentGuess, correctWord, numGuess);

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

        void ClearRow()
        {
            for (int i = 0; i < wordLength; i++)
                allRows[currentRow][i].text = "";

            currentIndex = 0;
        }


    #endregion
}
