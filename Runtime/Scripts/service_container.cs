public class ServiceContainer
{
    private readonly Dictionary<Type, object> _services = new();

    public void Register<T>(T service)
    {
        _services[typeof(T)] = service;
    }

    public T Resolve<T>()
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }
        throw new InvalidOperationException($"Service of type {typeof(T)} is not registered.");
    }
    public bool TryResolve<T>(out T service)
    {
        if (_services.TryGetValue(typeof(T), out var foundService))
        {
            service = (T)foundService;
            return true;
        }
        service = default;
        return false;
    }

    public bool TryResolve(Type serviceType, out object service)
    {
        if (_services.TryGetValue(serviceType, out var foundService))
        {
            service = foundService;
            return true;
        }
        service = null;
        return false;
    }
}