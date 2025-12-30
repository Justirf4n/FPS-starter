using UnityEngine;
using System.Collections.Generic;

public class BulletTrailPool : MonoBehaviour
{
    public static BulletTrailPool Instance;

    [SerializeField] private GameObject trailPrefab;
    [SerializeField] private int poolSize = 20;

    private Queue<GameObject> pool = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < poolSize; i++)
            CreateNew();
    }

    private void CreateNew()
    {
        GameObject obj = Instantiate(trailPrefab, transform);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    public GameObject Get()
    {
        foreach (var obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        CreateNew();
        var newObj = pool.Peek();
        newObj.SetActive(true);
        return newObj;
    }
}
