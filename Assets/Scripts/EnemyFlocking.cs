using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyFlocking : MonoBehaviour
{
    public float separationRadius = 1.5f;
    public float separationForce = 1.0f;
    public LayerMask enemyLayer;

    private AIPath aiPath;
    private Transform player;

    void Awake()
    {
        aiPath = GetComponent<AIPath>();
        player = GameObject.FindGameObjectWithTag("Player").transform; // Or assign via inspector
    }

    void Update()
    {
        if (aiPath.canMove)
        {
            Vector2 separationOffset = ComputeSeparationOffset();
            Vector3 flockedDestination = player.position + (Vector3)separationOffset;

            aiPath.destination = flockedDestination;
        }
    }

    Vector2 ComputeSeparationOffset()
    {
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, separationRadius, enemyLayer);
        Vector2 separation = Vector2.zero;
        int count = 0;

        foreach (Collider2D neighbor in neighbors)
        {
            if (neighbor.gameObject == gameObject) continue;

            Vector2 diff = (Vector2)(transform.position - neighbor.transform.position);
            float distance = diff.magnitude;

            if (distance < 0.1f)
            {
                // Small random push to break perfect overlap
                Debug.Log("enemy too close to each other");
                diff = Random.insideUnitCircle.normalized * 2f;
                distance = 1f;
            }

            separation += diff.normalized / distance;
            count++;
        }

        if (count > 0)
        {
            separation /= count;
            separation *= separationForce;
        }

        return separation;
    }
}
