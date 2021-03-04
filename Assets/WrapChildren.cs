using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrapChildren : MonoBehaviour
{
    void Update()
    {
        // need to be able to convert local coordinates to world coordinates and vice versa.
        float width = transform.lossyScale.x, height = transform.lossyScale.y;

        // origin in world coordinates
        Vector3 origin = transform.position + new Vector3(-width / 2, 0, -height / 2);

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            // get local coordinates via difference between self and origin in world space
            Vector3 localPosition = child.position - origin;

            if (localPosition.x < 0 || localPosition.x > width)
                localPosition.x -= Mathf.Floor(localPosition.x / width) * width;

            if (localPosition.z < 0 || localPosition.z > height)
                localPosition.z -= Mathf.Floor(localPosition.z / height) * height;

            child.position = origin + localPosition;
        }
    }
}
