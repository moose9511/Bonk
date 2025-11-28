using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using System.Net;
using System.Collections;

public class PlayerMovement2 : NetworkBehaviour
{
    float hInput, vInput;

    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpSpeed = 6f;
    [SerializeField] private float hitboxOffset = .5f;
    [SerializeField] private float forceDampen = 1f;
    [SerializeField] private float gravity = -14.81f;
    private Vector3 extraForce = Vector3.zero;

    private bool isGrounded;
    private RaycastHit groundHit;
    private RaycastHit obstacleHit;
    private Collider[] bodyColliders;
    private Vector3 correction;

    private Vector3 lastMove = new Vector3(1, 0, 1);
    private Vector3 movement;
    private Collider col;
    private Stats stats;
    private CameraMovement cameraMovement;

    public override void OnNetworkSpawn()
    {
        col = GetComponent<Collider>();
        stats = GetComponent<Stats>();
        cameraMovement = GetComponentInChildren<CameraMovement>();

        StartCoroutine(flipGravity());
    }

    private IEnumerator flipGravity()
    {
        Debug.Log("flipping gravity in 15 seconds");
        yield return new WaitForSeconds(15f);
        Debug.Log("gravity flipped");
        transform.localEulerAngles += new Vector3(90, 0, 0);

        StartCoroutine(flipGravity());
    }
    void Update()
    {
        if (!IsOwner) return;

        vInput = Input.GetAxisRaw("Vertical");
        hInput = Input.GetAxisRaw("Horizontal");

        // prevents faster diagonal movement
        if (Mathf.Abs(hInput) == 1 && Mathf.Abs(vInput) == 1)
        {
            hInput *= .7071f;
            vInput *= .7071f;
        }

        // gets movement direction 
        movement = transform.forward * vInput + transform.right * hInput;

        // gets last movement direction for slope calculation
        if (movement != Vector3.zero)
            lastMove = movement;

        ApplyGroundMotion();
        bodyColliders = Physics.OverlapSphere(transform.position, .6f, LayerMask.GetMask("Ground"));

        // if player is touching ground reset fall speed and allow jump
        ObstacleSpeed groundSpeed = GroundedEvent();

        // gets speed of either an obstacle htting the player or the ground the player is standing on
        obstacleHit = new RaycastHit();
		if (bodyColliders.Length > 0)
            FindWallCollider(bodyColliders, out obstacleHit);

        ObstacleSpeed bodySpeed = null;
        if (obstacleHit.collider != null)
            bodySpeed = obstacleHit.collider.GetComponent<ObstacleSpeed>();
        

        // prioritizes body collisions over ground collisions
        if (bodySpeed != null)
        {
            Debug.Log("body speed applied");
			Vector3 normalDiff = bodySpeed.getVelocity().normalized - transform.position.normalized;
            Vector3 speed = bodySpeed.getVelocity();
            Vector3 scaled = Vector3.Scale(normalDiff, speed);

            if(scaled.magnitude < speed.magnitude && extraForce.magnitude < speed.magnitude)
				extraForce += speed - scaled;

		}
        else if (groundSpeed != null && extraForce.magnitude < groundSpeed.getVelocity().magnitude)
            extraForce += groundSpeed.getVelocity();

        // removes extraforce in direction of walls
        if (extraForce.magnitude > 0 && Physics.SphereCast(transform.position, .5f, extraForce.normalized, out RaycastHit hitInfo, .3f))
        {
            Vector3 stoppingForce = extraForce;
            extraForce = Vector3.ProjectOnPlane(extraForce, hitInfo.normal);
            stoppingForce -= extraForce;

            Debug.Log("stopping force applied: " + stoppingForce.magnitude);
            
            stats.TakeDamage(stoppingForce.magnitude);
        }

        ColliderExtensions.GetPenetrationInLayer(col, LayerMask.GetMask("Ground"), out correction);
        transform.position += correction;

        transform.position += ((movement * moveSpeed + extraForce) * Time.deltaTime);

        if (isGrounded)
            forceDampen = .7f;
        else
            forceDampen = 2f;
        DampenExtraForce();
    }

    private void DampenExtraForce()
    {
        if(Mathf.Abs(extraForce.x) > 1f)
            extraForce.x -= extraForce.x / forceDampen * Time.deltaTime;
        else
            extraForce.x = 0;   
        if (Mathf.Abs(extraForce.z) > 1f)
            extraForce.z -= extraForce.z / forceDampen * Time.deltaTime;
        else
            extraForce.z = 0;

    }

    // handles movement based on the angle of the ground
    private void ApplyGroundMotion()
    {
        Physics.SphereCast(transform.position, .3f, -transform.up, out groundHit, hitboxOffset);
        Vector3 horForce = Vector3.ProjectOnPlane(extraForce, transform.up);

        float angle = Vector3.Angle(horForce + lastMove, groundHit.normal);

        if (groundHit.collider != null && Mathf.Abs(angle - 90) < 40)
        {
            isGrounded = true;

            if(vInput != 0 || hInput != 0 || horForce != Vector3.zero)
            {
                extraForce = ((transform.up * Mathf.Sin((angle - 90) * Mathf.Deg2Rad) * (horForce + lastMove * moveSpeed).magnitude) + Vector3.ProjectOnPlane(extraForce, transform.up));
            } else
            {
                extraForce = Vector3.zero;
            }
        }
        else
            isGrounded = false;

    }

    // handles jump and gravity application
    private ObstacleSpeed GroundedEvent()
    {
        ObstacleSpeed groundSpeed = null;
        if (isGrounded)
        {
            //extraForce = Vector3.ProjectOnPlane(extraForce, transform.up);
            groundSpeed = groundHit.collider.GetComponent<ObstacleSpeed>();

            // jump input
            if (Input.GetKey(KeyCode.Space))
                extraForce += transform.up * jumpSpeed;
        }
        else
            extraForce += gravity * Time.deltaTime * transform.up;

        return groundSpeed;
    }

    // finds the collider of a wall the player is touching
    private void FindWallCollider(Collider[] bodyColliders, out RaycastHit hitInfo)
    {
        foreach (Collider target in bodyColliders)
        {
            if (target.GetComponent<ObstacleSpeed>())
            {
                Physics.Raycast(transform.position, (target.transform.position - transform.position).normalized, out RaycastHit hit, Mathf.Infinity);
                if (hit.collider != target)
                    continue;

                hitInfo = hit;
                return;
            }
        }

        hitInfo = new RaycastHit();

    }
    private void OnDrawGizmos()
    {
        //Gizmos.DrawRay(transform.position, Vector3.down * 1.1f);
        Gizmos.DrawRay(transform.position, lastMove);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position - transform.up * hitboxOffset, .3f);
        Gizmos.DrawWireSphere(transform.position, .6f);
        Gizmos.color = Color.blue;
        if(extraForce.magnitude > 0)
            Gizmos.DrawWireSphere(transform.position + extraForce.normalized * .1f, .5f);
    }
}
