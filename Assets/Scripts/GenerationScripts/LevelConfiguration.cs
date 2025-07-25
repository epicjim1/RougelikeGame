// ScriptableObject to define level configurations
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Level Config", menuName = "Dungeon/Level Configuration")]
public class LevelConfiguration : ScriptableObject
{
    [Header("Level Info")]
    public string levelName;
    public string levelDescription;

    [Header("Room Templates")]
    public RoomTemplate playerSpawnTemplate;
    public RoomTemplate bossRoomTemplate;
    public RoomTemplate keyRoomTemplate;
    public RoomTemplate[] roomTemplates;

    [Header("Tileset")]
    public TileBase collisionTile;
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase leftWallTopTile;
    public TileBase rightWallTopTile;
    public TileBase topWallTopTile;
    public TileBase bottomWallTopTile;
    public TileBase bottomLeftWallTile;
    public TileBase topLeftWallTile;
    public TileBase topLeftCornerTopTile;
    public TileBase bottomRightWallTile;
    public TileBase bottomRightWallTopTile;
    public TileBase topRightWallTopTile;
    public TileBase topRightWallTopSideTile;

    [Header("Enemy Spawning")]
    public List<EnemySpawnSet> enemySpawnSet;
    public int minEnemiesPerRoom = 1;
    public int maxEnemiesPerRoom = 3;
    //[Range(0, 1)]
    public float corridorEnemyChance = 0.05f;

    [Header("Chests")]
    public GameObject chest;
    public float[] chestSpawnWeights;
    //[Range(0f, 1f)]
    public float chestSpawnChancePerRoom = 0.3f;

    [Header("Generation Settings")]
    public int roomCount = 10;
    public Vector2Int gridSize = new(100, 100);
    public int roomPadding = 2;
    public int margin = 5;

    [Header("Visual Theme")]
    public Color ambientLightColor = Color.white;
    public AudioSource audioSource;
    public AudioClip ambientMusic;
}

[System.Serializable]
public class EnemySpawnSet
{
    public GameObject[] enemyPrefabs;
    public float[] enemySpawnWeights;
}