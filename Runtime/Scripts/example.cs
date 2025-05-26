using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class DummnyService
{
    public string Name { get; set; } = "Dummy Service";
}

public class DummyClient
{
    [Inject]
    public DummnyService DummyService;

    public void PrintServiceName()
    {
        Console.WriteLine(DummyService.Name);
    }
}

public class DependencyInjectionDemo
{
    public static void Main(string[] args)
    {
        var instances = new List<object>
        {
            new DummyClient()
        };
        var container = new ServiceContainer();
        container.Register(new DummnyService());

        var injector = new Injector(container);
        var instancesWithInjectables = instances
            .Where(instance => instance.GetType()
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Any(m => m.GetCustomAttribute<InjectAttribute>() != null))
            .ToList();
        foreach (var instance in instancesWithInjectables)
        {
            injector.Inject(instance);
        }
        foreach (var instance in instances)
        {
            if (instance is DummyClient client)
            {
                client.PrintServiceName();
            }
        }
    }
}