using UnityEngine;
using Unity.Netcode;
public class CameraMovement : NetworkBehaviour
{
    private Camera cam;
    private float xrot, yrot;
    private float lastXRot, lastYRot;

    private Vector3 camRotation, parentRotation;

    NetworkVariable<float> sensitivity = new(3f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    void Update()
    {
        if (!IsOwner) return;

        float mousex = Input.GetAxis("Mouse X") * sensitivity.Value;
        float mousey = Input.GetAxis("Mouse Y") * sensitivity.Value;
        
        camRotation = transform.up * mousex + transform.right * mousey;

        cam.transform.localEulerAngles += camRotation;
        transform.transform.localEulerAngles += Vector3.ProjectOnPlane(camRotation, transform.up);

        lastXRot = mousex;
        lastYRot = mousey;
    }

    public override void OnNetworkSpawn()
    {
        cam = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        xrot = cam.transform.localEulerAngles.x;
        yrot = transform.localEulerAngles.y;

        if (!IsOwner)
        {
            if(cam != null)
                cam.enabled = false;
        }
        else
        {
            if(cam != null)
                cam.enabled = true;
        }
    }
}
