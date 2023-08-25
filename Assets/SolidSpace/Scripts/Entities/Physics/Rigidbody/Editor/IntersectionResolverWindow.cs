using SolidSpace.Mathematics;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace SolidSpace.Entities.Physics.Rigidbody.Editor
{
    public class IntersectionResolverWindow : EditorWindow
    {
        [SerializeField] private ShapeInfo _shapeA;
        [SerializeField] private ShapeInfo _shapeB;

        private void OnEnable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            
            var serialized = new SerializedObject(this);
            EditorGUILayout.PropertyField(serialized.FindProperty(nameof(_shapeA)));
            EditorGUILayout.PropertyField(serialized.FindProperty(nameof(_shapeB)));

            if (EditorGUI.EndChangeCheck())
            {
                serialized.ApplyModifiedProperties();
                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            Undo.RecordObject(this, string.Empty);
            
            DrawShapeHandles(ref _shapeA, "A");
            DrawShapeHandles(ref _shapeB, "B");

            var convertedA = Convert(_shapeA);
            var convertedB = Convert(_shapeB);

            if (CollisionResolver.ResolveIntersection(convertedA, convertedB, out var motionA, out var motionB))
            {
                var pointsA = CollisionResolver.GetShapePointsClockwise(convertedA);
                HandlesDrawLine(pointsA.p0, pointsA.p0 + motionA * 2f, Color.cyan);
                HandlesDrawLine(pointsA.p1, pointsA.p1 + motionA * 2f, Color.cyan);
                HandlesDrawLine(pointsA.p2, pointsA.p2 + motionA * 2f, Color.cyan);
                HandlesDrawLine(pointsA.p3, pointsA.p3 + motionA * 2f, Color.cyan);
                
                var pointsB = CollisionResolver.GetShapePointsClockwise(convertedB);
                HandlesDrawLine(pointsB.p0, pointsB.p0 + motionB * 2f, Color.cyan);
                HandlesDrawLine(pointsB.p1, pointsB.p1 + motionB * 2f, Color.cyan);
                HandlesDrawLine(pointsB.p2, pointsB.p2 + motionB * 2f, Color.cyan);
                HandlesDrawLine(pointsB.p3, pointsB.p3 + motionB * 2f, Color.cyan);
            }

            Repaint();
        }

        private void DrawShapeHandles(ref ShapeInfo shape, string name)
        {
            var rotation = Quaternion.AngleAxis(shape.rotation, Vector3.forward);
            shape.position = Handles.PositionHandle(shape.position, rotation);
            
            HandlesDrawShape(Convert(shape), shape.color);
            
            Handles.Label(shape.position, name);
        }

        private CenterRotationSize Convert(ShapeInfo shape)
        {
            return new CenterRotationSize
            {
                center = shape.position,
                rotation = FloatMath.Deg2Rad * shape.rotation,
                size = shape.size
            };
        }

        private void HandlesDrawShape(CenterRotationSize shape, Color color)
        {
            var points = CollisionResolver.GetShapePointsClockwise(shape);
            HandlesDrawLine(points.p0, points.p1, color);
            HandlesDrawLine(points.p1, points.p2, color);
            HandlesDrawLine(points.p2, points.p3, color);
            HandlesDrawLine(points.p3, points.p0, color);
            
            HandlesLabel(points.p0, "p0");
            HandlesLabel(points.p1, "p1");
            HandlesLabel(points.p2, "p2");
            HandlesLabel(points.p3, "p3");
            
            HandlesDrawDottedLine(points.p0, points.p2, color);
            HandlesDrawDottedLine(points.p1, points.p3, color);
        }

        private void HandlesLabel(float2 position, string text)
        {
            Handles.Label(new Vector3(position.x, position.y, 0), text);
        }

        private void HandlesDrawLine(float2 a, float2 b, Color color)
        {
            Handles.color = color;
            var posA = new Vector3(a.x, a.y, 0);
            var posB = new Vector3(b.x, b.y, 0);
            Handles.DrawLine(posA, posB);
        }

        private void HandlesDrawDottedLine(float2 a, float2 b, Color color)
        {
            Handles.color = color;
            var posA = new Vector3(a.x, a.y, 0);
            var posB = new Vector3(b.x, b.y, 0);
            Handles.DrawDottedLine(posA, posB, 1);
        }
    }
}