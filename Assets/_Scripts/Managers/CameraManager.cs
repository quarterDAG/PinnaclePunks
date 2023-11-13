using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{

    [Header("Camera Settings")]
    [SerializeField] private Cinemachine.CinemachineTargetGroup cinemachineTargetGroup;


    private void Start ()
    {
        PlayerManager.Instance.SetCameraManager(this);
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


    #region Reset
    public void ResetCameraManager ()
    {
        ResetCinemachineTargetGroup();
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
