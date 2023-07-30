using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupGenerator : MonoBehaviour
{
    public GameObject slowTimePowerupPrefab;
    public GameObject moreRailsPowerupPrefab;
    public GameObject moveStationsPowerupPrefab;

    public static float powerupHeight = 3.5f;

    public static Dictionary<PowerupSystem.PowerupType, GameObject> powerupPrefabMapping;

    public void GeneratePowerups(List<Tile> tiles)
    {
        powerupPrefabMapping = new Dictionary<PowerupSystem.PowerupType, GameObject>
        {
            { PowerupSystem.PowerupType.SlowTimePowerup, slowTimePowerupPrefab},
            { PowerupSystem.PowerupType.MoreRailsPowerup, moreRailsPowerupPrefab},
            { PowerupSystem.PowerupType.MoveStationPowerup, moveStationsPowerupPrefab},
        };

        foreach (Tile tile in tiles)
        {
            if (tile != null && tile.powerup != PowerupSystem.PowerupType.NoPowerup)
            {
                Vector3 position = tile.coordinates + new Vector3(0, tile.elevation * TileGenerator.elevationHeight + powerupHeight, 0);
                tile.powerupRef = Instantiate(powerupPrefabMapping[tile.powerup], position, Quaternion.identity, tile.objectParents[Tile.ObjectParent.Powerups].transform);
            }
        }
    }
}
