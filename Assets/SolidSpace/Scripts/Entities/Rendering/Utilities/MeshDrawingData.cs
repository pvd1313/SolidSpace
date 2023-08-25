using UnityEngine;

namespace SolidSpace.Entities.Rendering.Utilities
{
    public struct MeshDrawingData
    {
        public Mesh mesh;
        public int layer;
        public Material material;
        public Matrix4x4 matrix;
        public Camera camera;
        public int subMeshIndex;
        public MaterialPropertyBlock properties;
        public bool castShadows;
        public bool receiveShadows;
        public bool useLightProbes;
    }
}