using UnityEngine;
using Unity.Netcode;
public class CameraMovement : NetworkBehaviour
{
    private Camera cam;
    private float xrot, yrot;
    
    NetworkVariable<float> sensitivity = new(3f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    void Update()
    {
        if (!IsOwner) return;
        //if (!playerMovement.canControl) return;

        float mousex = Input.GetAxis("Mouse X") * sensitivity.Value;
        float mousey = Input.GetAxis("Mouse Y") * sensitivity.Value;

        xrot -= mousey;
        xrot = Mathf.Clamp(xrot, -90f, 90f);

        yrot += mousex;

        cam.transform.localEulerAngles = new Vector3(xrot, 0, 0);
        transform.localEulerAngles = new Vector3(0, yrot, 0);
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
