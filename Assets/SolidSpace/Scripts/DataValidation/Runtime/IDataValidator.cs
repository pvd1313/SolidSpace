namespace SolidSpace.DataValidation
{
    public interface IDataValidator<T>
    {
        public string Validate(T data);
    }
}