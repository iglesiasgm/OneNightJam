using UnityEngine;

public abstract class RandomRouteEvent :
    MonoBehaviour
{
    protected RandomEventManager eventManager;

    protected RandomEventDefinition definition;

    protected EventSpawnPoint spawnPoint;

    protected Transform trainReference;

    public RandomEventState State
    {
        get;
        private set;
    }

    public RandomEventDefinition Definition =>
        definition;

    public EventSpawnPoint SpawnPoint =>
        spawnPoint;

    public void Initialize(
        RandomEventManager manager,
        RandomEventDefinition eventDefinition,
        EventSpawnPoint point,
        Transform train
    )
    {
        eventManager = manager;
        definition = eventDefinition;
        spawnPoint = point;
        trainReference = train;

        State =
            RandomEventState.Prepared;

        OnPrepared();
    }

    protected virtual void OnPrepared()
    {
    }

    protected void ActivateEvent()
    {
        if (
            State !=
            RandomEventState.Prepared
        )
        {
            return;
        }

        State =
            RandomEventState.Active;

        OnActivated();
    }

    protected virtual void OnActivated()
    {
    }

    protected void CompleteEvent()
    {
        if (
            State ==
            RandomEventState.Succeeded ||
            State ==
            RandomEventState.Failed
        )
        {
            return;
        }

        State =
            RandomEventState.Succeeded;

        Debug.Log(
            $"Evento completado: " +
            $"{definition.DisplayName}"
        );

        eventManager.EventSucceeded(
            this
        );

        OnSucceeded();
    }

    protected void FailEvent()
    {
        if (
            State ==
            RandomEventState.Succeeded ||
            State ==
            RandomEventState.Failed
        )
        {
            return;
        }

        State =
            RandomEventState.Failed;

        Debug.Log(
            $"Evento fallido: " +
            $"{definition.DisplayName}"
        );

        eventManager.EventFailed(
            this
        );

        OnFailed();
    }

    protected virtual void OnSucceeded()
    {
    }

    protected virtual void OnFailed()
    {
    }
}