using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DirichletDomain : MonoBehaviour
{

    public Color matColor;
    private MeshRenderer myRenderer;
    // Start is called before the first frame update
    void Start()
    {
        myRenderer = GetComponent<MeshRenderer>();
        matColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        matColor.a = 0.5f;
    }

    void Update() 
    {
        myRenderer.material.color = new Color(matColor.r, matColor.g, matColor.b, 0.5f);
    }

}
