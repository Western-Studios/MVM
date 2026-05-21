using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Room : MonoBehaviour
{
    public static Room Active { get; private set; }

    [SerializeField] private string roomId;
    [SerializeField] private bool startActive;

    public string RoomId => roomId;
    public Bounds Bounds => col.bounds;
    public bool IsCleared { get; private set; }

    public UnityEvent onEntered;

    private Collider2D col;
    private readonly List<EnemyBase> enemies = new();

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        GetComponentsInChildren(enemies);
    }

    private void Start()
    {
        if (startActive) Activate();
    }

    public void Activate()
    {
        if (Active == this) return;
        Active = this;
        CameraController.Instance?.SetRoom(this);
        onEntered.Invoke();
    }

    public void NotifyEnemyDied()
    {
        enemies.RemoveAll(e => e == null);
        if (enemies.Count == 0)
            IsCleared = true;
    }
}
