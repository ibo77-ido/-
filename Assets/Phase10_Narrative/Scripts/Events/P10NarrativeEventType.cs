namespace Phase10_Narrative
{
    public enum P10NarrativeEventType
    {
        None,
        ChapterStartRequested,
        StateAdvanceRequested,
        NodeAdvanceRequested,
        NodeReplayRequested,
        FlagChanged,
        ChapterResetRequested,
        GameStarted,
        OrderCompleted,
        ScoreThresholdReached,
        DialogueLineStarted,
        NarrativeStateEntered,
        NarrativePropInspected
    }
}
