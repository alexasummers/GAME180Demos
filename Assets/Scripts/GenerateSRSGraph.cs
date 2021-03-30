using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Uses Graph, 


public class SRSGraph : Graph {

    // inherits
    /*
    public Dictionary<Vector2Int, Node> nodes = new Dictionary<Vector2Int, Node>();
    public Dictionary<Vector2Int, List<WeightedGraphConnection>> connections = new Dictionary<Vector2Int, List<WeightedGraphConnection>>();
    public LayerMask playerWalls;
    */

    // Quantization will look different for an SRSGraph
    public override Node Quantize(Vector3 worldPosition) {
        return base.Quantize(worldPosition);
    }
}

public class GenerateSRSGraph : MonoBehaviour
{
    internal class NodeGenerationMetadata {
        public Node node;
        public float leftMaxDistance = 0, upMaxDistance = 0, rightMaxDistance = 0, downMaxDistance = 0;
        public float upLeftMaxDistance = 0, upRightMaxDistance = 0, downRightMaxDistance = 0, downLeftMaxDistance = 0;


        private bool isViable = true;
        public bool IsViable {
            get {
                return isViable;
            }
        }

        public float maximin = -1;
        public float Maximin {
            get {
                if (maximin < 0) {
                    List<float> fs = new List<float>();
                    fs.Add(leftMaxDistance);
                    fs.Add(upMaxDistance);
                    fs.Add(rightMaxDistance);
                    fs.Add(downMaxDistance);
                    fs.Add(upLeftMaxDistance);
                    fs.Add(upRightMaxDistance);
                    fs.Add(downRightMaxDistance);
                    fs.Add(downLeftMaxDistance);
                    fs.Sort();
                    maximin = fs[0];
                    print("Maximin for " + node.logicalPosition + " is " + maximin);
                }
                return maximin;
            }
        }

        internal class MaximinComparer : IComparer<NodeGenerationMetadata>
        {
            public int Compare(NodeGenerationMetadata x, NodeGenerationMetadata y)
            {
                float xMin = x.Maximin, yMin = y.Maximin;

                if (xMin == yMin) return 0;
                if (xMin < yMin) return 1;
                return -1;
            }

        }

    }

    public float resolution = 1f;
    public Transform rootTransform;

    public LayerMask playerWalls;

    public SRSGraph graph;

    internal NodeGenerationMetadata[,] generationIndex;
    internal int logicalXMax = 0, logicalYMax = 0; 


    public enum GenerationState {
        Initializing,
        GatheringMaximin,
        SelectingRegionCenter,
        SelectingExpansionDirection,
        ExpandingRegion
    }
    public GenerationState currentState = GenerationState.Initializing;


    void Awake()
    {
        print("SRS Graph generating");
        graph = new SRSGraph();
        graph.playerWalls = playerWalls;

        int xScale = Mathf.FloorToInt(rootTransform.localScale.x), 
            zScale = Mathf.FloorToInt(rootTransform.localScale.z);

        
        // connections have a source, a target, and a cost.
        for (float x = 0; x < xScale; x += resolution) {
            for (float z = 0; z < zScale; z += resolution) {
                Node n = new Node(new Vector3(x + resolution * 0.5f, 1, z + resolution * 0.5f) + rootTransform.position, new Vector2Int(Mathf.RoundToInt(x / resolution), Mathf.RoundToInt(z / resolution)));
                if (n.logicalPosition.x > logicalXMax) logicalXMax = n.logicalPosition.x;
                if (n.logicalPosition.y > logicalYMax) logicalYMax = n.logicalPosition.y;
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

        generationIndex = new NodeGenerationMetadata[logicalXMax + 1, logicalYMax + 1];
        for (int x = 0; x <= logicalXMax; x++)
        {
            for (int y = 0; y <= logicalYMax; y++)
            {
                generationIndex[x, y] = new NodeGenerationMetadata();
                generationIndex[x, y].node = graph.nodes[new Vector2Int(x, y)];
            }
        }
        currentState = GenerationState.GatheringMaximin;
    }


    int sortedNodeIndex = 0;
    List<NodeGenerationMetadata> sortedNodes;
    // Update is called once per frame
    void Update()
    {

        switch (currentState)
        {
            case GenerationState.SelectingRegionCenter:
                while (sortedNodeIndex < sortedNodes.Count && !sortedNodes[sortedNodeIndex].IsViable) { sortedNodeIndex++; }
                if (sortedNodeIndex < sortedNodes.Count) {
                    // new region center chosen
                    print("Region center starting at " + sortedNodes[sortedNodeIndex].node.logicalPosition);
                    print("Maximin is " + sortedNodes[sortedNodeIndex].Maximin);
                    currentState = GenerationState.SelectingExpansionDirection;
                } else {
                    // done with regional collection
                }
                break;
            case GenerationState.GatheringMaximin:
                // Iterate all nodes in generationIndex
                // update with directional limits in all four directions
                sortedNodes = new List<NodeGenerationMetadata>();
                for (int x = 0; x < logicalXMax; x++)
                {
                    for (int y = 0; y < logicalYMax; y++)
                    {
                        // distances
                        Vector3 nwp = generationIndex[x, y].node.worldPosition;
                        float upMaxDistance = (generationIndex[x, logicalYMax].node.worldPosition - nwp).magnitude,
                              downMaxDistance = (generationIndex[x, 0].node.worldPosition - nwp).magnitude,
                              leftMaxDistance = (generationIndex[0, y].node.worldPosition - nwp).magnitude,
                              rightMaxDistance = (generationIndex[logicalXMax, y].node.worldPosition - nwp).magnitude;

                        float upLeftMaxDistance = upMaxDistance < leftMaxDistance ? upMaxDistance : leftMaxDistance,
                              upRightMaxDistance = upMaxDistance < rightMaxDistance ? upMaxDistance : rightMaxDistance,
                              downLeftMaxDistance = downMaxDistance < leftMaxDistance ? downMaxDistance : leftMaxDistance,
                              downRightMaxDistance = downMaxDistance < rightMaxDistance ? downMaxDistance : rightMaxDistance;

                        upLeftMaxDistance = Mathf.Sqrt(upLeftMaxDistance * 2);
                        upRightMaxDistance = Mathf.Sqrt(upRightMaxDistance * 2);
                        downLeftMaxDistance = Mathf.Sqrt(downLeftMaxDistance * 2);
                        downRightMaxDistance = Mathf.Sqrt(downRightMaxDistance * 2);
                              
                        RaycastHit info;
                        if (Physics.Raycast(nwp, Vector3.forward, out info, upMaxDistance, playerWalls)) upMaxDistance = info.distance;
                        if (Physics.Raycast(nwp, Vector3.back, out info, downMaxDistance, playerWalls)) downMaxDistance = info.distance;
                        if (Physics.Raycast(nwp, Vector3.left, out info, leftMaxDistance, playerWalls)) leftMaxDistance = info.distance;
                        if (Physics.Raycast(nwp, Vector3.right, out info, rightMaxDistance, playerWalls)) rightMaxDistance = info.distance;

                        if (Physics.Raycast(nwp, Vector3.forward + Vector3.left, out info, upLeftMaxDistance, playerWalls)) upLeftMaxDistance = info.distance;
                        if (Physics.Raycast(nwp, Vector3.forward + Vector3.right, out info, upRightMaxDistance, playerWalls)) upRightMaxDistance = info.distance;
                        if (Physics.Raycast(nwp, Vector3.back + Vector3.left, out info, downLeftMaxDistance, playerWalls)) downLeftMaxDistance = info.distance;
                        if (Physics.Raycast(nwp, Vector3.back + Vector3.right, out info, downRightMaxDistance, playerWalls)) downRightMaxDistance = info.distance;

                        generationIndex[x, y].upMaxDistance = upMaxDistance;
                        generationIndex[x, y].downMaxDistance = downMaxDistance;
                        generationIndex[x, y].leftMaxDistance = leftMaxDistance;
                        generationIndex[x, y].rightMaxDistance = rightMaxDistance;

                        generationIndex[x, y].upLeftMaxDistance = upLeftMaxDistance;
                        generationIndex[x, y].upRightMaxDistance = upRightMaxDistance;
                        generationIndex[x, y].downRightMaxDistance = downRightMaxDistance;
                        generationIndex[x, y].downLeftMaxDistance = downLeftMaxDistance;
                        

                        sortedNodes.Add(generationIndex[x, y]);
                    }
                }

                // now, sort
                sortedNodes.Sort(new NodeGenerationMetadata.MaximinComparer());
                currentState = GenerationState.SelectingRegionCenter;

                // may need to invert
                break;
        }

    }

    void OnDrawGizmos() 
    {
        if (graph == null) return;

        Color c = Gizmos.color;
        if (currentState == GenerationState.SelectingRegionCenter || currentState == GenerationState.SelectingExpansionDirection) {

            for (int i = 0; i < sortedNodes.Count; i++) {
                float f = (float)i / (float)sortedNodes.Count;
                Gizmos.color = new Color(f, f, f, 1);
                Gizmos.DrawSphere(sortedNodes[i].node.worldPosition, resolution * 0.4f);
            }

        }

        Gizmos.color = c;
    }
}
