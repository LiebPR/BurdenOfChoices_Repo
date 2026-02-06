using System.Collections.Generic;
using UnityEngine;

public class TutorialMissionSystem : MonoBehaviour
{
    #region Inspector
    [SerializeField] List<MonoBehaviour> missions;
    #endregion

    int currentIndex;
    
    private void Start()
    {
        StartMissions();
    }

    public void StartMissions()
    {
        if (missions == null || missions.Count == 0)
        {
            Debug.LogWarning("[MissionSystem] No hay misiones asignadas.");
            return;
        }

        currentIndex = 0;
        PlayNextMission();
    }

    void PlayNextMission()
    {
        if (currentIndex >= missions.Count)
        {
            return;
        }

        var mono = missions[currentIndex];
        var mission = mono as IMissionStep;

        if (mission == null)
        {
            Debug.LogWarning($"[MissionSystem] {mono.name} no implementa IMissionStep.");
            currentIndex++;
            PlayNextMission();
            return;
        }

        if (mission.IsCompleted)
        {
            currentIndex++;
            PlayNextMission();
            return;
        }

        currentIndex++;

        mission.OnMissionCompleted += PlayNextMission;
        mission.StartMission();
    }
}