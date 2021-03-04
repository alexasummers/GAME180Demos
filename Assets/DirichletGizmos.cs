using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DirichletGizmos : MonoBehaviour
{
    // Start is called before the first frame update

    public float xOffset, zOffset, xMin, xMax, zMin, zMax, resolution = 1.0f;

    List<DirichletDomain> domains = new List<DirichletDomain>();

    List<Vector3> starts = new List<Vector3>();
    List<Vector3> ends = new List<Vector3>();

    Color[,] debugColors;

    public LayerMask playerWalls;

    public bool showEdges = false;

    float lastCalc = 0.0f;
    private int xc, zc;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        CheckRegenerate();
    }

    void CheckRegenerate() {
        List<DirichletDomain> domains = new List<DirichletDomain>();
        float c = 0f;
        for (int i = 0; i < transform.childCount; i++) {
            DirichletDomain domain = transform.GetChild(i).GetComponent<DirichletDomain>();
            if (domain != null) {
                domains.Add(domain);
                c += domain.transform.position.x * Mathf.PI + domain.transform.position.z * Mathf.Epsilon + (domain.matColor.r + domain.matColor.g * 64 + domain.matColor.b * 1024);
            }
        }

        if (c != lastCalc) {
            lastCalc = c;
            xc = Mathf.FloorToInt((xMax - xMin) / resolution);
            zc = Mathf.FloorToInt((zMax - zMin) / resolution);

            debugColors = new Color[xc, zc];
            for (int x = 0; x < xc; x++) {
                for (int z = 0; z < zc; z++) {
                    // which am I closest to?
                    float minDistance = -1;
                    DirichletDomain closestDomain = null;
                    Vector3 candidate = new Vector3(x * resolution, 1, z * resolution);
                    for (int d = 0; d < domains.Count; d++) {
                        float curDistance = Vector3.Distance(domains[d].transform.position, candidate);
                        if (minDistance < 0 || curDistance < minDistance) {
                            minDistance = curDistance;
                            closestDomain = domains[d];
                        }
                    }
                    debugColors[x, z] = closestDomain.matColor;
                }
            }

            starts.Clear();
            ends.Clear();
            for (int i = 0; i < domains.Count; i++) {
                for (int j = i; j < domains.Count; j++) {

                    if (!Physics.Raycast(domains[i].transform.position, (domains[j].transform.position - domains[i].transform.position).normalized, (domains[j].transform.position - domains[i].transform.position).magnitude, playerWalls.value)) {
                        starts.Add(domains[i].transform.position);
                        ends.Add(domains[j].transform.position);
                    }
                }
            }
            Debug.Log(domains.Count.ToString() + " nodes, " + starts.Count.ToString() + " connections");
        }
    }

    void OnDrawGizmos() {
        Color c = Gizmos.color;

        for (int x = 0; x < xc; x++) {
            for (int z = 0; z < zc; z++) {
                Gizmos.color = debugColors[x, z];
                Gizmos.DrawSphere(new Vector3(xOffset + x * resolution, 1, zOffset + z * resolution), resolution * 0.5f);
            }
        }

        if (showEdges) {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < starts.Count; i++) {
                Gizmos.DrawLine(starts[i], ends[i]);
            }
        }

        Gizmos.color = c;

    }
}
