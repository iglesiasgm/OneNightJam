using UnityEngine;

public class CrossingEntityHitbox : MonoBehaviour
{
    private CrossingEntityEvent owner;

    public void Initialize(
        CrossingEntityEvent eventOwner
    )
    {
        owner = eventOwner;
    }

    private void OnTriggerEnter(
        Collider other
    )
    {
        if (owner == null)
            return;

        if (
            !other.transform.root.CompareTag(
                "Train"
            )
        )
        {
            return;
        }

        owner.OnHitByTrain();
    }
}