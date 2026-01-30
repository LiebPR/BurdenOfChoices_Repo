using System;
using UnityEngine;

public class TeleportSequenceStep : MonoBehaviour, ISequenceStep
{
    [SerializeField] Transform player;
    [SerializeField] Transform npc;
    [SerializeField] Transform pointNPC;
    [SerializeField] Transform pointPlayer;

    public void Play(Action onFinished)
    {
        // Teletransportar
        player.position = pointPlayer.position;
        npc.position = pointNPC.position;

        // Orientar a -Z
        Vector3 targetDirection = Vector3.back; // -Z
        if (player != null)
            player.rotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        onFinished?.Invoke();
    }
}
