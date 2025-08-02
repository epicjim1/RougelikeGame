using UnityEngine;
using System.Collections.Generic;
using System.Linq;
//using System.Diagnostics; // Needed for .Count

public enum RoomType
{
    SpawnEnemiesInside,
    BossRoom,
    NeedKeyToEnter
}

public enum BossType
{
    ElementalGolem,
    FlyingDemon,
    Necromancer,
    Computer,
}

public class RoomController : MonoBehaviour
{
    public RoomType roomType = RoomType.SpawnEnemiesInside;
    public SpikeController associatedSpikeTrap;

    public Transform[] spawnPoints;
    public GameObject[] enemyPrefabs;
    public DoorController doorController;

    private bool roomIsActive = false;
    private List<GameObject> enemiesInRoom = new();

    public BossType bossType = BossType.ElementalGolem;
    public GameObject[] bosses;
    private GameObject bossInstance;

    private void Start()
    {
        if (roomType == RoomType.BossRoom)
        {
            SpawnBoss();
        }
    }

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
                    enemiesInRoom.Add(enemy);
                }
            }
            else if (roomType == RoomType.BossRoom)
            {
                doorController.myRenderer.sprite = doorController.closedDoor;
                doorController.myCollider.enabled = true;
                if (bossType == BossType.ElementalGolem)
                {
                    StartCoroutine(bossInstance.GetComponent<GolemBoss>().StartBossFight());
                }
                else if (bossType == BossType.FlyingDemon)
                {
                    //StartCoroutine(bossInstance.GetComponent<FlyingDemonBoss>().StartBossFight());
                }
            }
        }
    }

    private void Update()
    {
        
    }

    public void OnEnemyDefeated(GameObject defeatedEnemy)
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
        bossInstance = Instantiate(bosses[Random.Range(0, bosses.Length)], new Vector3((spawnPoints[3].position.x + spawnPoints[2].position.x) / 2, spawnPoints[3].position.y, 0), Quaternion.identity);
        enemiesInRoom.Add(bossInstance);

        if (bossType == BossType.ElementalGolem)
        {
            GolemBoss gb = bossInstance.GetComponent<GolemBoss>();
            gb.roomController = this;
            gb.corners = spawnPoints;
        }
        else if (bossType == BossType.FlyingDemon)
        {

        }
    }
}