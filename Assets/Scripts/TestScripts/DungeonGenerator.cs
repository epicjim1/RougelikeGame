using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    private HashSet<Vector2Int> corridorTiles = new();

    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public Tilemap wallTopTilemap;

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

    public RoomTemplate playerSpawnTemplate;
    public RoomTemplate bossRoomTemplate;
    public RoomTemplate exitRoomTemplate;
    public RoomTemplate[] roomTemplates;
    public int roomCount = 10;
    public Vector2Int gridSize = new(100, 100);
    public int roomPadding = 2;
    int margin = 5;

    public GameObject player;
    private GameObject playerSpawnRoomInstance;

    private bool[,] occupiedTiles;
    private bool[,] roomTiles;
    private List<RoomInstance> placedRooms = new();

    void Start()
    {
        occupiedTiles = new bool[gridSize.x, gridSize.y];
        roomTiles = new bool[gridSize.x, gridSize.y];
        //DrawDebugGrid();
        GenerateRooms();
        ConnectRoomsWithPaths();
        if (playerSpawnRoomInstance != null)
        {
            // Example: assume you have a "player" GameObject reference:
            player.transform.position = playerSpawnRoomInstance.transform.position + new Vector3(4, 4, 0);
            // playerSpawnOffset is optional if you want to offset within the room
        }
        else
        {
            Debug.LogError("Player spawn room was not placed!");
        }
    }

    void GenerateRooms()
    {
        int attempts = 0;
        int maxAttempts = 1000;

        // Track how many of each template have been placed
        Dictionary<RoomTemplate, int> templateCounts = new Dictionary<RoomTemplate, int>();

        // Initialize counts
        foreach (RoomTemplate template in roomTemplates)
        {
            templateCounts[template] = 0;
        }

        // === Phase 1: Place special rooms first ===
        PlaceSpecialRoom(playerSpawnTemplate, ref playerSpawnRoomInstance);  // Player start room
        templateCounts[playerSpawnTemplate] = 0;
        templateCounts[playerSpawnTemplate]++;

        //PlaceSpecialRoom(bossRoomTemplate, ref _);     // Boss room
        //templateCounts[bossRoomTemplate]++;

        //PlaceSpecialRoom(exitRoomTemplate, ref _);     // Exit room
        //templateCounts[exitRoomTemplate]++;

        while (placedRooms.Count < roomCount && attempts < maxAttempts)
        {
            attempts++;
            RoomTemplate template = roomTemplates[Random.Range(0, roomTemplates.Length)];

            // Check if we can still place this template
            if (templateCounts[template] >= template.maxNumberGenerated)
            {
                continue; // Skip this attempt and try again
            }

            Vector2Int size = template.size;

            Vector2Int pos = new Vector2Int(
                Random.Range(margin, gridSize.x - margin - size.x),
                Random.Range(margin, gridSize.y - margin - size.y)
            );

            if (!CanPlaceRoom(pos, size)) continue;

            Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
            GameObject roomObj = Instantiate(template.roomPrefab, worldPos, Quaternion.identity);

            placedRooms.Add(new RoomInstance
            {
                gridPos = pos,
                template = template,
                instance = roomObj
            });

            // Update count
            templateCounts[template]++;

            MarkTilesOccupied(pos, size);
            MarkRoomDoorsOpen(roomObj.GetComponent<Room>());
            DrawRoomDebug(placedRooms.Last());
        }
    }

    void PlaceSpecialRoom(RoomTemplate template, ref GameObject outRoomInstance, Vector2Int? forcedPos = null)
    {
        int attempts = 0;
        int maxAttempts = 100;

        while (attempts < maxAttempts)
        {
            attempts++;

            Vector2Int size = template.size;

            Vector2Int pos = forcedPos ?? new Vector2Int(
                Random.Range(margin, gridSize.x - margin - size.x),
                Random.Range(margin, gridSize.y - margin - size.y)
            );

            if (!CanPlaceRoom(pos, size))
            {
                continue;
            }

            Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
            GameObject roomObj = Instantiate(template.roomPrefab, worldPos, Quaternion.identity);

            placedRooms.Add(new RoomInstance
            {
                gridPos = pos,
                template = template,
                instance = roomObj
            });

            // Mark tiles
            MarkTilesOccupied(pos, size);
            MarkRoomDoorsOpen(roomObj.GetComponent<Room>());
            DrawRoomDebug(placedRooms.Last());

            outRoomInstance = roomObj;

            return; // success
        }

        Debug.LogWarning($"Failed to place special room: {template.roomPrefab.name}");
    }

    void DrawRoomDebug(RoomInstance room)
    {
        Vector3 pos = room.instance.transform.position;
        Vector2Int size = room.template.size;
        float w = size.x;
        float h = size.y;

        Vector3 bottomLeft = pos;
        Vector3 bottomRight = pos + new Vector3(w, 0, 0);
        Vector3 topRight = pos + new Vector3(w, h, 0);
        Vector3 topLeft = pos + new Vector3(0, h, 0);

        Debug.DrawLine(bottomLeft, bottomRight, Color.green, 30f);
        Debug.DrawLine(bottomRight, topRight, Color.green, 30f);
        Debug.DrawLine(topRight, topLeft, Color.green, 30f);
        Debug.DrawLine(topLeft, bottomLeft, Color.green, 30f);
    }

    bool IsOccupied(Vector2Int pos, Vector2Int size)
    {
        foreach (var room in placedRooms)
        {
            if (Vector2Int.Distance(room.gridPos, pos) < 2) return true;
        }
        return false;
    }

    bool CanPlaceRoom(Vector2Int pos, Vector2Int size)
    {
        Vector2Int start = new Vector2Int(Mathf.Max(pos.x - roomPadding, 0), Mathf.Max(pos.y - roomPadding, 0));
        Vector2Int end = new Vector2Int(Mathf.Min(pos.x + size.x + roomPadding, gridSize.x), Mathf.Min(pos.y + size.y + roomPadding, gridSize.y));

        for (int x = start.x; x < end.x; x++)
        {
            for (int y = start.y; y < end.y; y++)
            {
                if (occupiedTiles[x, y])
                    return false;
            }
        }

        return true;
    }

    void MarkTilesOccupied(Vector2Int pos, Vector2Int size)
    {
        for (int x = pos.x; x < pos.x + size.x; x++)
        {
            for (int y = pos.y; y < pos.y + size.y; y++)
            {
                occupiedTiles[x, y] = true;
                roomTiles[x, y] = true;
            }
        }
    }

    void MarkRoomDoorsOpen(Room roomScript)
    {
        foreach (var pair in roomScript.doorPositions)
        {
            Vector3 worldPos = pair.Value.position;
            Vector2Int gridPos = GridPosFromWorld(worldPos);

            // Clear a 3x3 area around each door for corridor access
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dy = -2; dy <= 2; dy++)
                {
                    Vector2Int clearPos = new Vector2Int(gridPos.x + dx, gridPos.y + dy);

                    if (clearPos.x >= 0 && clearPos.x < gridSize.x &&
                        clearPos.y >= 0 && clearPos.y < gridSize.y)
                    {
                        occupiedTiles[clearPos.x, clearPos.y] = false;
                    }
                }
            }
        }
    }

    Vector2Int GridPosFromWorld(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
    }

    void ConnectRoomsWithPaths()
    {
        corridorTiles.Clear();
        HashSet<Transform> connectedDoors = new HashSet<Transform>();

        for (int i = 1; i < placedRooms.Count; i++)
        {
            GameObject roomA = placedRooms[i - 1].instance;
            GameObject roomB = placedRooms[i].instance;

            Room scriptA = roomA.GetComponent<Room>();
            Room scriptB = roomB.GetComponent<Room>();

            Transform doorA = GetClosestDoorTransform(scriptA, roomB.transform.position);
            Transform doorB = GetClosestDoorTransform(scriptB, roomA.transform.position);

            CreateCorridorBetweenDoorsShared(doorA.position, doorB.position);

            connectedDoors.Add(doorA);
            connectedDoors.Add(doorB);
        }

        // Second pass: connect any remaining unconnected doors
        foreach (var room in placedRooms)
        {
            Room roomScript = room.instance.GetComponent<Room>();

            foreach (var doorPair in roomScript.doorPositions)
            {
                if (!connectedDoors.Contains(doorPair.Value))
                {
                    // Find nearest connected door
                    Vector3 nearestConnectedDoor = FindNearestConnectedDoor(doorPair.Value.position, connectedDoors);
                    CreateCorridorBetweenDoorsShared(doorPair.Value.position, nearestConnectedDoor);
                    connectedDoors.Add(doorPair.Value);
                }
            }
        }

        // Draw all corridors at once
        DrawAllCorridors();
        PlaceCorridorWalls();
    }

    Transform GetClosestDoorTransform(Room roomScript, Vector3 targetPosition)
    {
        Transform closest = null;
        float minDist = float.MaxValue;

        foreach (var pair in roomScript.doorPositions)
        {
            float dist = Vector3.Distance(pair.Value.position, targetPosition);
            if (dist < minDist)
            {
                minDist = dist;
                closest = pair.Value;
            }
        }

        return closest;
    }

    Vector3 FindNearestConnectedDoor(Vector3 doorPos, HashSet<Transform> connectedDoors)
    {
        Vector3 nearest = Vector3.zero;
        float minDist = float.MaxValue;

        foreach (var door in connectedDoors)
        {
            float dist = Vector3.Distance(doorPos, door.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = door.position;
            }
        }

        return nearest;
    }

    Vector3 GetClosestDoor(Room roomScript, Vector3 targetPosition)
    {
        Transform closest = null;
        float minDist = float.MaxValue;

        foreach (var pair in roomScript.doorPositions)
        {
            float dist = Vector3.Distance(pair.Value.position, targetPosition);
            if (dist < minDist)
            {
                minDist = dist;
                closest = pair.Value;
            }
        }

        return closest.position;
    }

    void CreateCorridorBetweenDoorsShared(Vector3 doorAWorld, Vector3 doorBWorld)
    {
        Vector2Int start = GridPosFromWorld(doorAWorld);
        Vector2Int goal = GridPosFromWorld(doorBWorld);

        List<Vector2Int> path = FindPath(start, goal);

        foreach (var p in path)
        {
            // Add 3x3 area to shared corridor set
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2Int tile = new Vector2Int(p.x + dx, p.y + dy);

                    if (tile.x >= 0 && tile.x < gridSize.x && tile.y >= 0 && tile.y < gridSize.y)
                    {
                        corridorTiles.Add(tile);
                    }
                }
            }
        }
    }

    void DrawAllCorridors()
    {
        foreach (var tile in corridorTiles)
        {
            if (!roomTiles[tile.x, tile.y]) // Don't overwrite rooms
            {
                Vector3 worldP = new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0);
                Debug.DrawLine(worldP, worldP + Vector3.one * 0.01f, Color.red, 30f);
                floorTilemap.SetTile(new Vector3Int(tile.x, tile.y, 0), floorTile);
            }
        }
    }

    void PlaceCorridorWalls()
    {
        HashSet<Vector2Int> wallPositions = new();
        HashSet<Vector2Int> leftWallTopPositions = new();
        HashSet<Vector2Int> rightWallTopPositions = new();
        HashSet<Vector2Int> topWallPositions = new();
        HashSet<Vector2Int> bottomWallPositions = new();
        HashSet<Vector2Int> leftWallPositions = new();

        foreach (var corridorTile in corridorTiles)
        {
            // Skip if this corridor tile is inside a room
           if (roomTiles[corridorTile.x, corridorTile.y])
                continue;

            // Check all 4 directions around corridor tile
            Vector2Int[] directions = {
            new Vector2Int(0, 1),   // Above
            new Vector2Int(0, -1),  // Below  
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 0)    // Right
        };

            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int wallPos = corridorTile + directions[i];

                // Make sure wall position is within bounds
                if (wallPos.x < 0 || wallPos.x >= gridSize.x ||
                    wallPos.y < 0 || wallPos.y >= gridSize.y)
                    continue;

                // Don't place walls on corridor tiles or room tiles
                if (corridorTiles.Contains(wallPos) || roomTiles[wallPos.x, wallPos.y])
                    continue;

                switch (i)
                {
                    case 0: // Above corridor
                        wallPositions.Add(wallPos);
                        topWallPositions.Add(wallPos);
                        break;
                    case 1: // Below corridor
                        wallPositions.Add(wallPos);
                        bottomWallPositions.Add(wallPos);
                        break;
                    case 2: // Left of corridor
                        leftWallTopPositions.Add(wallPos);
                        leftWallPositions.Add(wallPos);
                        break;
                    case 3: // Right of corridor
                            // For right wall top, place one tile to the left (on the floor)
                        rightWallTopPositions.Add(corridorTile);
                        break;
                        Vector2Int rightWallTopPos = wallPos + new Vector2Int(-1, 0);
                        if (rightWallTopPos.x >= 0 && rightWallTopPos.x < gridSize.x &&
                            rightWallTopPos.y >= 0 && rightWallTopPos.y < gridSize.y &&
                            corridorTiles.Contains(rightWallTopPos)) // Make sure it's on a corridor tile
                        {
                            rightWallTopPositions.Add(rightWallTopPos);
                        }
                        break;
                }
            }
        }

        // Place all wall tiles
        foreach (var pos in wallPositions)
        {
            wallTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), wallTile);
            if (floorTilemap.GetTile(new Vector3Int(pos.x + 1, pos.y, 0)) == floorTile)
            {
                wallTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), bottomRightWallTile);

                if (floorTilemap.GetTile(new Vector3Int(pos.x, pos.y - 1, 0)) == floorTile &&
                    floorTilemap.GetTile(new Vector3Int(pos.x - 1, pos.y, 0)) == floorTile &&
                    floorTilemap.GetTile(new Vector3Int(pos.x, pos.y + 1, 0)) != floorTile)
                {
                    wallTilemap.SetTile(new Vector3Int(pos.x - 1, pos.y, 0), bottomLeftWallTile);
                }
            }
            /*if (wallTilemap.GetTile(new Vector3Int(pos.x - 1, pos.y, 0)) == null)
            {
                wallTilemap.SetTile(new Vector3Int(pos.x - 1, pos.y, 0), bottomLeftWallTile);
            }*/
            if (floorTilemap.GetTile(new Vector3Int(pos.x, pos.y + 1, 0)) == floorTile)
            {
                if (floorTilemap.GetTile(new Vector3Int(pos.x - 1, pos.y + 1, 0)) != floorTile)
                {
                    wallTilemap.SetTile(new Vector3Int(pos.x - 1, pos.y, 0), bottomLeftWallTile);
                }
                if (floorTilemap.GetTile(new Vector3Int(pos.x + 1, pos.y + 1, 0)) != floorTile)
                {
                    wallTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), bottomRightWallTile);
                }
            }
        }

        // Place left wall top tiles
        foreach (var pos in leftWallTopPositions)
        {
            if (wallTilemap.GetTile(new Vector3Int(pos.x, pos.y, 0)) != (wallTile || bottomRightWallTile))
            {
                wallTopTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), leftWallTopTile);
            }
            else
            {
                if (floorTilemap.GetTile(new Vector3Int(pos.x, pos.y - 1, 0)) != floorTile)
                {
                    wallTopTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), leftWallTopTile);
                }
            }

            if (wallTilemap.GetTile(new Vector3Int(pos.x + 1, pos.y + 1, 0)) == (wallTile || bottomRightWallTile))
            {
                wallTopTilemap.SetTile(new Vector3Int(pos.x, pos.y + 1, 0), leftWallTopTile);

                if (wallTopTilemap.GetTile(new Vector3Int(pos.x, pos.y + 2, 0)) == null)
                {
                    Debug.Log("topleft corner from left trigggers");
                    wallTopTilemap.SetTile(new Vector3Int(pos.x, pos.y + 2, 0), topLeftCornerTopTile);
                }
            }
            if (wallTilemap.GetTile(new Vector3Int(pos.x + 1, pos.y, 0)) == wallTile)
            {
                wallTopTilemap.SetTile(new Vector3Int(pos.x, pos.y + 1, 0), topLeftCornerTopTile);
            }
            if (wallTilemap.GetTile(new Vector3Int(pos.x, pos.y - 1, 0)) == (wallTile || bottomRightWallTile) &&
                wallTilemap.GetTile(new Vector3Int(pos.x - 1, pos.y - 1, 0)) == wallTile &&
                wallTilemap.GetTile(new Vector3Int(pos.x + 1, pos.y - 1, 0)) == wallTile)
            {
                Debug.Log("asdasdasdas");
                wallTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), leftWallTopTile);
            }
        }

        // Place right wall top tiles (same tile as left, just positioned on right side)
        foreach (var pos in rightWallTopPositions)
        {


            Vector2Int right = pos + new Vector2Int(1, 0);
            Vector2Int up = pos + new Vector2Int(0, 1);
            if (wallTilemap.GetTile(new Vector3Int(right.x, right.y, 0)) == (wallTile || bottomRightWallTile))
            {
                if (wallTopTilemap.GetTile(new Vector3Int(up.x, up.y, 0)) == null && floorTilemap.GetTile(new Vector3Int(pos.x + 1, pos.y - 1, 0)) != floorTile)
                {
                    wallTopTilemap.SetTile(new Vector3Int(up.x, up.y, 0), topLeftCornerTopTile);
                }
                else
                {
                    wallTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), bottomLeftWallTile);
                    if (wallTilemap.GetTile(new Vector3Int(pos.x, pos.y + 1, 0)) == wallTile)
                    {
                        wallTopTilemap.SetTile(new Vector3Int(pos.x, pos.y + 1, 0), rightWallTopTile);
                    }
                    if (floorTilemap.GetTile(new Vector3Int(up.x + 1, up.y, 0)) == floorTile)
                    {
                        wallTopTilemap.SetTile(new Vector3Int(up.x, up.y, 0), topLeftCornerTopTile);
                    }
                    continue;
                }
            }

            if (wallTilemap.GetTile(new Vector3Int(pos.x, pos.y - 1, 0)) == (wallTile || bottomRightWallTile) &&
                wallTilemap.GetTile(new Vector3Int(pos.x - 1, pos.y - 1, 0)) == wallTile &&
                wallTilemap.GetTile(new Vector3Int(pos.x + 1, pos.y - 1, 0)) == wallTile)
            {
                Debug.Log("asdasdasdas");
                wallTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), rightWallTopTile);
                continue;
            }

            if (wallTilemap.GetTile(new Vector3Int(up.x, up.y, 0)) == wallTile)
            {
                wallTopTilemap.SetTile(new Vector3Int(up.x, up.y, 0), rightWallTopTile);
            }

            if (floorTilemap.GetTile(new Vector3Int(pos.x + 1, pos.y - 1, 0)) != floorTile)
            {
                wallTopTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), rightWallTopTile);
            }
        }

        // Place top wall top tiles (one tile up from top walls)
        foreach (var pos in topWallPositions)
        {
            Vector2Int topWallTopPos = pos + new Vector2Int(0, 1);
            if (topWallTopPos.x >= 0 && topWallTopPos.x < gridSize.x &&
                topWallTopPos.y >= 0 && topWallTopPos.y < gridSize.y)
            {
                wallTopTilemap.SetTile(new Vector3Int(topWallTopPos.x, topWallTopPos.y, 0), topWallTopTile);
                /*if (wallTopTilemap.GetTile(new Vector3Int(topWallTopPos.x, topWallTopPos.y, 0)) == leftWallTopTile &&
                    wallTopTilemap.GetTile(new Vector3Int(topWallTopPos.x + 1, topWallTopPos.y, 0)) == topWallTopTile &&
                    floorTilemap.GetTile(new Vector3Int(topWallTopPos.x + 1, topWallTopPos.y, 0)) == floorTile)
                {
                    Debug.Log("top wall tile detected left side topwalltile");
                    wallTilemap.SetTile(new Vector3Int(topWallTopPos.x, topWallTopPos.y, 0), topWallTopTile);
                }
                else
                {
                    wallTopTilemap.SetTile(new Vector3Int(topWallTopPos.x, topWallTopPos.y, 0), topWallTopTile);
                }*/

                /*if (wallTilemap.GetTile(new Vector3Int(topWallTopPos.x - 1, topWallTopPos.y, 0)) == wallTile)
                {
                    wallTopTilemap.SetTile(new Vector3Int(topWallTopPos.x - 1, topWallTopPos.y, 0), rightWallTopTile);
                }*/

                if (floorTilemap.GetTile(new Vector3Int(topWallTopPos.x + 1, topWallTopPos.y - 1, 0)) == floorTile)
                {
                    if (wallTopTilemap.GetTile(new Vector3Int(topWallTopPos.x, topWallTopPos.y - 1, 0)) == leftWallTopTile ||
                        floorTilemap.GetTile(new Vector3Int(topWallTopPos.x, topWallTopPos.y, 0)) == floorTile)
                    {
                        wallTopTilemap.SetTile(new Vector3Int(topWallTopPos.x, topWallTopPos.y, 0), topRightWallTopTile);
                        if (wallTopTilemap.GetTile(new Vector3Int(topWallTopPos.x - 1, topWallTopPos.y - 1, 0)) == leftWallTopTile)
                        {
                            wallTopTilemap.SetTile(new Vector3Int(topWallTopPos.x - 1, topWallTopPos.y, 0), topLeftCornerTopTile);
                        }
                    }
                    else
                    {
                        wallTopTilemap.SetTile(new Vector3Int(topWallTopPos.x, topWallTopPos.y, 0), bottomRightWallTopTile);
                    }
                }

                if (wallTopTilemap.GetTile(new Vector3Int(topWallTopPos.x, topWallTopPos.y - 1, 0)) == leftWallTopTile ||
                    wallTopTilemap.GetTile(new Vector3Int(topWallTopPos.x, topWallTopPos.y - 1, 0)) == rightWallTopTile)
                {
                    wallTopTilemap.SetTile(new Vector3Int(topWallTopPos.x, topWallTopPos.y, 0), topRightWallTopTile);
                }
            }
        }

        // Place bottom wall top tiles (one tile up from bottom walls)
        foreach (var pos in bottomWallPositions)
        {
            Vector2Int bottomWallTopPos = pos + new Vector2Int(0, 1);
            if (bottomWallTopPos.x >= 0 && bottomWallTopPos.x < gridSize.x &&
                bottomWallTopPos.y >= 0 && bottomWallTopPos.y < gridSize.y)
            {
                if (wallTopTilemap.GetTile(new Vector3Int(bottomWallTopPos.x, bottomWallTopPos.y, 0)) == rightWallTopTile)
                {
                    wallTopTilemap.SetTile(new Vector3Int(bottomWallTopPos.x, bottomWallTopPos.y, 0), bottomRightWallTopTile);
                }
                else
                {
                    wallTopTilemap.SetTile(new Vector3Int(bottomWallTopPos.x, bottomWallTopPos.y, 0), bottomWallTopTile);
                }

                if (floorTilemap.GetTile(new Vector3Int(bottomWallTopPos.x + 1, bottomWallTopPos.y - 1, 0)) == floorTile)
                {
                    if (wallTopTilemap.GetTile(new Vector3Int(bottomWallTopPos.x, bottomWallTopPos.y - 1, 0)) == leftWallTopTile ||
                        wallTopTilemap.GetTile(new Vector3Int(bottomWallTopPos.x, bottomWallTopPos.y - 1, 0)) == bottomRightWallTopTile ||
                        floorTilemap.GetTile(new Vector3Int(bottomWallTopPos.x, bottomWallTopPos.y, 0)) == floorTile)
                    {
                        wallTopTilemap.SetTile(new Vector3Int(bottomWallTopPos.x, bottomWallTopPos.y, 0), topRightWallTopTile);
                        if (wallTopTilemap.GetTile(new Vector3Int(bottomWallTopPos.x - 1, bottomWallTopPos.y - 1, 0)) == leftWallTopTile)
                        {
                            wallTopTilemap.SetTile(new Vector3Int(bottomWallTopPos.x - 1, bottomWallTopPos.y, 0), topLeftCornerTopTile);
                        }
                    }
                    else
                    {
                        wallTopTilemap.SetTile(new Vector3Int(bottomWallTopPos.x, bottomWallTopPos.y, 0), bottomRightWallTopTile);
                    }
                }
            }
        }

        // Detect and place corner tiles
        //PlaceCornerTiles(leftWallPositions, topWallPositions, bottomWallPositions);
    }

    void PlaceCornerTiles(HashSet<Vector2Int> leftWallPositions, HashSet<Vector2Int> topWallPositions, HashSet<Vector2Int> bottomWallPositions)
    {
        // Create right wall positions
        HashSet<Vector2Int> rightWallPositions = new();
        foreach (var corridorTile in corridorTiles)
        {
            if (occupiedTiles[corridorTile.x, corridorTile.y])
                continue;

            Vector2Int rightPos = corridorTile + new Vector2Int(1, 0);
            if (rightPos.x >= 0 && rightPos.x < gridSize.x &&
                rightPos.y >= 0 && rightPos.y < gridSize.y &&
                !corridorTiles.Contains(rightPos) && !occupiedTiles[rightPos.x, rightPos.y])
            {
                rightWallPositions.Add(rightPos);
            }
        }

        //Debug.Log($"Wall counts - Left: {leftWallPositions.Count}, Right: {rightWallPositions.Count}, Top: {topWallPositions.Count}, Bottom: {bottomWallPositions.Count}");

        // Bottom-left corners: Look for positions where we have a left wall above and a bottom wall to the right
        foreach (var bottomWall in bottomWallPositions)
        {
            Vector2Int leftOfBottom = bottomWall + new Vector2Int(-1, 0);
            Vector2Int aboveLeft = leftOfBottom + new Vector2Int(0, 1);

            if (leftWallPositions.Contains(aboveLeft))
            {
                //Debug.Log($"Bottom-left corner at {leftOfBottom}");

                if (wallTilemap.GetTile(new Vector3Int(leftOfBottom.x, leftOfBottom.y + 1, 0)) != topWallTopTile)
                {
                    wallTilemap.SetTile(new Vector3Int(leftOfBottom.x, leftOfBottom.y, 0), bottomLeftWallTile);
                }
            }
        }

        // Bottom-right corners: Look for positions where we have a right wall above and a bottom wall to the left
        foreach (var bottomWall in bottomWallPositions)
        {
            Vector2Int rightOfBottom = bottomWall + new Vector2Int(1, 0);
            Vector2Int aboveRight = rightOfBottom + new Vector2Int(0, 1);

            if (rightWallPositions.Contains(aboveRight))
            {
                // Bottom-right wall goes one to the left of where it was
                Vector2Int bottomRightPos = rightOfBottom + new Vector2Int(-1, 0);
                //Debug.Log($"Bottom-right corner at {bottomRightPos}");
                wallTilemap.SetTile(new Vector3Int(bottomRightPos.x, bottomRightPos.y, 0), bottomRightWallTile);

                // Bottom-right wall top goes right above the right wall
                Vector2Int cornerTopPos = rightOfBottom + new Vector2Int(-1, 1);
                //Debug.Log($"Bottom-right wall top at {cornerTopPos}");
                wallTopTilemap.SetTile(new Vector3Int(cornerTopPos.x, cornerTopPos.y, 0), bottomRightWallTopTile);
            }
        }

        // Top-left corners: Look for positions where we have a left wall below and a top wall to the right
        foreach (var topWall in topWallPositions)
        {
            Vector2Int leftOfTop = topWall + new Vector2Int(-1, 0);
            Vector2Int belowLeft = leftOfTop + new Vector2Int(0, -1);

            if (leftWallPositions.Contains(belowLeft))
            {
                // Top-left wall top goes one lower than before
                Vector2Int topLeftPos = leftOfTop;
                if (topLeftPos.x >= 0 && topLeftPos.x < gridSize.x &&
                    topLeftPos.y >= 0 && topLeftPos.y < gridSize.y &&
                    !corridorTiles.Contains(topLeftPos) && !occupiedTiles[topLeftPos.x, topLeftPos.y])
                {
                    //Debug.Log($"Top-left wall top at {topLeftPos}");
                    wallTopTilemap.SetTile(new Vector3Int(topLeftPos.x, topLeftPos.y, 0), topLeftWallTile);
                }

                // Top-left corner top goes where the old top-left was
                Vector2Int cornerTopPos = leftOfTop + new Vector2Int(0, 1);
                if (cornerTopPos.x >= 0 && cornerTopPos.x < gridSize.x &&
                    cornerTopPos.y >= 0 && cornerTopPos.y < gridSize.y &&
                    !corridorTiles.Contains(cornerTopPos) && !occupiedTiles[cornerTopPos.x, cornerTopPos.y])
                {
                    //Debug.Log($"Top-left corner top at {cornerTopPos}");
                    wallTopTilemap.SetTile(new Vector3Int(cornerTopPos.x, cornerTopPos.y, 0), topLeftCornerTopTile);
                }
            }
        }

        // Top-right corners: Look for positions where we have a right wall below and a top wall to the left
        foreach (var topWall in topWallPositions)
        {
            Vector2Int rightOfTop = topWall + new Vector2Int(1, 0);
            Vector2Int belowRight = rightOfTop + new Vector2Int(0, -1);

            if (rightWallPositions.Contains(belowRight))
            {
                // Top-right wall top goes one to the left
                Vector2Int topRightPos = rightOfTop + new Vector2Int(-1, 1);
                if (topRightPos.x >= 0 && topRightPos.x < gridSize.x &&
                    topRightPos.y >= 0 && topRightPos.y < gridSize.y &&
                    !corridorTiles.Contains(topRightPos) && !occupiedTiles[topRightPos.x, topRightPos.y])
                {
                    //Debug.Log($"Top-right wall top at {topRightPos}");
                    if (wallTopTilemap.GetTile(new Vector3Int(topRightPos.x, topRightPos.y + 1, 0)) != leftWallTopTile)
                    {
                        wallTopTilemap.SetTile(new Vector3Int(topRightPos.x, topRightPos.y, 0), topRightWallTopTile);
                    }
                }

                // Top-right wall top side goes right below the original position
                Vector2Int sidePos = rightOfTop + new Vector2Int(-1, 0);
                if (sidePos.x >= 0 && sidePos.x < gridSize.x &&
                    sidePos.y >= 0 && sidePos.y < gridSize.y &&
                    !corridorTiles.Contains(sidePos) && !occupiedTiles[sidePos.x, sidePos.y])
                {
                    //Debug.Log($"Top-right wall top side at {sidePos}");
                    wallTopTilemap.SetTile(new Vector3Int(sidePos.x, sidePos.y, 0), topRightWallTopSideTile);
                }
            }
        }

        // Handle inner corners after outer corners
        //PlaceInnerCornerTiles(leftWallPositions, rightWallPositions, topWallPositions, bottomWallPositions);
    }

    void CreateCorridorBetweenDoors(Vector3 doorAWorld, Vector3 doorBWorld)
    {
        Vector2Int start = GridPosFromWorld(doorAWorld);
        Vector2Int goal = GridPosFromWorld(doorBWorld);

        List<Vector2Int> path = FindPath(start, goal);

        foreach (var p in path)
        {
            // Corridor thickness = 3x3 around center tile
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2Int tile = new Vector2Int(p.x + dx, p.y + dy);

                    if (tile.x >= 0 && tile.x < gridSize.x && tile.y >= 0 && tile.y < gridSize.y)
                    {
                        // For now: Debug draw small box
                        Vector3 worldP = new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0);
                        Debug.DrawLine(worldP, worldP + Vector3.one * 0.01f, Color.red, 30f);

                        // OPTIONAL: Place tilemap floor tile here
                        floorTilemap.SetTile(new Vector3Int(tile.x, tile.y, 0), floorTile);
                    }
                }
            }
        }
    }

    void DrawDebugGrid()
    {
        for (int x = 0; x <= gridSize.x; x++)
        {
            Vector3 start = new Vector3(x, 0, 0);
            Vector3 end = new Vector3(x, gridSize.y, 0);
            Debug.DrawLine(start, end, Color.gray, 30f);
        }

        for (int y = 0; y <= gridSize.y; y++)
        {
            Vector3 start = new Vector3(0, y, 0);
            Vector3 end = new Vector3(gridSize.x, y, 0);
            Debug.DrawLine(start, end, Color.gray, 30f);
        }
    }

    List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        HashSet<Vector2Int> closedSet = new();
        PriorityQueue<Vector2Int> openSet = new();
        openSet.Enqueue(start, 0);

        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, int> gScore = new();
        gScore[start] = 0;

        while (openSet.Count > 0)
        {
            Vector2Int current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            closedSet.Add(current);

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor)) continue;
                if (IsBlocked(neighbor)) continue;

                int tentativeGScore = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;

                    int fScore = tentativeGScore + Heuristic(neighbor, goal);
                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore);
                }
            }
        }

        return new List<Vector2Int>(); // No path found
    }

    IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            Vector2Int neighbor = pos + dir;

            if (neighbor.x >= 0 && neighbor.x < gridSize.x && neighbor.y >= 0 && neighbor.y < gridSize.y)
                yield return neighbor;
        }
    }

    bool IsBlocked(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= gridSize.x || pos.y < 0 || pos.y >= gridSize.y)
            return true;

        // Check if a 3x3 area around this position would be blocked
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                Vector2Int checkPos = new Vector2Int(pos.x + dx, pos.y + dy);

                // Make sure we're within bounds
                if (checkPos.x < 0 || checkPos.x >= gridSize.x ||
                    checkPos.y < 0 || checkPos.y >= gridSize.y)
                    return true;

                // Check if this tile is occupied
                if (occupiedTiles[checkPos.x, checkPos.y])
                    return true;
            }
        }

        return false;
    }

    int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new();
        path.Add(current);

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}
