using Plana.Database;
using Plana.NetworkProtocol;
using Phrenapates.Services;
using Serilog;
using System.Reflection;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class ProtocolHandlerAttribute : Attribute
    {
        public Protocol Protocol { get; }

        public ProtocolHandlerAttribute(Protocol protocol)
        {
            Protocol = protocol;
        }
    }

    public interface IProtocolHandlerFactory
    {
        public object? Invoke(Protocol protocol, RequestPacket? req);
        public MethodInfo? GetProtocolHandler(Protocol protocol);
        public Type? GetRequestPacketTypeByProtocol(Protocol protocol);
        public void RegisterInstance(Type t, object? inst);
    }

    public class ProtocolHandlerFactory : IProtocolHandlerFactory
    {
        private readonly Dictionary<Protocol, MethodInfo> handlers = [];
        private readonly Dictionary<Protocol, Type> requestPacketTypes = [];
        private readonly Dictionary<Type, object?> handlerInstances = [];

        public ProtocolHandlerFactory()
        {
            foreach (var requestPacketType in Assembly.GetAssembly(typeof(RequestPacket))!.GetTypes().Where(x => x.IsAssignableTo(typeof(RequestPacket))))
            {
                if (requestPacketType == typeof(RequestPacket))
                    continue;

                var obj = Activator.CreateInstance(requestPacketType);
                var protocol = requestPacketType.GetProperty("Protocol")!.GetValue(obj);

                if (!requestPacketTypes.ContainsKey((Protocol)protocol!))
                {
                    requestPacketTypes.Add((Protocol)protocol!, requestPacketType);
                }
            }
        }

        public void RegisterInstance(Type t, object? inst)
        {
            handlerInstances.Add(t, inst);

            foreach (var method in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.GetCustomAttribute<ProtocolHandlerAttribute>() is not null))
            {
                var attr = method.GetCustomAttribute<ProtocolHandlerAttribute>()!;
                if (handlers.ContainsKey(attr.Protocol))
                    continue;

                handlers.Add(attr.Protocol, method);
                Log.Debug($"Loaded {method.Name} for {attr.Protocol}");
            }
        }

        public object? Invoke(Protocol protocol, RequestPacket? req)
        {
            var handler = GetProtocolHandler(protocol);
            if (handler is null)
                return null;

            handlerInstances.TryGetValue(handler.DeclaringType!, out var inst);
            return (ResponsePacket?)handler.Invoke(inst, [req]);
        }

        public MethodInfo? GetProtocolHandler(Protocol protocol)
        {
            handlers.TryGetValue(protocol, out var handler);

            return handler;
        }

        public Type? GetRequestPacketTypeByProtocol(Protocol protocol)
        {
            requestPacketTypes.TryGetValue(protocol, out var type);

            return type;
        }
    }

    public abstract class ProtocolHandlerBase : IHostedService
    {
        private IProtocolHandlerFactory protocolHandlerFactory;

        public ProtocolHandlerBase(IProtocolHandlerFactory _protocolHandlerFactory)
        {
            protocolHandlerFactory = _protocolHandlerFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            protocolHandlerFactory.RegisterInstance(GetType(), this);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    internal static class ServiceExtensions
    {
        public static void AddProtocolHandlerFactory(this IServiceCollection services)
        {
            services.AddSingleton<IProtocolHandlerFactory, ProtocolHandlerFactory>();
        }

        public static void AddProtocolHandlerGroup<T>(this IServiceCollection services) where T : ProtocolHandlerBase
        {
            services.AddHostedService<T>();
        }

        public static void AddProtocolHandlerGroupByType(this IServiceCollection services, Type type)
        {
            services.AddTransient(typeof(IHostedService), type);
        }
    }
}
