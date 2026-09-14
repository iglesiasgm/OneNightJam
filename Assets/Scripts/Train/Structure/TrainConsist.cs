using System.Collections.Generic;
using UnityEngine;

public class TrainConsist : MonoBehaviour
{
    [SerializeField] private CabinFollower leader;
    [Tooltip("Orden: el primero es el más cercano a la cabina, el último el más atrás.")]
    [SerializeField] private List<WagonFollower> wagons = new List<WagonFollower>();

    // LateUpdate asegura que la cabina ya actualizó su posición este frame
    // antes de que los vagones calculen la suya, sin depender del Script Execution Order.
    private void LateUpdate()
    {
        if (leader == null) return;

        float leaderDistance = leader.GetDistanceTraveled();
        float cumulativeOffset = 0f;

        for (int i = 0; i < wagons.Count; i++)
        {
            var wagon = wagons[i];
            if (wagon == null) continue;

            cumulativeOffset += wagon.CouplingDistance;
            float targetDistance = leaderDistance - cumulativeOffset;
            wagon.UpdatePosition(targetDistance);
        }
    }

    public void CoupleWagon(WagonFollower wagon, int index = -1)
    {
        if (wagon == null || wagons.Contains(wagon)) return;

        if (index < 0 || index >= wagons.Count)
            wagons.Add(wagon);
        else
            wagons.Insert(index, wagon);
    }

    public void DecoupleWagon(WagonFollower wagon)
    {
        wagons.Remove(wagon);
    }

    public IReadOnlyList<WagonFollower> GetWagons() => wagons;
}