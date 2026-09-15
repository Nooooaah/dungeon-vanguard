/// <summary>
/// 角色数值 —— 纯数据容器，不继承 MonoBehaviour
/// 组员B负责
/// </summary>
[System.Serializable]
public class CharacterStats
{
    public int maxHp;           // 最大生命值（默认60）
    public int currentHp;       // 当前生命值
    public int shield;          // 当前护盾
    public int maxEnergy;       // 最大能量（默认3）
    public int currentEnergy;   // 当前能量
    public Element element;     // 选择的元素

    public bool IsAlive => currentHp > 0;
    public bool IsDead => currentHp <= 0;

    public CharacterStats(int hp = 60, int energy = 3, Element e = Element.Fire)
    {
        maxHp = hp;
        currentHp = hp;
        maxEnergy = energy;
        currentEnergy = energy;
        shield = 0;
        element = e;
    }

    /// <summary>恢复满能量</summary>
    public void RestoreEnergy()
    {
        currentEnergy = maxEnergy;
    }

    /// <summary>消耗能量，返回是否成功</summary>
    public bool SpendEnergy(int amount)
    {
        if (currentEnergy < amount) return false;
        currentEnergy -= amount;
        return true;
    }

    /// <summary>清空护盾（回合开始时调用）</summary>
    public void ClearShield()
    {
        shield = 0;
    }
}
