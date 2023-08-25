using System.Reflection;

namespace SolidSpace.DataValidation.Editor
{
    internal struct ValidationMethod
    {
        public MethodInfo method;
        public object validator;
    }
}