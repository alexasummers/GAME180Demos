using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToPlace : MonoBehaviour
{

    public LayerMask validLayers;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit info;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out info, 1000f, validLayers))
            {
                transform.position = info.point;
            }
        }
    }
}
