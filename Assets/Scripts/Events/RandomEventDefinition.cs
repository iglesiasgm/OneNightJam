using UnityEngine;

[CreateAssetMenu(
    fileName = "RandomEventDefinition",
    menuName = "Tram Game/Random Event Definition"
)]
public class RandomEventDefinition : ScriptableObject
{
    [Header("Identification")]
    [SerializeField] private string eventId;
    [SerializeField] private string displayName;

    [Header("Event")]
    [SerializeField] private GameObject eventPrefab;
    [SerializeField] private RandomEventSpawnType spawnType;

    [Header("Probability")]
    [Min(0f)]
    [SerializeField] private float weight = 1f;

    [Header("Economy")]
    [SerializeField] private int successReward = 2;
    [SerializeField] private int failurePenalty = 5;

    public string EventId => eventId;
    public string DisplayName => displayName;

    public GameObject EventPrefab =>
        eventPrefab;

    public RandomEventSpawnType SpawnType =>
        spawnType;

    public float Weight =>
        weight;

    public int SuccessReward =>
        successReward;

    public int FailurePenalty =>
        failurePenalty;
}