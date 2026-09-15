using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Global UI sound effect handler — plays hover/click sounds on all buttons.
/// Automatically hooks into all buttons in every scene.
/// </summary>
public class UISoundHandler : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInit()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Delay one frame to let all UI initialize
        var go = new GameObject("UISoundHook");
        var hook = go.AddComponent<UISoundHandler>();
    }

    void Start()
    {
        HookAllButtons();
        Destroy(gameObject, 1f); // Clean up after hooking
    }

    void HookAllButtons()
    {
        var buttons = FindObjectsOfType<Button>(true);
        foreach (var btn in buttons)
        {
            AddHoverSound(btn);
        }
    }

    public static void AddHoverSound(Button btn)
    {
        if (btn == null) return;
        var existing = btn.GetComponent<UIHoverSound>();
        if (existing == null)
            btn.gameObject.AddComponent<UIHoverSound>();
    }

    public static void AddClickSound(Button btn)
    {
        if (btn == null) return;
        var existing = btn.GetComponent<UIClickSound>();
        if (existing == null)
            btn.gameObject.AddComponent<UIClickSound>();
    }
}

/// <summary>Plays a sound when the mouse hovers over a button.</summary>
public class UIHoverSound : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance?.PlayCardHover();
    }
}

/// <summary>Plays a sound when a button is clicked.</summary>
public class UIClickSound : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        SoundManager.Instance?.PlayUIClick();
    }
}
