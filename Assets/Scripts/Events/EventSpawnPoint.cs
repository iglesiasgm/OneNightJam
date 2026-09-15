using UnityEngine;

public class EventSpawnPoint : MonoBehaviour
{
    [Header("Event Spawn")]
    [SerializeField]
    private RandomEventSpawnType spawnType;

    [SerializeField]
    private Transform spawnTransform;

    private bool occupied = false;

    public RandomEventSpawnType SpawnType =>
        spawnType;

    public bool IsOccupied =>
        occupied;

    public Transform SpawnTransform
    {
        get
        {
            if (spawnTransform != null)
                return spawnTransform;

            return transform;
        }
    }

    public void SetOccupied(
        bool value
    )
    {
        occupied = value;
    }
}