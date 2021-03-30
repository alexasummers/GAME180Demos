using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DirichletDomain : MonoBehaviour
{

    public Color matColor;
    public bool lockY = true;
    public float yLockValue = 23.8f;
    private MeshRenderer myRenderer;
    // Start is called before the first frame update
    void Start()
    {
        myRenderer = GetComponent<MeshRenderer>();
        matColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        matColor.a = 0.9f;
    }

    void Update() 
    {
        if (lockY && transform.position.y != yLockValue) {
            Vector3 p = transform.position;
            p.y = yLockValue;
            transform.position = p;
        }

        try {
            myRenderer.material.color = new Color(matColor.r, matColor.g, matColor.b, 0.9f);
        } catch {
            // Eating material leak errors. Yes, I want to set the properties of the material, not the shared material.
            // I don't want to change the shared material.
            // I am fine with "leaking" materials into the scene, because I'm going to do that anyway.
        }
    }

}
