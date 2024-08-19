using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiloFPSController : MonoBehaviour
{
    [Header("Camera Settings")] 
    public Transform CameraPivot;
    public Vector2 CameraMinMax;
    public Vector2 MouseSensitivity;
    
    [Header("Movement Settings")]
    public float WalkSpeed;
    public float RunSpeed;
    public float CrouchSpeed;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
