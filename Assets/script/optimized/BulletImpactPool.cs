using UnityEngine;
using System.Collections.Generic;

public class BulletImpactPool : MonoBehaviour
{
    public static BulletImpactPool Instance;

    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private GameObject hitParticlePrefab;
    [SerializeField] private int poolSize = 30;

    private Queue<GameObject> holePool = new();
    private Queue<GameObject> particlePool = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        for (int i = 0; i < poolSize; i++)
        {
            CreateNew(bulletHolePrefab, holePool);
            CreateNew(hitParticlePrefab, particlePool);
        }
    }

    private void CreateNew(GameObject prefab, Queue<GameObject> pool)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    public GameObject GetHole() => GetFromPool(holePool, bulletHolePrefab);
    public GameObject GetParticle() => GetFromPool(particlePool, hitParticlePrefab);

    private GameObject GetFromPool(Queue<GameObject> pool, GameObject prefab)
    {
        foreach (var obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        GameObject newObj = Instantiate(prefab, transform);
        newObj.SetActive(true);
        pool.Enqueue(newObj);
        return newObj;
    }
}
