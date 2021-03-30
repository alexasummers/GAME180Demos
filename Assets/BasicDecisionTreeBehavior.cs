using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDecisionTreeBehavior : MonoBehaviour
{

    class DecisionNode {
        public string info;
        public DecisionNode yesChild, noChild;

        public bool isChoice { get { return info[0] == '?'; }}    

        public DecisionNode(string s) { info = s; }
    }


    public string scriptFileName = "basicDecisionTree.txt";

    private int LeadingTabs(string line) {
        int t = 0;
        for (; t < line.Length && line[t] == '\t'; t++) {}
        return t;
    }

    DecisionNode decisionRoot;

    SteeringSeek seek;
    SteeringPathFollowing pathFollowing;

    SteeringLookWhereYouAreGoing lookWhereYouAreGoing;
    SteeringFace face;

    SteeringVelocityMatch velocityMatch;

    public float reloadTime = 2.0f;
    private float reloadTimer = 0.0f;

    public float attackCooldownTime = 0.1f;
    private float attackCooldownTimer = 0.0f;

    public GameObject projectile;
    public Transform projectileSpawnPoint;

    void Awake() {

        seek = GetComponent<SteeringSeek>();
        pathFollowing = GetComponent<SteeringPathFollowing>();
        lookWhereYouAreGoing = GetComponent<SteeringLookWhereYouAreGoing>();
        face = GetComponent<SteeringFace>();
        velocityMatch = GetComponent<SteeringVelocityMatch>();

        // load it up
        string text = Resources.Load<TextAsset>(scriptFileName).text;
        string[] lines = text.Replace("\r", "").Split('\n');

        Stack<DecisionNode> treeBuilder = new Stack<DecisionNode>();
        int tabDepth = LeadingTabs(lines[0]);
        decisionRoot = new DecisionNode(lines[0].Trim());
        treeBuilder.Push(decisionRoot);

        for (int i = 1; i < lines.Length; i++) {
            int currentTabs = LeadingTabs(lines[i]);
            if (currentTabs == tabDepth + 1) {
                treeBuilder.Peek().yesChild = new DecisionNode(lines[i].Trim());
                print("Node " + treeBuilder.Peek().info + " gets left child " + lines[i].Trim());
                treeBuilder.Push(treeBuilder.Peek().yesChild);
            }
            if (currentTabs == tabDepth) {
                // right child
                treeBuilder.Pop();
                treeBuilder.Peek().noChild = new DecisionNode(lines[i].Trim());
                print("Node " + treeBuilder.Peek().info + " gets right child " + lines[i].Trim());
                treeBuilder.Push(treeBuilder.Peek().noChild);
            }
            if (currentTabs < tabDepth) {
                // pop until we have a node with a missing left or right child (should only be right) or we run out of nodes to pop
                treeBuilder.Pop();
                while (treeBuilder.Count > 0 && treeBuilder.Peek().noChild != null) treeBuilder.Pop();
                if (treeBuilder.Count > 0) {
                    treeBuilder.Peek().noChild = new DecisionNode(lines[i].Trim());
                    treeBuilder.Push(treeBuilder.Peek().noChild);
                }
            }
            tabDepth = currentTabs;
        }

        // DFS debug
        DebugNode(decisionRoot);

    }

    void DebugNode(DecisionNode node) {
        if (!node.isChoice)
            print("Node [" + node.info + "] is an action request");
        else {
            print("Node [" + node.info + "] has children [" + node.yesChild.info + "] and [" + node.noChild.info + "]");
            DebugNode(node.yesChild);
            DebugNode(node.noChild);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ammo = maxAmmo;
    }

    // Update is called once per frame
    void Update()
    {
        // run the decision tree every frame
        DecisionNode currentNode = decisionRoot;
        while (currentNode.isChoice) {
            currentNode = Evaluate(currentNode);
        }

        if (attackCooldownTimer > 0) attackCooldownTimer -= Time.deltaTime;

        currentAction = currentNode.info;
        switch (currentAction.ToLower()) {
            case "reload":
                if (reloadTimer < reloadTime) {
                    reloadTimer += Time.deltaTime;
                    if (reloadTimer >= reloadTime) {
                        ammo = maxAmmo;
                        reloadTimer = 0;
                    }
                }
                break;
            case "attackplayer":
                pathFollowing.enabled = false;
                seek.enabled = false;
                lookWhereYouAreGoing.enabled = false;
                face.enabled = true;
                velocityMatch.enabled = true;

                if (attackCooldownTimer <= 0) {
                    // attack the player (spawn a projectile)
                    if (ammo > 0) {
                        attackCooldownTimer = attackCooldownTime;
                        ammo--;
                        GameObject p = GameObject.Instantiate(projectile);
                        p.transform.position = projectileSpawnPoint.position;
                        p.transform.forward = transform.forward;
                    }
                }
                break;
            case "pursueplayer":
                face.enabled = true;
                seek.enabled = true;
                velocityMatch.enabled = false;
                lookWhereYouAreGoing.enabled = false;
                pathFollowing.enabled = false;
                break;
            case "patrol":
                face.enabled = false;
                seek.enabled = false;
                velocityMatch.enabled = false;
                lookWhereYouAreGoing.enabled = true;
                pathFollowing.enabled = true;
                break;
        }

    }

    public string currentAction;

    DecisionNode Evaluate(DecisionNode node) {
        if (node == null) return null;
        if (!node.isChoice) return node;

        string toParse = node.info.StartsWith("?") ? node.info.Substring(1) : node.info;
        string[] parts = toParse.Split(' ');
        switch (parts[0].ToLower()) {
            case "player.visible":
                // arc check
                if (Vector3.Angle(transform.forward, player.position - transform.position) > 60) return node.noChild;
                RaycastHit info;
                if (!Physics.Raycast(transform.position, player.position - transform.position, out info, (player.position - transform.position).magnitude, sightBlockers.value)) return node.noChild;

                if (info.transform == player) return node.yesChild;
                return node.noChild;

            case "player.inrange":
                if ((player.position - transform.position).magnitude <= playerEngageDistance) return node.yesChild;
                return node.noChild;

            case "self.outofammo":
                if (ammo <= 0) return node.yesChild;
                return node.noChild;
        }

        throw new System.NotImplementedException("No handler for descriptor [" + node.info + "]");
    }

    public LayerMask sightBlockers;
    public Transform player;

    public float playerEngageDistance = 10f;

    public int maxAmmo = 5;
    public int ammo = 5;
}
