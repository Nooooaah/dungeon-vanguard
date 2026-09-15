using System.Collections.Generic;

/// <summary>
/// Buff运行时数据 —— 挂在目标身上的一条Buff实例
/// </summary>
public class BuffInstance
{
    public BuffType Type;
    public int value;        // 每回合效果值（灼烧=扣血量）
    public int remainingDuration; // 剩余回合数
    public IDamageable target;    // 挂载目标

    public bool IsExpired => remainingDuration <= 0;

    /// <summary>每回合Tick，返回是否仍然有效</summary>
    public void Tick()
    {
        remainingDuration--;
    }
}

/// <summary>
/// Buff（持续效果）系统 —— 管理灼烧/冰冻的施加与结算
/// 组长负责，写在 Scripts/Combat/BuffSystem.cs
/// </summary>
public class BuffSystem
{
    // 所有活跃Buff列表
    private List<BuffInstance> _activeBuffs = new List<BuffInstance>();

    /// <summary>
    /// 给目标施加一个Buff
    /// </summary>
    /// <param name="target">目标</param>
    /// <param name="type">Buff类型</param>
    /// <param name="value">效果值（灼烧=每回合扣血量，冰冻无用）</param>
    /// <param name="duration">持续回合数</param>
    public void ApplyBuff(IDamageable target, BuffType type, int value, int duration)
    {
        if (!target.IsAlive) return;
        if (type == BuffType.None) return;
        if (duration <= 0) return;

        // 冰冻不叠加：已有冰冻则跳过
        if (type == BuffType.Freeze && HasBuff(target, BuffType.Freeze))
            return;

        // 灼烧可叠加层数
        if (type == BuffType.Burn)
        {
            var existingBurn = _activeBuffs.Find(b =>
                b.target == target && b.Type == BuffType.Burn && !b.IsExpired);
            if (existingBurn != null)
            {
                // 叠加：伤害取最大，回合数刷新
                existingBurn.value = existingBurn.value > value ? existingBurn.value : value;
                existingBurn.remainingDuration = duration;
                return;
            }
        }

        _activeBuffs.Add(new BuffInstance
        {
            Type = type,
            value = value,
            remainingDuration = duration,
            target = target
        });
    }

    /// <summary>
    /// 检查目标是否挂有指定Buff
    /// </summary>
    public bool HasBuff(IDamageable target, BuffType type)
    {
        return _activeBuffs.Exists(b =>
            b.target == target && b.Type == type && !b.IsExpired);
    }

    /// <summary>
    /// 获取目标的跳过回合状态（冰冻检查）
    /// </summary>
    public bool ShouldSkipTurn(IDamageable target)
    {
        return HasBuff(target, BuffType.Freeze);
    }

    /// <summary>
    /// 每回合开始时调用 —— 结算所有Buff的Tick效果
    /// </summary>
    /// <returns>(事件描述列表, 被冰冻跳过的目标集合)</returns>
    public (List<string> events, HashSet<IDamageable> frozenTargets) TickAllBuffs()
    {
        List<string> events = new List<string>();
        HashSet<IDamageable> frozenTargets = new HashSet<IDamageable>();

        foreach (var buff in _activeBuffs)
        {
            if (buff.IsExpired) continue;
            if (!buff.target.IsAlive) continue;

            switch (buff.Type)
            {
                case BuffType.Burn:
                    // 灼烧：每回合扣血
                    buff.target.TakeDamage(buff.value, Element.Fire);
                    events.Add($"[灼烧] 造成 {buff.value} 点伤害（剩余{buff.remainingDuration - 1}回合）");
                    break;

                case BuffType.Freeze:
                    // 冰冻：跳过本回合
                    frozenTargets.Add(buff.target);
                    events.Add($"[冰冻] 生效，跳过行动");
                    break;
            }

            buff.Tick();
        }

        // 清理过期Buff
        _activeBuffs.RemoveAll(b => b.IsExpired || !b.target.IsAlive);

        return (events, frozenTargets);
    }

    /// <summary>
    /// 结算指定目标的Buff效果（灼烧扣血、冰冻检查），不减少持续时间
    /// </summary>
    public (List<string> events, HashSet<IDamageable> frozenTargets) ProcessBuffsForTargets(List<IDamageable> targets)
    {
        List<string> events = new List<string>();
        HashSet<IDamageable> frozenTargets = new HashSet<IDamageable>();

        foreach (var buff in _activeBuffs)
        {
            if (buff.IsExpired) continue;
            if (!buff.target.IsAlive) continue;
            if (!targets.Contains(buff.target)) continue;

            switch (buff.Type)
            {
                case BuffType.Burn:
                    buff.target.TakeDamage(buff.value, Element.Fire);
                    events.Add($"[灼烧] 造成 {buff.value} 点伤害（剩余{buff.remainingDuration}回合）");
                    break;

                case BuffType.Freeze:
                    frozenTargets.Add(buff.target);
                    events.Add($"[冰冻] 生效，跳过行动");
                    break;
            }
        }

        _activeBuffs.RemoveAll(b => b.IsExpired || !b.target.IsAlive);

        return (events, frozenTargets);
    }

    /// <summary>
    /// 减少所有Buff持续时间，清理过期Buff（每回合结束时调用一次）
    /// </summary>
    public void TickAllDurations()
    {
        foreach (var buff in _activeBuffs)
        {
            buff.Tick();
        }
        _activeBuffs.RemoveAll(b => b.IsExpired || !b.target.IsAlive);
    }

    /// <summary>
    /// 战斗结束时清理所有Buff
    /// </summary>
    public void ClearAllBuffs()
    {
        _activeBuffs.Clear();
    }

    /// <summary>
    /// 移除目标身上的所有Buff（回复点用）
    /// </summary>
    public void RemoveAllBuffsFrom(IDamageable target)
    {
        _activeBuffs.RemoveAll(b => b.target == target);
    }
}
