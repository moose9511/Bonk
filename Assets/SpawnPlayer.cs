using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpawnPlayer : MonoBehaviour
{
    [SerializeField] private Button respawnBtn;
    [SerializeField] private GameObject playerPrefab;

    GameObject playerInstance;
    private Transform spawnTrans;

    public static SpawnPlayer instance;
    private void Awake()
    {
        instance = this;
    }

    public void Respawn()
    {
        var playa = playerInstance.GetComponent<Player>();
        playa.TakeDamageServerRpc(-100);
        playa.canvas.SetActive(true);
        playa.respawnPos = FindAnyObjectByType<CustomPlayerSpawner>().transform.position;
        playa.GetComponent<PlayerMovement2>().extraForce = Vector3.zero;

        var playercam = playerInstance.GetComponent<CameraMovement>();
        playercam.useSceneCam = false;
        playercam.UseCorrectCameras();

        respawnBtn.gameObject.SetActive(false);
    }

    public IEnumerator RespawnCountDown(GameObject player, Transform spawn)
    {
        respawnBtn.gameObject.SetActive(true);
        respawnBtn.interactable = false;

        playerInstance = player;
        spawnTrans = spawn;

        var text = respawnBtn.GetComponentInChildren<TextMeshProUGUI>();

        for (int i = 3; i > 0; i--)
        {
            text.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }

        respawnBtn.interactable = true;

        text.text = "RESPAWN";
    }


}
