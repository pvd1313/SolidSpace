namespace SolidSpace.Gizmos
{
    public interface IGizmosDrawerManager
    {
        void AddDrawer(IGizmosDrawer drawer);

        void RemoveDrawer(IGizmosDrawer drawer);
    }
}