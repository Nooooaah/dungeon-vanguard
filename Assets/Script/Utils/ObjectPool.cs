using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用对象池 —— 避免频繁 Instantiate/Destroy 造成GC卡顿
/// 组长负责，写在 Scripts/Utils/ObjectPool.cs
///
/// 使用场景：DamageText 跳字、元素反应特效、卡牌飞入动画等
///
/// 用法：
///   ObjectPool pool = new ObjectPool(prefab, 10);
///   GameObject obj = pool.Get();      // 取出一个
///   pool.Return(obj);                 // 归还
/// </summary>
public class ObjectPool
{
    private readonly GameObject _prefab;
    private readonly Queue<GameObject> _pool = new Queue<GameObject>();
    private readonly Transform _parent;
    private readonly MonoBehaviour _coroutineRunner;

    /// <param name="prefab">要池化的预制体</param>
    /// <param name="initialSize">初始创建数量</param>
    /// <param name="parent">池对象挂载的父节点</param>
    /// <param name="coroutineRunner">用于运行延迟归还协程的MonoBehaviour</param>
    public ObjectPool(GameObject prefab, int initialSize = 10, Transform parent = null, MonoBehaviour coroutineRunner = null)
    {
        _prefab = prefab;
        _parent = parent;
        _coroutineRunner = coroutineRunner;
        Prewarm(initialSize);
    }

    /// <summary>预热：提前创建 initialSize 个实例</summary>
    private void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = CreateNew();
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    /// <summary>从池中取出一个对象</summary>
    public GameObject Get()
    {
        if (_pool.Count == 0)
        {
            // 池子空了，临时创建一个
            var newObj = CreateNew();
            newObj.SetActive(true);
            return newObj;
        }

        var obj = _pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    /// <summary>从池中取出对象，并设置位置</summary>
    public GameObject GetAt(Vector3 position, Quaternion? rotation = null)
    {
        var obj = Get();
        obj.transform.position = position;
        if (rotation.HasValue)
            obj.transform.rotation = rotation.Value;
        return obj;
    }

    /// <summary>归还对象到池中</summary>
    public void Return(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        obj.transform.SetParent(_parent);
        _pool.Enqueue(obj);
    }

    /// <summary>
    /// 延迟归还（用于动画播完再回收，如 DamageText 浮动1秒后回收）
    /// </summary>
    public void ReturnAfter(GameObject obj, float delay)
    {
        if (obj == null) return;
        if (_coroutineRunner != null)
            _coroutineRunner.StartCoroutine(ReturnAfterCoroutine(obj, delay));
        else
            Return(obj);
    }

    private System.Collections.IEnumerator ReturnAfterCoroutine(GameObject obj, float delay)
    {
        yield return new UnityEngine.WaitForSeconds(delay);
        Return(obj);
    }

    private GameObject CreateNew()
    {
        var obj = Object.Instantiate(_prefab, _parent);
        obj.name = _prefab.name; // 去掉 (Clone) 后缀
        return obj;
    }

    /// <summary>清空池子，销毁所有对象</summary>
    public void Clear()
    {
        while (_pool.Count > 0)
        {
            var obj = _pool.Dequeue();
            if (obj != null)
                Object.Destroy(obj);
        }
    }

    /// <summary>当前池中可用对象数量</summary>
    public int AvailableCount => _pool.Count;
}


/// <summary>
/// 全局对象池管理器 —— 挂载在 GameManager 的 GameObject 上
/// 集中管理所有类型的对象池
///
/// 用法：
///   PoolManager.Instance.Get("DamageText", position);
///   PoolManager.Instance.Return("DamageText", obj);
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    // 所有对象池的注册表
    private Dictionary<string, ObjectPool> _pools = new Dictionary<string, ObjectPool>();

    void Awake()
    {
        Instance = this;
    }

    /// <summary>注册一个新的对象池</summary>
    public void RegisterPool(string key, GameObject prefab, int initialSize, Transform parent = null)
    {
        if (!_pools.ContainsKey(key))
        {
            _pools[key] = new ObjectPool(prefab, initialSize, parent ?? transform, this);
        }
    }

    /// <summary>延迟归还对象到指定池</summary>
    public void ReturnAfter(string key, GameObject obj, float delay)
    {
        if (_pools.TryGetValue(key, out var pool))
            pool.ReturnAfter(obj, delay);
    }

    /// <summary>从指定池中取出对象</summary>
    public GameObject Get(string key)
    {
        if (_pools.TryGetValue(key, out var pool))
            return pool.Get();
        Debug.LogWarning($"[PoolManager] 池 '{key}' 不存在");
        return null;
    }

    /// <summary>从指定池中取出对象并设置位置</summary>
    public GameObject GetAt(string key, Vector3 position)
    {
        if (_pools.TryGetValue(key, out var pool))
            return pool.GetAt(position);
        Debug.LogWarning($"[PoolManager] 池 '{key}' 不存在");
        return null;
    }

    /// <summary>归还对象到指定池</summary>
    public void Return(string key, GameObject obj)
    {
        if (_pools.TryGetValue(key, out var pool))
            pool.Return(obj);
    }

    /// <summary>清空所有池</summary>
    public void ClearAll()
    {
        foreach (var pool in _pools.Values)
            pool.Clear();
        _pools.Clear();
    }
}
