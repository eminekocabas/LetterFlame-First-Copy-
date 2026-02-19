using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public GameObject gameOverScene;
    public GameObject congratsScene;
    public GameObject seeMyPointsButton;

    public MonoBehaviour gameScript;
    private IGameResult gameResult;

    void Start()
    {
        gameResult = gameScript as IGameResult;

        if (gameResult == null)
            Debug.LogError("GameScript IGameResult implement etmiyor!");
    }


    public void SeeStatsCongratsButton() 
    { 
        congratsScene.SetActive(false);
        seeMyPointsButton.SetActive(true);
    }

    public void SeeStatsGameOverButton()
    {
        gameOverScene.SetActive(false);
        seeMyPointsButton.SetActive(true);
    }


    public void SeeMyPointsButton()
    {
        if (gameResult == null) return;

        if (gameResult.Win)
        {
            congratsScene.SetActive(true);
        }
        else
        {
            gameOverScene.SetActive(true);
        }
    }

   

}
