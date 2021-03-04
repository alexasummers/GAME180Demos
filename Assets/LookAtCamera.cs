using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Camera owningCamera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (owningCamera != null && owningCamera.gameObject.activeInHierarchy)
        {
            transform.forward = owningCamera.transform.forward;
        }
        else
        {
            // Find one
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i].gameObject.activeInHierarchy)
                {
                    owningCamera = cameras[i];
                }
            }
        }
    }
}
