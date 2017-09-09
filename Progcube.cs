using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Progcube.Core
{
    /// <summary>
    /// Represents a static entry point to bind the Progcube framework to your application.
    /// </summary>
    public static class ProgcubeFramework
    {
        /// <summary>
        /// Call this method in your application's Startup.ConfigureServices().
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="typesToScan">A list of types to scan their assemblies for CQRS handlers.</param>
        /// </summary>
        public static void Bind(IServiceCollection services, Type[] typesToScan)
        {
            // Register MediatR
            services.AddMediatR(typeof(ProgcubeFramework));
            services.AddTransient<IMediator>(x => new Mediator(x.GetService<SingleInstanceFactory>(), x.GetService<MultiInstanceFactory>()));
            services.AddTransient<SingleInstanceFactory>(x => t => x.GetService(t));
            services.AddTransient<MultiInstanceFactory>(x => t => x.GetServices(t));

            // Bind types from consumer assemblies
            foreach (var assembly in typesToScan.Select(t => t.GetTypeInfo().Assembly))
            {
                foreach (var type in assembly.ExportedTypes.Select(t => t.GetTypeInfo()).Where(t => t.IsClass && !t.IsAbstract))
                {
                    var interfaces = type.ImplementedInterfaces.Select(i => i.GetTypeInfo()).ToArray();

                    foreach (var handlerType in interfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncRequestHandler<,>)))
                    {
                        services.AddTransient(handlerType.AsType(), type.AsType());
                    }

                    foreach (var handlerType in interfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)))
                    {
                        services.AddTransient(handlerType.AsType(), type.AsType());
                    }

                    foreach (var handlerType in interfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                    {
                        services.AddTransient(handlerType.AsType(), type.AsType());
                    }
                }
            }
        }
    }
}
