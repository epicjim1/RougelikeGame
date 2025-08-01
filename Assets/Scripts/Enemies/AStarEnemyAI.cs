using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarEnemyAI : MonoBehaviour
{
    public EnemyData enemyData;

    private PlayerMovement playerMovement;
    private Transform playerTransform;
    private int currentHealth;
    private float lastAttackTime;
    //private bool hittingWall = false;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    [Header("A* Settings")]
    public float gridCellSize = 0.5f;
    public int gridWidth = 100;
    public int gridHeight = 100;
    public LayerMask obstacleMask;
    public float pathUpdateInterval = 0.2f;
    public float pathNodeReachedDistance = 0.3f;
    public float enemyRadius = 0.5f; // Half the width/height of the enemy
    public Vector2 radiusOffset = Vector2.zero;

    [Header("Debug")]
    public bool showGrid = false;
    public bool showPath = true;

    private AStarGrid grid;
    private List<Vector2> currentPath;
    private int currentPathIndex = 0;
    private float lastPathUpdate = 0f;
    private Vector2 lastPlayerPosition;

    void Start()
    {
        // Initialize A* grid
        Vector2 gridOrigin = (Vector2)transform.position - new Vector2(gridWidth * gridCellSize * 0.5f, gridHeight * gridCellSize * 0.5f);
        grid = new AStarGrid(gridWidth, gridHeight, gridCellSize, gridOrigin, obstacleMask, enemyRadius, radiusOffset);
        grid.ScanGrid();

        currentPath = new List<Vector2>();

        GameObject player = GameObject.FindWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
        playerTransform = player.transform;
        currentHealth = enemyData.maxHealth;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

        // Check if player is visible
        RaycastHit2D playerVisibilityHit = Physics2D.Raycast(transform.position, directionToPlayer, distance, obstacleMask);
        bool playerVisible = playerVisibilityHit.collider == null;

        Debug.DrawRay(transform.position, directionToPlayer * distance, playerVisible ? Color.green : Color.red);

        if (distance < enemyData.attackRange && playerVisible)
        {
            // Attack
            animator.SetBool("running", false);
            if (Time.time > lastAttackTime + enemyData.attackCooldown)
            {
                if (enemyData.isRanged)
                    Shoot();
                else
                    MeleeAttack();
                lastAttackTime = Time.time;
            }
        }
        else if (distance < enemyData.chaseRange)
        {
            animator.SetBool("running", true);

            // Update path if needed
            if (Time.time - lastPathUpdate > pathUpdateInterval ||
                Vector2.Distance(lastPlayerPosition, playerTransform.position) > gridCellSize)
            {
                UpdatePath();
                lastPathUpdate = Time.time;
                lastPlayerPosition = playerTransform.position;
            }

            // Follow current path
            FollowPath();
        }
        else
        {
            animator.SetBool("running", false);
        }
    }

    void UpdatePath()
    {
        Vector2 startPos = transform.position;
        Vector2 targetPos = playerTransform.position;

        currentPath = grid.FindPath(startPos, targetPos);
        currentPathIndex = 0;

        if (currentPath != null && currentPath.Count > 1)
        {
            // Remove the first node if it's too close (we're already there)
            if (Vector2.Distance(startPos, currentPath[0]) < pathNodeReachedDistance)
            {
                currentPathIndex = 1;
            }
        }
    }

    void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
        {
            return;
        }

        Vector2 targetNode = currentPath[currentPathIndex];
        Vector2 direction = (targetNode - (Vector2)transform.position).normalized;
        float distanceToNode = Vector2.Distance(transform.position, targetNode);

        // Check if we've reached the current node
        if (distanceToNode < pathNodeReachedDistance)
        {
            currentPathIndex++;
            if (currentPathIndex < currentPath.Count)
            {
                targetNode = currentPath[currentPathIndex];
                direction = (targetNode - (Vector2)transform.position).normalized;
            }
            else
            {
                // Reached the end of the path
                return;
            }
        }

        // Move towards the target node
        Vector2 newPosition = rb.position + direction * enemyData.moveSpeed * Time.deltaTime;
        rb.MovePosition(newPosition);

        // Update sprite direction
        if (direction.x < 0)
            spriteRenderer.flipX = true;
        else if (direction.x > 0)
            spriteRenderer.flipX = false;
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection ranges
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.chaseRange);

        // Draw enemy size
        Vector2 collisionCenter = (Vector2)transform.position + radiusOffset;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(collisionCenter, enemyRadius);

        //Draw offset indicator
        if (radiusOffset != Vector2.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, collisionCenter);
            Gizmos.DrawWireSphere(collisionCenter, 0.1f);
        }

        // Draw grid
        if (showGrid && grid != null)
        {
            grid.DrawGrid();
        }

        // Draw current path
        if (showPath && currentPath != null && currentPath.Count > 1)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }

            // Highlight current target node
            if (currentPathIndex < currentPath.Count)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(currentPath[currentPathIndex], 0.2f);
            }
        }
    }

    void MeleeAttack()
    {
        animator.SetTrigger("attack");
        Debug.Log($"{enemyData.enemyName} performs melee attack!");
        // Deal damage to playerTransform here
        if (!playerMovement.getIsDashing())
        {
            playerMovement.takeDamage(enemyData.damage);

            // Knockback direction from enemy to player
            Vector2 knockbackDir = (playerTransform.position - transform.position).normalized;

            // Knockback force
            float knockbackForce = enemyData.knockbackStrength; // Adjust this value as needed

            // Apply knockback to the player
            playerMovement.ApplyKnockback(knockbackDir, knockbackForce);
        }
    }

    void Shoot()
    {
        if (enemyData.projectilePrefab == null) return;

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        GameObject proj = Instantiate(enemyData.projectilePrefab, transform.position, Quaternion.identity);
        proj.GetComponent<Rigidbody2D>().linearVelocity = direction * 5f;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}

// A* Node class
public class AStarNode
{
    public Vector2 worldPosition;
    public int gridX;
    public int gridY;
    public bool walkable;

    public int gCost; // Distance from start
    public int hCost; // Distance to target
    public int fCost => gCost + hCost; // Total cost

    public AStarNode parent;

    public AStarNode(bool walkable, Vector2 worldPos, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPos;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}

// A* Grid class
public class AStarGrid
{
    private AStarNode[,] grid;
    private int gridSizeX, gridSizeY;
    private float nodeSize;
    private Vector2 gridOrigin;
    private LayerMask obstacleMask;
    private float enemyRadius;
    private Vector2 radiusOffset;

    public AStarGrid(int width, int height, float cellSize, Vector2 origin, LayerMask obstacles, float enemySize, Vector2 offset)
    {
        gridSizeX = width;
        gridSizeY = height;
        nodeSize = cellSize;
        gridOrigin = origin;
        obstacleMask = obstacles;
        enemyRadius = enemySize;
        radiusOffset = offset;

        grid = new AStarNode[gridSizeX, gridSizeY];
    }

    public void ScanGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = gridOrigin + new Vector2(x * nodeSize + nodeSize * 0.5f, y * nodeSize + nodeSize * 0.5f);
                bool walkable = IsNodeWalkable(worldPoint);
                grid[x, y] = new AStarNode(walkable, worldPoint, x, y);
            }
        }
    }

    private bool IsNodeWalkable(Vector2 worldPoint)
    {
        // Apply the radius offset to the collision center
        Vector2 collisionCenter = worldPoint + radiusOffset;

        // Check if the enemy (with its radius) can fit at this position
        // We check multiple points around the enemy's perimeter
        int checkPoints = 8; // Number of points to check around the enemy's perimeter

        for (int i = 0; i < checkPoints; i++)
        {
            float angle = (360f / checkPoints) * i * Mathf.Deg2Rad;
            Vector2 checkPoint = collisionCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * enemyRadius;

            if (Physics2D.OverlapCircle(checkPoint, nodeSize * 0.1f, obstacleMask))
            {
                return false;
            }
        }

        // Also check the center point
        return !Physics2D.OverlapCircle(collisionCenter, nodeSize * 0.1f, obstacleMask);
    }

    public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos)
    {
        AStarNode startNode = NodeFromWorldPoint(startPos);
        AStarNode targetNode = NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null || !startNode.walkable || !targetNode.walkable)
        {
            return null;
        }

        List<AStarNode> openSet = new List<AStarNode>();
        HashSet<AStarNode> closedSet = new HashSet<AStarNode>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            AStarNode currentNode = openSet[0];

            // Find node with lowest fCost
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Found target
            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            // Check neighbors
            foreach (AStarNode neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // No path found
    }

    List<Vector2> RetracePath(AStarNode startNode, AStarNode endNode)
    {
        List<AStarNode> path = new List<AStarNode>();
        AStarNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();

        List<Vector2> waypoints = new List<Vector2>();
        foreach (AStarNode node in path)
        {
            waypoints.Add(node.worldPosition);
        }

        return waypoints;
    }

    AStarNode NodeFromWorldPoint(Vector2 worldPosition)
    {
        Vector2 localPos = worldPosition - gridOrigin;
        int x = Mathf.RoundToInt(localPos.x / nodeSize - 0.5f);
        int y = Mathf.RoundToInt(localPos.y / nodeSize - 0.5f);

        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
        {
            return grid[x, y];
        }

        return null;
    }

    List<AStarNode> GetNeighbors(AStarNode node)
    {
        List<AStarNode> neighbors = new List<AStarNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbors;
    }

    int GetDistance(AStarNode nodeA, AStarNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public void DrawGrid()
    {
        if (grid != null)
        {
            foreach (AStarNode node in grid)
            {
                Gizmos.color = node.walkable ? Color.white : Color.red;
                Gizmos.DrawWireCube(node.worldPosition, Vector3.one * nodeSize);
            }
        }
    }
}