using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using System.Net;

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
    private Collider[] bodyColliders;
    private Vector3 correction;

    private Vector3 lastMove = new Vector3(1, 0, 1);
    private Collider col;
    public override void OnNetworkSpawn()
    {
        col = GetComponent<Collider>();
    }

    void FixedUpdate()
    {
        vInput = Input.GetAxisRaw("Vertical");
        hInput = Input.GetAxisRaw("Horizontal");

        // prevents faster diagonal movement
        if (Mathf.Abs(hInput) == 1 && Mathf.Abs(vInput) == 1)
        {
            hInput *= .7071f;
            vInput *= .7071f;
        }

        // gets movement direction 
        Vector3 movement = transform.forward * vInput + transform.right * hInput;

        // gets last movement direction for slope calculation
        if (movement != Vector3.zero)
            lastMove = movement;

        SetGroundMovement();
        bodyColliders = Physics.OverlapSphere(transform.position, .6f, LayerMask.GetMask("Ground"));

        // if player is touching ground reset fall speed and allow jump
        ObstacleSpeed groundSpeed = GroundedEvent();

        // gets speed of either an obstacle htting the player or the ground the player is standing on
        Collider wallCollider = FindWallCollider(bodyColliders);
        ObstacleSpeed bodySpeed = null;
        if (wallCollider != null)
            bodySpeed = wallCollider.GetComponent<ObstacleSpeed>();
        

        // prioritizes body collisions over ground collisions
        if (bodySpeed != null)
        {
            if (Mathf.Abs(Vector3.Magnitude(bodySpeed.getVelocity() + transform.position)) > Vector3.Magnitude(transform.position))
            {
                extraForce += bodySpeed.getVelocity();
            }
        }
        else if (groundSpeed != null)
            extraForce += groundSpeed.getVelocity();

        // removes extraforce in direction of walls
        if (extraForce.magnitude > 0 && Physics.SphereCast(transform.position, .5f, extraForce.normalized, out RaycastHit hitInfo, .3f))
        {
            extraForce = Vector3.ProjectOnPlane(extraForce, hitInfo.normal);
        }

        ColliderExtensions.GetPenetrationInLayer(col, LayerMask.GetMask("Ground"), out correction);
        transform.position += correction;
        
        transform.position += ((movement * moveSpeed + extraForce) * Time.deltaTime);

        //Debug.Log(extraForce);
        if (isGrounded)
            forceDampen = .7f;
        else
            forceDampen = 2f;
        DampenExtraForce();
    }

    private void DampenExtraForce()
    {
        if(Mathf.Abs(extraForce.x) > 0)
            extraForce.x -= extraForce.x / forceDampen * Time.deltaTime;
        if (Mathf.Abs(extraForce.z) > 0)
            extraForce.z -= extraForce.z / forceDampen * Time.deltaTime;

    }

    private void SetGroundMovement()
    {
        // ground check, at a certain angle the player is not grounded
        Physics.SphereCast(transform.position, .3f, -transform.up, out groundHit, hitboxOffset);

        float angle = Vector3.Angle(lastMove + Vector3.ProjectOnPlane(extraForce, transform.up), groundHit.normal);

        if (groundHit.collider != null && Mathf.Abs(angle - 90) < 40)
        {
            isGrounded = true;

            // slope movement calculation
            if (vInput != 0 || hInput != 0)
                extraForce += Mathf.Sin((angle - 90) * Mathf.Deg2Rad) * moveSpeed * transform.up;

        }
        else
            isGrounded = false;
    }

    private ObstacleSpeed GroundedEvent()
    {
        ObstacleSpeed groundSpeed = null;
        if (isGrounded)
        {
            if()
            extraForce = Vector3.ProjectOnPlane(extraForce, transform.up);
            groundSpeed = groundHit.collider.GetComponent<ObstacleSpeed>();

            // jump input
            if (Input.GetKey(KeyCode.Space))
                extraForce += transform.up * jumpSpeed;
        }
        else
            extraForce += gravity * Time.deltaTime * transform.up;

        return groundSpeed;
    }

    private Collider FindWallCollider(Collider[] bodyColliders)
    {
        foreach (Collider target in bodyColliders)
        {
            if (target.GetComponent<ObstacleSpeed>())
            {
                return target;
            }
        }
        return null;

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
