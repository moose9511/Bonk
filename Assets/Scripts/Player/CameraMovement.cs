using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
public class CameraMovement : NetworkBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] public Camera sceneCam;
    private float xrot, yrot;
    private float lastXRot, lastYRot;

    public bool useSceneCam = false;
    private bool enableMovement = true;

    NetworkVariable<float> sensitivity = new(3f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    void Update()
    {
        if (!IsOwner || !enableMovement) return;

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
		sceneCam = FindAnyObjectByType<SceneCam>().GetComponent<Camera>();

		if (!IsOwner)
        {
            cam.GetComponent<AudioListener>().enabled = false; 
			cam.enabled = false;
			enabled = false;
			return;
		}

        var audioListener = GetComponentInChildren<AudioListener>();

        NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;

        transform.position += new Vector3(0, 1.5f, 0);
		Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        xrot = cam.transform.localEulerAngles.x;
        yrot = transform.localEulerAngles.y;
    }

    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        if (!IsOwner) return;

        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete &&
            sceneEvent.ClientId == NetworkManager.LocalClientId)
        {

            if (sceneEvent.SceneName == "map1")
            {
                useSceneCam = false;
                UseCorrectCameras();
                var spawner = FindAnyObjectByType<CustomPlayerSpawner>();
                transform.position = spawner.transform.position;
                cam.transform.rotation = spawner.spawnQuaternion;
                GetComponent<Player>().SetState("map1");

            }
            else
            {
                useSceneCam = true;
                UseCorrectCameras();
                var spawner = FindAnyObjectByType<CustomPlayerSpawner>();
                transform.position = spawner.transform.position;
                cam.transform.rotation = spawner.spawnQuaternion;
                GetComponent<Player>().SetState("WaitingRoom");
            }
        }
    }



    public void UseCorrectCameras(bool useScene = true)
    {
        if (!IsOwner) return;
        sceneCam = FindAnyObjectByType<SceneCam>().GetComponent<Camera>();

        if (sceneCam == null) return;

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
