using SolidSpace.GameCycle;
using SolidSpace.Gizmos.Shapes;
using UnityEngine;

using Rect = SolidSpace.Gizmos.Shapes.Rect;

namespace SolidSpace.Gizmos
{
    internal class GizmosManager : IInitializable, IGizmosManager
    {
        public int RenderVersion { get; private set; }
        
        private const int BufferSize = 256;
        
        private readonly GizmosConfig _config;
        private readonly IGizmosStateStorage _storage;

        private Material _material;
        private ShapeStorage<Line> _wireLines;
        private ShapeStorage<Rect> _wireRects;
        private ShapeStorage<Polygon> _wirePolygons;
        private ShapeStorage<Square> _wireSquares;
        private ShapeStorage<Square> _screenSquares;

        public GizmosManager(GizmosConfig config, IGizmosStateStorage storage)
        {
            _config = config;
            _storage = storage;
        }
        
        public void OnInitialize()
        {
            _wireLines = new ShapeStorage<Line>(BufferSize);
            _wireRects = new ShapeStorage<Rect>(BufferSize);
            _wirePolygons = new ShapeStorage<Polygon>(BufferSize);
            _wireSquares = new ShapeStorage<Square>(BufferSize);
            _screenSquares = new ShapeStorage<Square>(BufferSize);
            _material = new Material(_config.Shader);

            Camera.onPostRender += OnRender;
        }
        
        public GizmosHandle GetHandle(object owner, string name, Color defaultColor)
        {
            var id = _storage.GetOrCreateHandleId(owner.GetType().FullName + "(" + name + ")", defaultColor);
            
            return new GizmosHandle(id, this, _storage);
        }

        public GizmosHandle GetHandle(object owner, Color defaultColor)
        {
            var id = _storage.GetOrCreateHandleId(owner.GetType().FullName, defaultColor);
            
            return new GizmosHandle(id, this, _storage);
        }

        private void OnRender(Camera camera)
        {
            _material.SetPass(0);
            
            GizmosScreenDrawer.BeginDraw(camera);
            GizmosScreenDrawer.DrawSquares(_screenSquares);
            GizmosScreenDrawer.EndDraw();
            
            _screenSquares.Clear();
            
            GizmosWireDrawer.BeginDraw();
            GizmosWireDrawer.DrawLines(_wireLines);
            GizmosWireDrawer.DrawRects(_wireRects);
            GizmosWireDrawer.DrawPolygons(_wirePolygons);
            GizmosWireDrawer.DrawSquares(_wireSquares);
            GizmosWireDrawer.EndDraw();
            
            _wireLines.Clear();
            _wireRects.Clear();
            _wirePolygons.Clear();
            _wireSquares.Clear();

            RenderVersion++;
        }

        internal void ScheduleLineDraw(Line line) => _wireLines.Add(line);
        internal void ScheduleWireRectDraw(Rect rect) => _wireRects.Add(rect);
        internal void ScheduleWirePolygonDraw(Polygon polygon) => _wirePolygons.Add(polygon);
        internal void ScheduleWireSquareDraw(Square square) => _wireSquares.Add(square);
        internal void ScheduleScreenSquareDraw(Square square) => _screenSquares.Add(square);

        public void OnFinalize()
        {
            _wireLines.Dispose();
            _wireRects.Dispose();
            _wirePolygons.Dispose();
            _wireSquares.Dispose();
            _screenSquares.Dispose();
            Object.Destroy(_material);
            Camera.onPostRender -= OnRender;
        }
    }
}