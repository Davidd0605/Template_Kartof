// test events

using UnityEngine;

public interface IEvent { }

public struct TestEvent : IEvent { }
public struct TestArgEvent : IEvent { public int Value; }

// actual events

public struct DamageDealtEvent : IEvent { public float Amount; public Vector3 Direction; public GameObject Target; }
public struct DamageReceivedEvent : IEvent { public float Amount; public bool IsPlayer; }
public struct RageLevelAdvancedEvent : IEvent { public int Level; }
public struct RageChangedEvent : IEvent { public float NormalizedRage; }

public struct Phone2FAEvent : IEvent{ public int correctNumber; }

public struct correctNumberEvent : IEvent{};

public struct PlayerDeadEvent : IEvent { }

public struct PlayerWonEvent : IEvent { }

public struct EnemySpawnEvent : IEvent { }
public struct EnemyCullEvent : IEvent { public bool IsWaveEnemy; }
public struct EnemyDeadEvent : IEvent { public bool IsWaveEnemy; }

