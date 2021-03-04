using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;

    public int poolSize = 1000;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject g = Instantiate(prefab);
            g.name = prefab.name;
            g.SetActive(false);
            g.transform.parent = transform;

            pool.Enqueue(g);
        }
    }

    public GameObject GetFromPool()
    {
        GameObject g = pool.Dequeue();
        g.SetActive(true);
        return g;
    }

    public void ReturnToPool(GameObject g)
    {
        g.SetActive(false);
        pool.Enqueue(g);
    }

}
