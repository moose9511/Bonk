using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpeed : MonoBehaviour
{
    Vector3 startPos;
    Vector3 lastPos;

    private Vector3 velocity;
    [SerializeField] private float power = 100f;
    [SerializeField] private int smoothingFrames = 5;

    private Queue<Vector3> velocityHistory = new Queue<Vector3>();

    private void Start()
    {
        lastPos = transform.position;
    }
    void FixedUpdate()
    {
        velocity = (transform.position - lastPos) / Time.deltaTime;

        velocityHistory.Enqueue(velocity);

        if(velocityHistory.Count > smoothingFrames)
        {
            velocityHistory.Dequeue();
        }

        Vector3 smoothedVelocity = Vector3.zero;
        foreach(var v in velocityHistory)
        {
            smoothedVelocity += v;
        }
        velocity = smoothedVelocity / velocityHistory.Count;
        lastPos = transform.position;
    }

    public Vector3 getHitPower() { return velocity * power; }
    public Vector3 getVelocity() { return velocity; }
}
