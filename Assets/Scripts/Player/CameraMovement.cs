using UnityEngine;
using Unity.Netcode;
public class CameraMovement : NetworkBehaviour
{
    private Camera cam;
    private float xrot, yrot;
    private float lastXRot, lastYRot;

    NetworkVariable<float> sensitivity = new(3f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    void Update()
    {
        if (!IsOwner) return;

        float mousex = Input.GetAxis("Mouse X") * sensitivity.Value;
        float mousey = Input.GetAxis("Mouse Y") * sensitivity.Value;

        xrot -= mousey;
        xrot = Mathf.Clamp(xrot, -90f, 90f);

		cam.transform.localEulerAngles = new Vector3(xrot, 0, 0);
		transform.Rotate(new Vector3(0, mousex, 0));  

		lastXRot = mousex;
        lastYRot = mousey;
    }

    public override void OnNetworkSpawn()
    {
        cam = GetComponentInChildren<Camera>();

        transform.position += new Vector3(0, 1.5f, 0);
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
