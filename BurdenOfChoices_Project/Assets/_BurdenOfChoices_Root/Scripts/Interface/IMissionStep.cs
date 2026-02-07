using System;

public interface IMissionStep
{
    bool IsCompleted { get; }
    event Action OnMissionCompleted;
    void StartMission();
}
