using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpikeController : MonoBehaviour
{
    [Header("Tilemap Settings")]
    public Tilemap spikeTilemap;
    public Tile spikeDown;
    public Tile spikeMidLower;
    public Tile spikeMidHigher;
    public Tile spikeUp;

    [Header("Spike Area Settings")]
    public Vector3Int areaStart = new Vector3Int(0, 0, 0); // bottom-left of spike area (in grid coordinates)
    public Vector2Int areaSize = new Vector2Int(3, 1);     // width x height

    [Header("Timing")]
    public float animationDelay = 0.1f;

    public bool isSpikesUp = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            ExtendSpikes(); // extend
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            RetractSpikes(); // retract
        }
    }

    public void ExtendSpikes()
    {
        if (!isSpikesUp)
        {
            isSpikesUp = true;
            StartCoroutine(AnimateSpikes(spikeDown, spikeMidLower, spikeMidHigher, spikeUp));
        }
    }

    public void RetractSpikes()
    {
        if (isSpikesUp)
        {
            isSpikesUp = false;
            StartCoroutine(AnimateSpikes(spikeUp, spikeMidHigher, spikeMidLower, spikeDown));
        }
    }

    private IEnumerator AnimateSpikes(Tile initial, Tile midFirst, Tile midSecond, Tile final)
    {
        SetTilesInArea(initial);
        yield return new WaitForSeconds(animationDelay);

        SetTilesInArea(midFirst);
        yield return new WaitForSeconds(animationDelay);

        SetTilesInArea(midSecond);
        yield return new WaitForSeconds(animationDelay);

        SetTilesInArea(final);
        spikeTilemap.RefreshAllTiles();
    }

    private void SetTilesInArea(Tile tileToSet)
    {
        for (int x = 0; x < areaSize.x; x++)
        {
            for (int y = 0; y < areaSize.y; y++)
            {
                Vector3Int pos = new Vector3Int(areaStart.x + x, areaStart.y + y, 0);
                if (spikeTilemap.HasTile(pos))
                {
                    spikeTilemap.SetTile(pos, tileToSet);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 center = spikeTilemap.CellToWorld(areaStart + new Vector3Int(areaSize.x / 2, areaSize.y / 2, 0));
        Vector3 size = new Vector3(areaSize.x, areaSize.y, 1) * spikeTilemap.cellSize.x;

        Gizmos.DrawWireCube(center, size);
    }
}
