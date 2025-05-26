using System;
using System.Reflection;
using System.Linq;
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InjectAttribute : Attribute { }

public class Injector
{
    private readonly ServiceContainer _serviceContainer;

    public Injector(ServiceContainer serviceContainer) => _serviceContainer = serviceContainer;

    public void Inject(object target)
    {
        var type = target.GetType();
        var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<InjectAttribute>() != null);
        foreach (var member in members)
        {
            Type memberType = member switch
            {
                PropertyInfo property => property.PropertyType,
                FieldInfo field => field.FieldType,
                _ => throw new InvalidOperationException($"Unsupported member type: {member.GetType()}")
            };
            var service = _serviceContainer.TryResolve(memberType, out var resolvedService)
                ? resolvedService
                : throw new InvalidOperationException($"Service of type {memberType} is not registered.");

            switch (member)
            {
                case PropertyInfo property:
                    property.SetValue(target, service);
                    break;
                case FieldInfo field:
                    field.SetValue(target, service);
                    break;
            }
        }
    }
}