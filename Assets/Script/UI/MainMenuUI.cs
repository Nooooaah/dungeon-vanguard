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
        UnityEngine.SceneManagement.SceneManager.LoadScene("ElementSelection");
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
