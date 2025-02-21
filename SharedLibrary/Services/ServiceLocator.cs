namespace SharedLibrary.Services;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

    public static void RegisterService<T>(T service)
    {
        var type = typeof(T);
        if (!Services.ContainsKey(type))
        {
            Services[type] = service;
        }
    }

    public static T GetService<T>()
    {
        var type = typeof(T);
        if (Services.ContainsKey(type))
        {
            return (T)Services[type];
        }
        throw new InvalidOperationException($"Service of type {type} not registered.");
    }
}