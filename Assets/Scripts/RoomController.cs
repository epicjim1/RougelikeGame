using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Needed for .Count

public enum RoomType
{
    SpawnEnemiesInside,
    BossRoom,
    NeedKeyToEnter
}

public class RoomController : MonoBehaviour
{
    public RoomType roomType = RoomType.SpawnEnemiesInside;
    public SpikeController associatedSpikeTrap;

    public Transform[] spawnPoints;
    public GameObject[] enemyPrefabs;
    public DoorController doorController;

    private bool roomIsActive = false;
    private List<EnemyController> enemiesInRoom = new();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !roomIsActive && (roomType == RoomType.SpawnEnemiesInside || roomType == RoomType.BossRoom))
        {
            roomIsActive = true;
            associatedSpikeTrap.ExtendSpikes();

            if (roomType == RoomType.SpawnEnemiesInside)
            {
                foreach (Transform point in spawnPoints)
                {
                    GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                    GameObject enemy = Instantiate(enemyPrefab, point.position, Quaternion.identity);
                    EnemyController controller = enemy.GetComponent<EnemyController>();
                    controller.roomController = this;
                    enemiesInRoom.Add(controller);

                    Debug.Log(enemiesInRoom);
                }
            }
            else if (roomType == RoomType.BossRoom)
            {
                doorController.myRenderer.sprite = doorController.closedDoor;
                SpawnBoss();
            }
        }
    }

    private void Update()
    {
        
    }

    public void OnEnemyDefeated(EnemyController defeatedEnemy)
    {
        if (enemiesInRoom.Contains(defeatedEnemy))
        {
            enemiesInRoom.Remove(defeatedEnemy);
        }

        // Check if all enemies are gone
        if (roomIsActive && enemiesInRoom.Count == 0)
        {
            associatedSpikeTrap.RetractSpikes();
            this.enabled = false;
        }
    }

    private void SpawnBoss()
    {
        Debug.Log("Spawn boss");
    }
}