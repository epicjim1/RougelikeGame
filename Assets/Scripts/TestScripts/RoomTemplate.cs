using UnityEngine;

[System.Serializable]
public class RoomTemplate
{
    public GameObject roomPrefab;
    public Vector2Int size = new Vector2Int(16, 16);
    public int maxNumberGenerated = 100;
    //public bool topDoor, rightDoor, bottomDoor, leftDoor;
}
