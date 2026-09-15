using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// 玩家存档 —— 仅负责 HP 的 CSV 持久化。
/// 卡组数据由 GameManager._playerDeck 统一管理，不再在此重复存储。
/// </summary>
public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; private set; }

    public int Current_HP;
    public int MaxHP;

    public TextAsset playerData;

    private static readonly string SavePath = Application.persistentDataPath + "/playerdata.csv";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadPlayerState();
    }

    /// <summary>保存 HP 数据到 CSV</summary>
    public void SavePlayerState()
    {
        if (GameManager.Instance != null)
        {
            Current_HP = GameManager.Instance.PlayerCurrentHp;
            MaxHP = GameManager.Instance.PlayerMaxHp;
        }

        var data = new List<string>
        {
            "CurrentHP," + Current_HP.ToString(),
            "MaxHP," + MaxHP.ToString()
        };

        File.WriteAllLines(SavePath, data);
    }

    /// <summary>从 CSV 读取 HP 数据</summary>
    public void LoadPlayerState()
    {
        string content;

        if (File.Exists(SavePath))
        {
            content = File.ReadAllText(SavePath);
        }
        else if (playerData != null)
        {
            content = playerData.text;
        }
        else
        {
            return;
        }

        string[] dataRow = content.Split('\n');
        foreach (var row in dataRow)
        {
            if (string.IsNullOrWhiteSpace(row)) continue;

            string[] rowArray = row.Trim().Split(',');
            if (rowArray[0].StartsWith("#"))
                continue;
            else if (rowArray[0] == "CurrentHP")
                Current_HP = int.Parse(rowArray[1]);
            else if (rowArray[0] == "MaxHP")
                MaxHP = int.Parse(rowArray[1]);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerCurrentHp = Current_HP;
            GameManager.Instance.PlayerMaxHp = MaxHP;
        }
    }

    void OnApplicationQuit()
    {
        SavePlayerState();
    }
}
