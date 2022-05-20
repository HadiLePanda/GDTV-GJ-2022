// Created by the GameDev.tv team. Let us know what cool things you create using this! https://GameDev.tv

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Objects with this component should be a child of the checkpoint coordinator
public class Checkpoint : MonoBehaviour
{
    CheckpointCoordinator checkpointCoordinator; 

    const string playerTag = "Player";

    void Awake()
    {
        checkpointCoordinator = GetComponentInParent<CheckpointCoordinator>();
    }

    //Checks whether the player passed through the checkpoint
    void OnTriggerEnter(Collider other)
    {
        //Some general error checking
        if (checkpointCoordinator == null) {return; } 
        if (!other.CompareTag(playerTag)) { return; }

        //Update the checkpoints if the player drives through the correct one
        if (this == checkpointCoordinator.GetNextCheckpoint())
        {
            checkpointCoordinator.SetNextCheckpoint();
            Debug.LogFormat("{0} reached.  The next checkpoint is {1}.", gameObject.name, checkpointCoordinator.GetNextCheckpoint().name);
        }
    }
}
