public enum SessionState
{
    Pending,
    Active,
    Exiting,
    Closed
}

/// <summary>
/// One Workstation interaction = one Gameplay Session.
/// Carries all state for a single gameplay run from enter to exit.
/// Bound to RuntimeMode — GameplayMode requires an active session.
/// </summary>
public class GameplayModuleSession
{
    public int SessionId { get; }
    public AreaType SourceArea { get; }
    public RuntimeMode RuntimeMode { get; }
    public SessionState State { get; private set; }

    public bool IsPending => State == SessionState.Pending;
    public bool IsActive => State == SessionState.Active;
    public bool IsExiting => State == SessionState.Exiting;
    public bool IsClosed => State == SessionState.Closed;

    private static int nextId = 1;

    public GameplayModuleSession(AreaType areaType)
    {
        SessionId = nextId++;
        SourceArea = areaType;
        RuntimeMode = RuntimeMode.GameplayMode;
        State = SessionState.Pending;
    }

    public void CommitActive()
    {
        if (State != SessionState.Pending)
        {
            UnityEngine.Debug.LogWarning($"[GameplayModuleSession] Cannot commit active from state {State}");
            return;
        }
        State = SessionState.Active;
    }

    public void BeginExit()
    {
        if (State != SessionState.Active)
        {
            UnityEngine.Debug.LogWarning($"[GameplayModuleSession] Cannot begin exit from state {State}");
            return;
        }
        State = SessionState.Exiting;
    }

    public void Close()
    {
        State = SessionState.Closed;
    }

    // ── Session ↔ RuntimeMode Consistency Validation ──────────

    /// <summary>
    /// Legal: WorldMode + No/Closed Session, GameplayMode + Pending/Active/Exiting Session.
    /// Illegal: GameplayMode + Null/Closed Session, WorldMode + Active Session.
    /// </summary>
    public static bool IsConsistent(RuntimeMode mode, GameplayModuleSession session)
    {
        if (mode == RuntimeMode.WorldMode)
        {
            // WorldMode: session must be null or closed
            return session == null || session.IsClosed;
        }

        if (mode == RuntimeMode.GameplayMode)
        {
            // GameplayMode: session must exist and not be closed
            return session != null && !session.IsClosed;
        }

        return false;
    }
}
