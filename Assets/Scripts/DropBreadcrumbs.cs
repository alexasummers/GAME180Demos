using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropBreadcrumbs : MonoBehaviour
{
    [Tooltip("The number of breadcrumbs to show before recycling.")]
    public int trailSize = 30;
    private Queue<GameObject> spawnedCrumbs = new Queue<GameObject>();

    public float crumbsPerSecond = 4;  // breadcrumbs per second

    // qd-timer pattern
    private float dropTimer = 0, dropTimeout = 0;

    // Breadcrumb pool
    [Tooltip("An ObjectPool to spawn breadcrumbs. If one is not assigned, this component will look for one whose name starts with 'breadcrumb.'")]
    public ObjectPool breadcrumbPool;

    [Tooltip("Where to place the breadcrumb relative to the agent's position. Normally this will only need a Y value.")]
    public Vector3 dropPositionOffset = Vector3.zero;


    void Start()
    {
        dropTimeout = 1f / crumbsPerSecond;

        if (breadcrumbPool == null)
        {
            // Try to find one.

            // A list of all ObjectPool objects in the scene.
            ObjectPool[] pools = GameObject.FindObjectsOfType<ObjectPool>();

            // Search them for one that starts with the word "breadcrumb."
            for (int i = 0; i < pools.Length; i++)
                if (pools[i].name.ToLower().StartsWith("breadcrumb")) breadcrumbPool = pools[i];

            // If still nothing, standard dependency-disable pattern.
            if (breadcrumbPool == null)
            {
                this.enabled = false;
                Debug.LogError("Object " + transform.name + " has a DropBreadcrumbs behavior attached, but no breadcrumb pool was found.");
            }
        }
    }


    void Update()
    {
        dropTimer += Time.deltaTime;
        if (dropTimer > dropTimeout)
        {
            dropTimer = 0;
            GameObject crumb = breadcrumbPool.GetFromPool();
            crumb.transform.position = transform.position + dropPositionOffset;
            crumb.transform.rotation = transform.rotation;

            spawnedCrumbs.Enqueue(crumb);
            if (spawnedCrumbs.Count > trailSize)
                breadcrumbPool.ReturnToPool(spawnedCrumbs.Dequeue());
        }
    }
}
