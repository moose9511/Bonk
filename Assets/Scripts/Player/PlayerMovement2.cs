using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using System.Net;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.CompilerServices;

public class PlayerMovement2 : NetworkBehaviour
{
    float hInput, vInput;

    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpSpeed = 6f;
    [SerializeField] private float hitboxOffset = .5f;
    [SerializeField] private float forceDampen = 1f;
    [SerializeField] private float gravity = -14.81f;
    public Vector3 extraForce = Vector3.zero;

    private bool isGrounded;
    private RaycastHit groundHit;
    private RaycastHit obstacleHit;
    private Collider[] bodyColliders;
    private Vector3 correction;

    private Vector3 lastMove = new(1, 0, 1);
    private Vector3 movement;
    private Collider col;
    private Player player;

    public override void OnNetworkSpawn()
    {
        col = GetComponent<Collider>();
        player = GetComponent<Player>();

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
            // adds extra force based on the difference in direction between player and obstacle
            Vector3 normalDiff = bodySpeed.getVelocity().normalized - transform.position.normalized;
            Vector3 speed = bodySpeed.getVelocity();
            Vector3 scaled = Vector3.Scale(normalDiff, speed);
            
            // only adds force if it will increase the player's current extraforce
            if (scaled.magnitude < speed.magnitude && extraForce.magnitude < speed.magnitude)
				extraForce += speed - scaled;
		}
        else if (groundSpeed != null && extraForce.magnitude < groundSpeed.getVelocity().magnitude)
        {
            // adds ground movement to extraforce
            if (extraForce.magnitude < groundSpeed.getVelocity().magnitude)
                extraForce += groundSpeed.getVelocity();
        }
            

        // removes extraforce in direction of walls
        if (extraForce.magnitude > 0 && Physics.SphereCast(transform.position, .5f, extraForce.normalized, out RaycastHit hitInfo, .3f, LayerMask.GetMask("Ground")))
        {
            Vector3 stoppingForce = extraForce;

            extraForce = Vector3.ProjectOnPlane(extraForce, hitInfo.normal);
            stoppingForce -= extraForce;

            player.TakeDamageServerRpc(stoppingForce.magnitude);
        }

        // gets any overlap in colliders and corrects it
        ColliderExtensions.GetPenetrationInLayer(col, LayerMask.GetMask("Ground"), out correction);
        transform.position += correction;

        // applies movement
        transform.position += ((movement * moveSpeed + extraForce) * Time.deltaTime);

        // when on ground, dampen extraforce more
        if (isGrounded)
            forceDampen = .7f;
        else
            forceDampen = 2f;
        DampenExtraForce();
    }

    // constatly dampens extra force applied to the player, dampen force changes based on if the player is grounded
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
        Physics.Raycast(transform.position, -transform.up, out groundHit, 1.1f, LayerMask.GetMask("Ground"));

        if (groundHit.collider == null)
        {
            isGrounded = false;
            return;
        }

        Vector3 horForce = Vector3.ProjectOnPlane(extraForce, transform.up);
        Vector3 groundNormal = groundHit.normal;

        float angle = Vector3.Angle(Vector3.Normalize(horForce + lastMove), groundNormal);
        

        if (angle >= 90f)
        {
            isGrounded = true;

            if (vInput != 0 || hInput != 0 || horForce != Vector3.zero)
            {
                Vector3 dir = ((transform.up * Mathf.Sin((angle - 90) * Mathf.Deg2Rad) * 
                    (horForce + lastMove * moveSpeed).magnitude) + Vector3.ProjectOnPlane(extraForce, transform.up));

                extraForce = dir;
            }
            else
            {
                extraForce = Vector3.zero;
            }
        } else if (angle < 90f && angle > 40f)
        {
            Vector3 slopeDirection = Vector3.ProjectOnPlane(extraForce, groundNormal);
            if (slopeDirection.y < gravity)
            {
                isGrounded = false;
                return;
            }

            extraForce = slopeDirection;
            isGrounded = true;
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

    [Rpc(SendTo.Owner)]
    public void AddForceRpc(Vector3 force)
    {
        extraForce += force;
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
        Gizmos.DrawRay(transform.position, -transform.up * 1.1f);
    }
}
