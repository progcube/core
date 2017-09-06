using System;
using Microsoft.Extensions.DependencyInjection;

namespace Progcube.Core
{
    /// <summary>
    /// Represents a static entry point to bind the Progcube framework to your application.
    /// </summary>
    public static class Progcube
    {
        /// <summary>
        /// Call this method in your application's Startup.ConfigureServices().
        /// </summary>
        public static void Bind(IServiceCollection services)
        {
            // TODO: bind mediator here
        }
    }
}
