using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Scriptable Objects/Gun")]
public class Gun : ScriptableObject
{
    [SerializeField] private Mesh gunMesh;
    [SerializeField] private Projectile projectilePrefab;
}
