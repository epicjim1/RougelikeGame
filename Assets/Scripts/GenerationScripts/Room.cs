using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public Dictionary<Vector2Int, Transform> doorPositions = new();

    void Awake()
    {
        // Populate doors dictionary with door direction vectors and their world positions
        doorPositions.Clear();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Door_"))
            {
                Vector2Int dir = DirectionFromName(child.name.Replace("Door_", ""));
                doorPositions[dir] = child;
            }
        }
    }

    Vector2Int DirectionFromName(string name)
    {
        return name switch
        {
            "Top" => Vector2Int.up,
            "Right" => Vector2Int.right,
            "Bottom" => Vector2Int.down,
            "Left" => Vector2Int.left,
            _ => Vector2Int.zero
        };
    }

    public bool HasDoor(Vector2Int dir) => doorPositions.ContainsKey(dir);
    public Transform GetDoor(Vector2Int dir) => doorPositions.TryGetValue(dir, out var door) ? door : null;
}
