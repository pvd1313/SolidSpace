using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Debug = UnityEngine.Debug;

namespace SolidSpace.DataValidation.Editor
{
    internal static class AssemblyValidatorFactory
    {
        private const string AssemblyNameFilter = @"^(Assembly-CSharp)|(SolidSpace.*)";
        
        private static readonly Dictionary<Type, ValidationMethod> Validators;
        private static readonly Type[] ValidationArgumentTypes;
        private static readonly Type[] ConstructorArgumentTypes;

        static AssemblyValidatorFactory()
        {
            try
            {
                Validators = new Dictionary<Type, ValidationMethod>();
                ValidationArgumentTypes = new[] { typeof(object) };
                ConstructorArgumentTypes = new Type[0];
                
                Initialize();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void Initialize()
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => Regex.IsMatch(a.FullName, AssemblyNameFilter))
                .SelectMany(a => a.GetTypes()).ToList();

            foreach (var type in allTypes)
            {
                var attribute = type.GetCustomAttribute<InspectorDataValidatorAttribute>();
                if (attribute is null)
                {
                    continue;
                }

                if (type.IsAbstract)
                {
                    var message = $"'{type.FullName}' can not be used for validation. Validator can not be abstract";
                    Debug.LogError(message);
                    
                    continue;
                }
                
                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var constructor = type.GetConstructor(flags, null, ConstructorArgumentTypes, null);
                if (constructor is null)
                {
                    var message = $"Failed to get parameterless constructor in type '{type.FullName}'";
                    Debug.LogError(message);
                    
                    continue;
                }

                var methodFound = false;
                foreach (var inter in type.GetInterfaces().Where(i => i.IsGenericType))
                {
                    var genericDefinition = inter.GetGenericTypeDefinition();
                    if (genericDefinition != typeof(IDataValidator<>))
                    {
                        continue;
                    }

                    var genericArgument0 = inter.GetGenericArguments()[0];
                    if (Validators.TryGetValue(genericArgument0, out var validationMethod))
                    {
                        var message = $"'{genericArgument0.FullName}' can be validated by more than one validator. " +
                                      $"'{validationMethod.validator.GetType().FullName}' will be used. " +
                                      $"'{type.FullName}' will be ignored.";
                        Debug.LogError(message);
                        
                        break;
                    }

                    try
                    {
                        var method = new ValidationMethod
                        {
                            method = GetValidationMethod(type, genericArgument0),
                            validator = constructor.Invoke(null)
                        };

                        if (method.method == null)
                        {
                            Debug.LogError($"Failed to get validation method in '{type.FullName}'");
                            
                            break;
                        }

                        Validators[genericArgument0] = method;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    methodFound = true;
                }

                if (!methodFound)
                {
                    Debug.LogError($"Failed to find any validation method in '{type.FullName}'");
                }
            }
        }

        private static MethodInfo GetValidationMethod(Type objectType, Type genericArgumentType)
        {
            IDataValidator<object> dummy;

            ValidationArgumentTypes[0] = genericArgumentType;

            return objectType.GetMethod(nameof(dummy.Validate), ValidationArgumentTypes);
        }

        public static bool TryGetValidatorFor(Type type, out ValidationMethod validator)
        {
            // TODO [T-7]: Validation ignores class inheritance.
            return Validators.TryGetValue(type, out validator);
        }
    }
}