namespace Phase10_Narrative
{
    public enum P10NarrativeCommandType
    {
        None,
        NarrativePauseGameplay,
        NarrativeResumeGameplay,
        NarrativeRequestInputLock,
        NarrativeReleaseInputLock,
        NarrativeRequestOpenDialogue,
        NarrativeFinishedBlockingSegment
    }
}
