// ScriptableObject to define level configurations
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Level Config", menuName = "Dungeon/Level Configuration")]
public class LevelConfiguration : ScriptableObject
{
    [Header("Level Info")]
    public string levelName;
    public string levelDescription;

    [Header("Room Templates")]
    public RoomTemplate playerSpawnTemplate;
    public RoomTemplate bossRoomTemplate;
    public RoomTemplate exitRoomTemplate;
    public RoomTemplate[] roomTemplates;

    [Header("Tileset")]
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

    [Header("Generation Settings")]
    public int roomCount = 10;
    public Vector2Int gridSize = new(100, 100);
    public int roomPadding = 2;
    public int margin = 5;

    [Header("Visual Theme")]
    public Color ambientLightColor = Color.white;
    public Material backgroundMaterial;
    public AudioClip ambientMusic;
}