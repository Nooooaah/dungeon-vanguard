using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Plays GIF frames on a UI Image by cycling through sprites.
/// Loads all PNG frames from a folder at runtime and plays them in sequence.
/// </summary>
[RequireComponent(typeof(Image))]
public class GifPlayer : MonoBehaviour
{
    [Header("Settings")]
    public float frameDuration = 0.16f; // seconds per frame
    public bool loop = true;
    public string framesFolder = "gif_frames"; // folder name under Resources

    private Image _image;
    private List<Sprite> _frames = new List<Sprite>();
    private int _currentFrame = 0;
    private float _timer = 0f;

    void Start()
    {
        _image = GetComponent<Image>();
        LoadFrames();
    }

    void LoadFrames()
    {
        // Load all PNG textures from the folder
        var textures = Resources.LoadAll<Texture2D>(framesFolder);
        if (textures.Length == 0)
        {
            Debug.LogError("[GifPlayer] No frames found in Resources/" + framesFolder);
            return;
        }

        // Sort by name (frame_000, frame_001, ...)
        System.Array.Sort(textures, (a, b) => a.name.CompareTo(b.name));

        foreach (var tex in textures)
        {
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            _frames.Add(sprite);
        }

        Debug.Log("[GifPlayer] Loaded " + _frames.Count + " frames from " + framesFolder);

        if (_frames.Count > 0)
            _image.sprite = _frames[0];
    }

    void Update()
    {
        if (_frames.Count == 0) return;

        _timer += Time.deltaTime;
        if (_timer >= frameDuration)
        {
            _timer = 0f;
            _currentFrame++;

            if (_currentFrame >= _frames.Count)
            {
                if (loop)
                    _currentFrame = 0;
                else
                    return;
            }

            _image.sprite = _frames[_currentFrame];
        }
    }

    void OnDestroy()
    {
        foreach (var sprite in _frames)
        {
            if (sprite != null)
                Destroy(sprite);
        }
        _frames.Clear();
    }
}
