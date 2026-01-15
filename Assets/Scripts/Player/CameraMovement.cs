using UnityEngine;
using Unity.Netcode;
public class CameraMovement : NetworkBehaviour
{
    private Camera cam;
    [SerializeField] public Camera sceneCam;
    private float xrot, yrot;
    private float lastXRot, lastYRot;

    private bool useSceneCam = false;
    private bool enableMovement = true;

    NetworkVariable<float> sensitivity = new(3f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    void Update()
    {
        if (!IsOwner || !enableMovement) return;

        if (useSceneCam)
        {
            sceneCam.enabled = true;
            cam.enabled = false;
        } else
        {
            sceneCam.enabled = false;
            cam.enabled = true;
        }

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
        if(!IsOwner) return;

        cam = GetComponentInChildren<Camera>();
        var sceneCamObj = FindAnyObjectByType<SceneCam>();
        sceneCam = sceneCamObj.GetComponent<Camera>();

        transform.position += new Vector3(0, 1.5f, 0);
		Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        xrot = cam.transform.localEulerAngles.x;
        yrot = transform.localEulerAngles.y;

        if(gameObject.scene.name == "WaitingRoom")
        {
            useSceneCam = true;
            UseCorrectCameras();
        }
    }

    public void UseCorrectCameras()
    {
        if(!IsOwner) return;

        if (useSceneCam)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            sceneCam.enabled = true;
            cam.enabled = false;
            enableMovement = false;
        } else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            sceneCam.enabled = false;
            cam.enabled = true;
            enableMovement = true;
        }
    }
}
