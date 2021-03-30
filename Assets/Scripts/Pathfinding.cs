using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pathfinding : MonoBehaviour
{
    public enum PathfindingState {
        IdentifyingBestOpenNode,
        IdentifyingConnections,
        UpdatingNodeLists,
        Done
    }
    public PathfindingState currentStep = PathfindingState.Done;

    public bool runAllAtOnce = false;

    public float gizmoSphereSize = 2f;

    Graph graph;

    private class PathfindingEntry {
        public Node node;
        public float costSoFar;
        public float heuristicValue;
        public PathfindingEntry predecessor;

        public PathfindingEntry(Node node, float costSoFar, float heuristicValue, PathfindingEntry predecessor) {
            this.node = node;
            this.costSoFar = costSoFar;
            this.heuristicValue = heuristicValue;
            this.predecessor = predecessor;
        }

    }

    private Dictionary<PathfindingEntry, Node> openList = new Dictionary<PathfindingEntry, Node>();
    private Dictionary<Node, PathfindingEntry> pathfindingEntries = new Dictionary<Node, PathfindingEntry>();

    public Transform player, target, pathOutput;

    public bool localizeAllPathNodes = false;

    public bool smoothPath = false;

    // Start is called before the first frame update
    void Start()
    {
        graph = GetComponent<GeneratePathfindingGraph>().graph;
        SetPathToFind(player.position, target.position);
    }


    PathfindingEntry bestNode;
    List<WeightedGraphConnection> connections = new List<WeightedGraphConnection>();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            // the target will be placed now; we can set the new path to find.
            SetPathToFind(player.position, target.position);
        }

        System.DateTime start = System.DateTime.Now;
        bool canReportTime = currentStep != PathfindingState.Done;

        bool stepDone = false;
        while (!stepDone || runAllAtOnce) {
            switch (currentStep) {
                case PathfindingState.Done:
                    if (runAllAtOnce && canReportTime) {
                        print((System.DateTime.Now - start).TotalMilliseconds + " ms to find path");
                    }

                    


                    return;
                case PathfindingState.IdentifyingBestOpenNode:
                    // look at open list to find which has the lowest total-estimated-cost
                    // (we'll do this the bad way for now)

                    // find everything on the open list and find the smallest one we have.
                    PathfindingEntry best = null;
                    foreach (PathfindingEntry p in openList.Keys) {
                        if (best == null || p.heuristicValue + p.costSoFar < best.heuristicValue + best.costSoFar) {
                            best = p;
                        }
                    }
                    bestNode = best;
                    stepDone = true;

                    if (best.node == targetNode) {
                        print("Total fill: " + pathfindingEntries.Count);
                        currentStep = PathfindingState.Done;

                        // Localize and configure the pathOutput transform with suitable children
                        // start with pathfindingEntries
                        PathfindingEntry currentEntry = pathfindingEntries[targetNode];
                        List<Node> pathNodes = new List<Node>();
                        while (currentEntry != null) {
                            pathNodes.Add(currentEntry.node);
                            currentEntry = pathfindingEntries[currentEntry.node].predecessor; // why did I make the predecessor a pathfinding entry? Weird.
                        }
                        pathNodes.Reverse();

                        List<Vector3> waypoints = new List<Vector3>();
                        waypoints.Add(player.position);

                        for (int i = 0; i < pathNodes.Count; i++) {
                            if ((i > 0 && i < pathNodes.Count - 1) || localizeAllPathNodes) {
                                waypoints.Add(pathNodes[i].worldPosition);
                            }
                        }
                        waypoints.Add(target.position);


                        if (smoothPath) SmoothPath(waypoints);

                        // Are we there?
                        for (int i = 0; i < pathOutput.childCount; i++) {
                            Destroy(pathOutput.GetChild(i).gameObject);
                        }

                        for (int i = 0; i < waypoints.Count; i++) {
                            GameObject g = new GameObject("waypoint " + i.ToString());
                            g.transform.position = waypoints[i];
                            g.transform.parent = pathOutput;
                        }

                    } else {
                        currentStep = PathfindingState.IdentifyingConnections;
                    }

                    break;
                case PathfindingState.IdentifyingConnections:
                    // The best is chosen; grab all its connections into a list
                    connections.Clear();
                    for (int i = 0; i < graph.connections[bestNode.node.logicalPosition].Count; i++) {
                        connections.Add(graph.connections[bestNode.node.logicalPosition][i]);
                    }
                    stepDone = true;
                    currentStep = PathfindingState.UpdatingNodeLists;
                    break;
                case PathfindingState.UpdatingNodeLists:
                    // Evaluate each connection:
                    // new nodes need a heuristic value
                    // non-new nodes need to know if there are improvements
                    // closed nodes that are improved need to go back on the open list

                    for (int i = 0; i < connections.Count; i++) {
                        Node farEnd = connections[i].destination;

                        if (pathfindingEntries.ContainsKey(farEnd)) {
                            // how are we doing?
                            PathfindingEntry existing = pathfindingEntries[farEnd];
                            // heuristic value will be the same
                            if (existing.costSoFar > bestNode.costSoFar + connections[i].cost) {
                                // improvement!
                                existing.costSoFar = bestNode.costSoFar + connections[i].cost;
                                existing.predecessor = bestNode;

                                if (!openList.ContainsKey(existing)) {
                                    openList.Add(existing, existing.node);
                                }
                            }
                        } else {
                            // new node
                            PathfindingEntry newEntry = new PathfindingEntry(farEnd, bestNode.costSoFar + connections[i].cost, GetHeuristic(farEnd, targetNode), bestNode);
                            pathfindingEntries.Add(farEnd, newEntry);
                            openList.Add(newEntry, farEnd);

                            // // how did something get into the open list but not be in pathfinding entries?
                            // // Is that right?
                            // if (openList.ContainsKey(newEntry)) {
                            //     print("???");

                            //     bool found = false;
                            //     foreach (PathfindingEntry wtf in openList.Keys) {
                            //         if (wtf == newEntry || wtf.GetHashCode() == newEntry.GetHashCode()) {
                            //             found = true;
                            //             print("?!?!?!?!?");
                            //         }

                            //         if (wtf.CompareTo(newEntry) == 0) {
                            //             print("aha");
                            //         }
                            //     }
                            //     if (!found) print("SERIOUSLY *W*T*F*");

                            //     openList[newEntry] = farEnd;

                            // } else {
                            // }
                        }
                    }

                    // remove best from open list
                    int priorCount = openList.Count;
                    openList.Remove(bestNode);
                    if (openList.Count == priorCount) {
                        print("Removal shenanigans");
                    }

                    currentStep = PathfindingState.IdentifyingBestOpenNode;
                    stepDone = true;
                    break;
            }
        }

    }

    public void SmoothPath(List<Vector3> waypoints) {
        
        if (waypoints.Count == 2) return;
        
        List<Vector3> smoothed = new List<Vector3>();
        smoothed.Add(waypoints[0]);
        for (int i = 2; i < waypoints.Count - 1; i++) {
            Vector3 f = smoothed[smoothed.Count - 1] + Vector3.up, t = waypoints[i] + Vector3.up;
            Vector3 m = t - f;
            if (Physics.Raycast(f, m.normalized, m.magnitude, graph.playerWalls)) {
                smoothed.Add(waypoints[i - 1]);
            }
        }
        smoothed.Add(waypoints[waypoints.Count - 1]);

        waypoints.Clear();
        for (int i = 0; i < smoothed.Count; i++) waypoints.Add(smoothed[i]);
        
    }

    private Node sourceNode, targetNode;
    public void SetPathToFind(Vector3 sourcePosition, Vector3 targetPosition) {
        // how do we quantize a world position with this generation?
        sourceNode = graph.Quantize(sourcePosition);
        targetNode = graph.Quantize(targetPosition);

        openList.Clear();
        pathfindingEntries.Clear();

        PathfindingEntry firstEntry = new PathfindingEntry(sourceNode, 0, GetHeuristic(sourceNode, targetNode), null);

        pathfindingEntries.Add(sourceNode, firstEntry);
        openList.Add(firstEntry, sourceNode);

        currentStep = PathfindingState.IdentifyingBestOpenNode;
    }


    public enum HeuristicType {
        Euclidean,
        Manhattan,
        Zero
    }
    public HeuristicType heuristicType = HeuristicType.Euclidean;

    float GetHeuristic(Node source, Node target) {

        switch (heuristicType) {
            case HeuristicType.Euclidean:
                return (target.worldPosition - source.worldPosition).magnitude;
            case HeuristicType.Manhattan:
                return Mathf.Abs(target.worldPosition.x - source.worldPosition.x) + Mathf.Abs(target.worldPosition.z - source.worldPosition.z);
            case HeuristicType.Zero:
                return 0;
            default:
                throw new System.NotImplementedException("Unknown heuristic value " + heuristicType);
        }


    }

    void OnDrawGizmos() {

        Color c = Gizmos.color;

        if (graph != null) {
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            Gizmos.DrawCube(graph.Quantize(player.position).worldPosition, new Vector3(8,1, 8));
        }

        // color all pathfinding nodes magenta...
        Gizmos.color = Color.magenta;
        foreach (Node k in pathfindingEntries.Keys) {
            Gizmos.DrawSphere(k.worldPosition, gizmoSphereSize);
        }

        // then color all open nodes yellow.
        Gizmos.color = Color.yellow;
        foreach (PathfindingEntry k in openList.Keys) {
            Gizmos.DrawSphere(k.node.worldPosition, gizmoSphereSize);
        }

        switch (currentStep) {
            case PathfindingState.IdentifyingBestOpenNode:  // just finished updating the node list, no other data here
                break;
            case PathfindingState.IdentifyingConnections:   // just finished identifying the best node
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(bestNode.node.worldPosition, gizmoSphereSize);

                Gizmos.color = Color.white;
                PathfindingEntry prev = bestNode;
                while (prev != null) {
                    if (prev.predecessor != null) {
                        Gizmos.DrawLine(prev.node.worldPosition, prev.predecessor.node.worldPosition);
                    }
                    prev = prev.predecessor;
                }

                // now draw the heuristic
                switch (heuristicType) {
                    case HeuristicType.Euclidean:
                        Gizmos.DrawLine(bestNode.node.worldPosition, targetNode.worldPosition);
                        break;
                    case HeuristicType.Manhattan:
                        Vector3 w1 = bestNode.node.worldPosition;
                        w1.z = targetNode.worldPosition.z;
                        Gizmos.DrawLine(bestNode.node.worldPosition, w1);
                        Gizmos.DrawLine(w1, targetNode.worldPosition);
                        break;
                }

                break;
            case PathfindingState.UpdatingNodeLists:    // just finished identifying connections
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(bestNode.node.worldPosition, gizmoSphereSize);

                Gizmos.color = Color.red;
                for (int i = 0; i < connections.Count; i++) {
                    Gizmos.DrawLine(connections[i].source.worldPosition, connections[i].destination.worldPosition);
                }
                break;
            case PathfindingState.Done:
                Gizmos.color = Color.white;
                if (targetNode != null) {
                    PathfindingEntry t = pathfindingEntries[targetNode];
                    while (t != null) {
                        if (t.predecessor != null) {
                            Gizmos.DrawLine(t.node.worldPosition, t.predecessor.node.worldPosition);
                        }
                        t = t.predecessor;
                    }
                }
                break;
        }


        // keep the source green
        if (sourceNode != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(sourceNode.worldPosition, gizmoSphereSize);
        }

        if (targetNode != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetNode.worldPosition, gizmoSphereSize);
        }



        Gizmos.color = c;

    }
}
