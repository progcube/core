# Progcube Core

The Progcube framework aims to speed up development of ASP.NET Core 2 SaaS applications.

## Installation

(NuGet package not available yet.)

Clone the repository and run the following command in your application folder:

```
dotnet add reference ../Progcube/Core/Progcube.Core.csproj
```

Alternatively, add the following to your csproj:

```
<ItemGroup>
    <ProjectReference Include="../Progcube/Core/Progcube.Core.csproj" />
</ItemGroup>
```

Finally, bind the Progcube framework in the ConfigureServices() method of your application's Startup.cs:

```
public void ConfigureServices(IServiceCollection services)
{
    // ...

    ProgcubeFramework.Bind(services, new []{
        typeof(YourApplication.Startup),        // Your own application
        typeof(Progcube.Subscriptions.Startup)  // List all Progcube dependencies you are using
    });

    // ...
}
```

## Considerations

The Progcube framework enforces GUIDs for all entity IDs. This is a design decision and we offer no way to override the ID type. Please consider this when evaluating using the framework.
