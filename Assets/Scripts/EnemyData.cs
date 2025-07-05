using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int maxHealth;
    public float moveSpeed;
    public float chaseRange;
    public float attackRange;
    public float attackCooldown;
    public int damage;

    public bool isRanged;
    public float knockbackStrength;
    public GameObject projectilePrefab;
}
