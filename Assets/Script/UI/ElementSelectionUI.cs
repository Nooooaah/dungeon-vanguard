using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Element Selection: 3 element cards (Fire/Water/Wind), click to start game.
/// </summary>
public class ElementSelectionUI : MonoBehaviour
{
    [Header("Element Buttons")]
    public Button fireButton;
    public Button waterButton;
    public Button windButton;

    [Header("Back")]
    public Button backButton;

    void Start()
    {
        if (fireButton != null)
            fireButton.onClick.AddListener(() => SelectElement(Element.Fire));
        if (waterButton != null)
            waterButton.onClick.AddListener(() => SelectElement(Element.Water));
        if (windButton != null)
            windButton.onClick.AddListener(() => SelectElement(Element.Wind));
        if (backButton != null)
            backButton.onClick.AddListener(OnBack);
    }

    void SelectElement(Element element)
    {
        PlayerPrefs.SetInt("SelectedElement", (int)element);
        PlayerPrefs.Save();

        if (GameManager.Instance != null)
            GameManager.Instance.StartNewGame(element);
        else
            Debug.LogError("[ElementSelection] GameManager.Instance is null!");

        Debug.Log("[ElementSelection] Selected: " + element);
    }

    void OnBack()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(GameConstants.SCENE_MAIN_MENU);
    }
}
