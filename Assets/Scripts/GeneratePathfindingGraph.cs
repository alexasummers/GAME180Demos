using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {
    public Vector3 worldPosition;
    public Vector2Int logicalPosition; // needed?

    public float heuristicValue = 0f;

    public Node(Vector3 world, Vector2Int logical) {
        worldPosition = world;
        logicalPosition = logical;
    }
}

public class WeightedGraphConnection {
    public float cost;
    public Node source, destination;

    public WeightedGraphConnection(Node s, Node d, float c) {
        source = s;
        destination = d;
        cost = c;
    }
}

public class Graph {
    public Dictionary<Vector2Int, Node> nodes = new Dictionary<Vector2Int, Node>();
    public Dictionary<Vector2Int, List<WeightedGraphConnection>> connections = new Dictionary<Vector2Int, List<WeightedGraphConnection>>();

    public LayerMask playerWalls;

    public virtual Node Quantize(Vector3 worldPosition) {
        Vector3 intended = worldPosition;
        intended.y = 1;

        // find the closest that we can see.
        Node closestVisible = null;
        float closestDistance = 0;
        foreach (Vector2Int k in nodes.Keys) {
            // visible?
            Vector3 toNode = nodes[k].worldPosition - intended;
            RaycastHit info;
            if (toNode.magnitude < 16) {
                if (!Physics.Raycast(intended, toNode.normalized, out info, closestVisible == null ? toNode.magnitude : closestDistance, playerWalls.value)) {
                    // no hit, so we can see it
                    if (closestVisible == null || toNode.magnitude < closestDistance) {
                        closestVisible = nodes[k];
                        closestDistance = toNode.magnitude;
                    }
                } else {
                    Transform t = info.transform;
                    if (t.gameObject.layer != LayerMask.NameToLayer("Obstacle")) {
                        UnityEngine.MonoBehaviour.print("Shenanigans; hit " + t.name);
                    }
                }
            }
        }

        if (closestVisible == null) {
            UnityEngine.MonoBehaviour.print("position " + worldPosition.ToString("F3") + " is not closest to any node..");
        }

        return closestVisible;

    }
}

public class GeneratePathfindingGraph : MonoBehaviour
{
    public float resolution = 1f;
    public Transform rootTransform;

    public LayerMask playerWalls;

    public Graph graph;

    void Awake()
    {
        graph = new Graph();
        graph.playerWalls = playerWalls;

        int xScale = Mathf.FloorToInt(rootTransform.localScale.x), 
            zScale = Mathf.FloorToInt(rootTransform.localScale.z);

        // automatically create nodes at each...
        // really we want nodes and connections, so...
        // each node...
        // needs outbound connections.

        // connections have a source, a target, and a cost.
        for (float x = 0; x < xScale; x += resolution) {
            for (float z = 0; z < zScale; z += resolution) {
                Node n = new Node(new Vector3(x + resolution * 0.5f, 1, z + resolution * 0.5f) + rootTransform.position, new Vector2Int(Mathf.RoundToInt(x / resolution), Mathf.RoundToInt(z / resolution)));
                graph.connections.Add(n.logicalPosition, new List<WeightedGraphConnection>());
                graph.nodes.Add(n.logicalPosition, n);
            }
        }

        foreach (Vector2Int n in graph.connections.Keys) {
            // eight possible connections.
            for (int xd = -1; xd <= 1; xd++) {
                for (int zd = -1; zd <= 1; zd++) {
                    if (xd != 0 || zd != 0) {
                        float d = (xd * zd == 0) ? 1 : 1.415f;
                        Vector3 world = graph.nodes[n].worldPosition;
                        if (!Physics.Raycast(world, new Vector3(xd, 0, zd), d * resolution, playerWalls)) {
                            // line of sight confirmed
                            Vector2Int lpt = n + new Vector2Int(xd, zd);
                            if (graph.connections.ContainsKey(lpt)) {
                                // existence confirmed
                                graph.connections[n].Add(new WeightedGraphConnection(graph.nodes[n], graph.nodes[lpt], Vector3.Distance(graph.nodes[n].worldPosition, graph.nodes[lpt].worldPosition)));
                            }
                        }
                    }
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos() 
    {
        if (graph == null) return;

        Color c = Gizmos.color;
        Gizmos.color = Color.cyan;

        foreach (Vector2Int n in graph.connections.Keys) {
            Gizmos.DrawSphere(graph.nodes[n].worldPosition, resolution / 5f);
            foreach (WeightedGraphConnection w in graph.connections[n]) {
                Gizmos.DrawLine(w.source.worldPosition, w.destination.worldPosition);
            }
        }


        Gizmos.color = c;
    }
}
