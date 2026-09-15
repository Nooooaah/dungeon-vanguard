using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button exitButton;

    void Start()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGame);
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinue);
            continueButton.interactable = HasSaveData();
        }
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExit);
    }

    void OnNewGame()
    {
<<<<<<< HEAD
        UnityEngine.SceneManagement.SceneManager.LoadScene(GameConstants.SCENE_ELEMENT_SELECTION);
=======
        UnityEngine.SceneManagement.SceneManager.LoadScene("ElementSelection");
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
    }

    void OnContinue()
    {
        Debug.Log("[MainMenu] Continue game");
    }

    void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    bool HasSaveData()
    {
        return PlayerPrefs.GetInt("HasSave", 0) == 1;
    }
}
