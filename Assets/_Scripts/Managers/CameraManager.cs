using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{

    [Header("Camera Settings")]
    public List<Camera> playerCameras;
    [SerializeField] private Cinemachine.CinemachineTargetGroup cinemachineTargetGroup;
    [SerializeField] private float proximityThreshold = 50f; // Distance at which cameras switch
    [SerializeField] private Canvas dividerCanvas;

    private void Start ()
    {
        PlayerManager.Instance.SetCameraManager(this);
    }

    private void Update ()
    {
        CheckPlayersProximity();
    }

    public void AddPlayerToCinemachineTargetGroup ( Transform playerTransform )
    {
        var target = new Cinemachine.CinemachineTargetGroup.Target
        {
            target = playerTransform,
            weight = 1f,
            radius = 5f
        };

        cinemachineTargetGroup.AddMember(playerTransform, target.weight, target.radius);
    }

    public void AddPlayerCameraToPlayerCameras ( PlayerInput instantiatedPlayer )
    {
        Camera playerCamera = instantiatedPlayer.GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            playerCameras.Add(playerCamera); // Add the camera to the list
        }
        else
        {
            Debug.LogWarning("No camera found in the player's children. Make sure the player prefab has a camera.");
        }
    }

    private void CheckPlayersProximity ()
    {
        bool allPlayersClose = true;

        for (int i = 0; i < playerCameras.Count; i++)
        {
            for (int j = i + 1; j < playerCameras.Count; j++)
            {
                float distance = Vector3.Distance(playerCameras[i].transform.parent.position, playerCameras[j].transform.parent.position);
                if (distance > proximityThreshold)
                {
                    allPlayersClose = false;
                    break;
                }
            }
            if (!allPlayersClose) break;
        }

        // Enable/disable cameras based on players' proximity
        foreach (var cam in playerCameras)
        {
            cam.enabled = !allPlayersClose;
        }
        dividerCanvas.enabled = !allPlayersClose;
    }

    #region Reset
    public void ResetCameraManager ()
    {
        playerCameras.Clear();
        ResetCinemachineTargetGroup();
        dividerCanvas.enabled = true;
    }

    private void ResetCinemachineTargetGroup ()
    {
        while (cinemachineTargetGroup.m_Targets.Length > 0)
        {
            cinemachineTargetGroup.RemoveMember(cinemachineTargetGroup.m_Targets[0].target);
        }
    }
    #endregion
}
