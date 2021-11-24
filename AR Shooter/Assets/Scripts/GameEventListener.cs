using UnityEngine;
using UnityEngine.Events;
using Game.Weapons;

public class GameEventListener : MonoBehaviour
{
    [SerializeField] private GameEvent[] eventsOnAwake;

    [Space]
    [SerializeField] private GameEvent[] eventsOnStart;

    [Space]
    [SerializeField] private GameEvent[] eventsOnEnable;

    [Space]
    [SerializeField] private GameEvent[] eventsOnDisable;

    private void Awake()
    {
        foreach (GameEvent gameEvent in eventsOnAwake)
        {
            OnEventRaised(gameEvent);
        }
    }

    private void Start()
    {
        foreach (GameEvent gameEvent in eventsOnStart)
        {
            OnEventRaised(gameEvent);
        }
    }

    private void OnEnable()
    { 
        //Event?.RegisterListener(this);

        foreach (GameEvent gameEvent in eventsOnEnable)
        {
            OnEventRaised(gameEvent);
        }
    }

    private void OnDisable()
    {
        //Event?.UnregisterListener(this);

        foreach (GameEvent gameEvent in eventsOnDisable)
        {
            OnEventRaised(gameEvent);
        }
    }

    private void OnEventRaised(GameEvent gameEvent)
    {
        //weaponsConfigsUpdateEvent.Invoke();

        gameEvent.Raise();
    }
}
