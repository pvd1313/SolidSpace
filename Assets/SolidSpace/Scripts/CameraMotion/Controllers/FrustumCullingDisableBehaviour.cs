using UnityEngine;

namespace SolidSpace.CameraMotion
{
    public class FrustumCullingDisableBehaviour : MonoBehaviour
    {
        private const float BigValue = 100000;
        
        [SerializeField] private Camera _camera;
        
        private void OnPreCull()
        {
            _camera.cullingMatrix = Matrix4x4.Ortho(-BigValue, BigValue, -BigValue, BigValue, 0.01f, BigValue) *
                                    Matrix4x4.Translate(Vector3.forward * -BigValue * 0.5f) *
                                    _camera.worldToCameraMatrix;
        }

        private void OnDisable()
        {
            _camera.ResetCullingMatrix();
        }
    }
}