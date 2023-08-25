namespace SolidSpace.UI.Factory
{
    public interface IStringFieldCorrectionBehaviour
    {
        public string TryFixString(string value, out bool wasFixed);
    }
}