using System.Collections.Generic;
using UnityEngine;
using System.IO;

<<<<<<< HEAD
=======
/// <summary>
/// 玩家存档 —— 仅负责 HP 的 CSV 持久化。
/// 卡组数据由 GameManager._playerDeck 统一管理，不再在此重复存储。
/// </summary>
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; private set; }

<<<<<<< HEAD
    public CardStore CardStore;//卡片仓库脚本，获取卡牌数据

    public int Current_EP;
    public int Current_HP;
    public int MaxHP;
    public int MaxEP;
    public int[] Cards;
=======
    public int Current_HP;
    public int MaxHP;
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7

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

<<<<<<< HEAD
    //初始化
    void Start()
    {
        if (CardStore != null && CardStore.cardList != null)
        {
            Cards = new int[CardStore.cardList.Count];
        }
        else
        {
            Debug.LogError("[PlayerState] CardStore 未赋值，Cards 初始化失败");
            Cards = new int[0];
        }

        LoadPlayerState();
    }

    //保存数据
    public void SavePlayerState()
    {
        // 从 GameManager 同步最新血量数据
=======
    void Start()
    {
        LoadPlayerState();
    }

    /// <summary>保存 HP 数据到 CSV</summary>
    public void SavePlayerState()
    {
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
        if (GameManager.Instance != null)
        {
            Current_HP = GameManager.Instance.PlayerCurrentHp;
            MaxHP = GameManager.Instance.PlayerMaxHp;
        }

<<<<<<< HEAD
        List<string> data = new List<string>();
        //记录角色血量、能量
        data.Add("CurrentHP," + Current_HP.ToString());
        data.Add("MaxHP," + MaxHP.ToString());
        data.Add("CurrentEP," + Current_EP.ToString());
        data.Add("MaxEP," + MaxEP.ToString());
        //记录卡牌
        for (int i = 0; i < Cards.Length; i++)
        {
            if (Cards[i] != 0)
            {
                data.Add("Card," + i.ToString() + "," + Cards[i].ToString());
            }
        }
        //写入数据
        File.WriteAllLines(SavePath, data);
    }

    //加载数据
=======
        var data = new List<string>
        {
            "CurrentHP," + Current_HP.ToString(),
            "MaxHP," + MaxHP.ToString()
        };

        File.WriteAllLines(SavePath, data);
    }

    /// <summary>从 CSV 读取 HP 数据</summary>
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
    public void LoadPlayerState()
    {
        string content;

<<<<<<< HEAD
        // 优先读存档文件，不存在时回退到 TextAsset 初始数据
=======
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
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
<<<<<<< HEAD
            Debug.LogWarning("[PlayerState] 无存档文件且未配置 playerData，跳过加载");
=======
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
            return;
        }

        string[] dataRow = content.Split('\n');
        foreach (var row in dataRow)
        {
            if (string.IsNullOrWhiteSpace(row)) continue;

            string[] rowArray = row.Trim().Split(',');
            if (rowArray[0].StartsWith("#"))
<<<<<<< HEAD
            {
                continue;
            }
            else if (rowArray[0] == "CurrentHP")
            {
                Current_HP = int.Parse(rowArray[1]);
            }
            else if (rowArray[0] == "CurrentEP")
            {
                Current_EP = int.Parse(rowArray[1]);
            }
            else if (rowArray[0] == "MaxHP")
            {
                MaxHP = int.Parse(rowArray[1]);
            }
            else if (rowArray[0] == "MaxEP")
            {
                MaxEP = int.Parse(rowArray[1]);
            }
            else if (rowArray[0] == "Card")
            {
                int id = int.Parse(rowArray[1]);
                int num = int.Parse(rowArray[2]);
                if (id >= 0 && id < Cards.Length)
                {
                    Cards[id] = num;
                }
            }
        }

        // 加载后将血量数据同步回 GameManager
=======
                continue;
            else if (rowArray[0] == "CurrentHP")
                Current_HP = int.Parse(rowArray[1]);
            else if (rowArray[0] == "MaxHP")
                MaxHP = int.Parse(rowArray[1]);
        }

>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
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
