// Created by the GameDev.tv team. Let us know what cool things you create using this! https://GameDev.tv

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointCoordinator : MonoBehaviour
{
    //Store the checkpoints in the order that they appear around the track
    [SerializeField] List<Checkpoint> checkpoints;

    Checkpoint nextCheckpoint;

    void Awake()
    {
        //If you haven't manually set up the checkpoints in the inspector...
        if (checkpoints.Count == 0)
        {
            //Loop through all children of the trackpoint coordinator and add them to the checkpoints list
            foreach (Transform child in transform)
            {
                Checkpoint checkpoint = child.GetComponent<Checkpoint>();
                if (checkpoints != null)
                {
                    checkpoints.Add(checkpoint);

                }
            }
        }

        //Set the first checkpoint
        nextCheckpoint = checkpoints[0];
    }

    //Returns the next checkpoint in the list
    public Checkpoint GetNextCheckpoint()
    {
        return nextCheckpoint;
    }

    //Sets up the next checkpoint and loops back to the start if the end of the list is reached
    public void SetNextCheckpoint()
    {
        int nextCheckpointIndex = checkpoints.IndexOf(nextCheckpoint) + 1;
        if (nextCheckpointIndex >= checkpoints.Count)
        {
            nextCheckpointIndex = 0;
        }
        nextCheckpoint = checkpoints[nextCheckpointIndex];
    }
}
