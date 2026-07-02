using UnityEngine;

public class BinderExample : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] int scoreMultiplier = 2;

    // 1. Define the Bindings (The "Subscription Tickets")
    // These hold the logic that runs when an event is raised.
    EventBinding<TestEvent> testEventBinding;
    EventBinding<TestArgEvent> testArgEventBinding;

    void OnEnable() 
    {
        // 2. Initialize the No-Argument Binding
        // Use this for simple triggers like "Game Over" or "Level Start"
        testEventBinding = new EventBinding<TestEvent>(() => {
            Debug.Log("TestEvent received! (No arguments)");
        });

        // 3. Initialize the Argument Binding (Using a Lambda)
        // 'e' represents the eventData (the info inside the event)
        testArgEventBinding = new EventBinding<TestArgEvent>(e => {
            int total = e.Value * scoreMultiplier;
            Debug.Log($"TestArgEvent received! Value: {e.Value}. Multiplied: {total}");
        });

        // 4. Register with the Event Bus
        // This tells the Bus: "Start sending events to these bindings."
        EventBus<TestEvent>.Register(testEventBinding);
        EventBus<TestArgEvent>.Register(testArgEventBinding);
    }

    void OnDisable() 
    {
        // 5. UNREGISTER (CRITICAL STEP)
        // If you don't do this, the EventBus will try to call code on a 
        // deleted object, causing a crash or memory leak.
        EventBus<TestEvent>.Deregister(testEventBinding);
        EventBus<TestArgEvent>.Deregister(testArgEventBinding);
    }

    // --- TESTING SECTION ---
    [ContextMenu("Test Raise Events")]
    public void DebugRaise() 
    {
        // This is how ANY script in the game triggers the events:
        EventBus<TestEvent>.Raise(new TestEvent());
        
        EventBus<TestArgEvent>.Raise(new TestArgEvent { 
            Value = 10 
        });
    }
}
