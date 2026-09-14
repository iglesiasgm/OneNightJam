using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Linq;

public class StationManager : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Transform[] stationPrefabsInScene;

    public class Station
    {
        public Transform transform;
        public float startDistance; // inicio del andén sobre el spline
        public float endDistance;   // fin del andén sobre el spline
    }

    public List<Station> Stations { get; private set; } = new List<Station>();

    private float splineLength;

    private void Awake()
    {
        splineLength = splineContainer.CalculateLength();

        foreach (var stationT in stationPrefabsInScene)
        {
            Bounds bounds = GetStationBounds(stationT);

            float distA = ProjectPointToSplineDistance(bounds.min);
            float distB = ProjectPointToSplineDistance(bounds.max);

            Stations.Add(new Station
            {
                transform = stationT,
                startDistance = Mathf.Min(distA, distB),
                endDistance = Mathf.Max(distA, distB)
            });
        }

        // Ordenar las estaciones por su posición a lo largo del recorrido
        Stations = Stations.OrderBy(s => s.startDistance).ToList();
    }

    private Bounds GetStationBounds(Transform stationT)
    {
        // Preferimos un Collider si existe (más prolijo y alineado al diseño del andén)
        var collider = stationT.GetComponentInChildren<Collider>();
        if (collider != null)
        {
            return collider.bounds;
        }

        // Si no hay collider, usamos los Renderers como fallback
        var renderers = stationT.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"La estación {stationT.name} no tiene Collider ni Renderer. Se usará solo su posición.");
            return new Bounds(stationT.position, Vector3.zero);
        }

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        return bounds;
    }

    private float ProjectPointToSplineDistance(Vector3 worldPoint)
    {
        float3 localPoint = splineContainer.transform.InverseTransformPoint(worldPoint);

        SplineUtility.GetNearestPoint(
            splineContainer.Spline,
            localPoint,
            out float3 nearest,
            out float t
        );

        return t * splineLength;
    }

    public float GetSplineLength() => splineLength;
}