using System.Collections.Generic;
using UnityEngine;

public class RoadCrossingController : MonoBehaviour
{
    [SerializeField] private string trainTag = "Train";

    private readonly HashSet<Collider> trainColliders =
        new HashSet<Collider>();

    public bool IsClosed =>
        trainColliders.Count > 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsTrain(other))
            return;

        trainColliders.Add(other);

        Debug.Log("Cruce cerrado: tren aproximándose.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsTrain(other))
            return;

        trainColliders.Remove(other);

        if (trainColliders.Count == 0)
        {
            Debug.Log("Cruce abierto.");
        }
    }

    private bool IsTrain(Collider other)
    {
        return other.transform.root.CompareTag(
            trainTag
        );
    }

    private void OnDisable()
    {
        trainColliders.Clear();
    }
}