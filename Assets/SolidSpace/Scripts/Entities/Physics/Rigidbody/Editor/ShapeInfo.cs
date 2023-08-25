using UnityEngine;

namespace SolidSpace.Entities.Physics.Rigidbody.Editor
{
    [System.Serializable]
    public struct ShapeInfo
    {
        [SerializeField] public Color color;
        [SerializeField] public Vector2 position;
        [SerializeField] public Vector2 size;
        [SerializeField] public float rotation;
    }
}