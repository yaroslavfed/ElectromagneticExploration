using Autofac;
using Electromagnetic.Common.Installers;

namespace Direct.Core.Installers;

public static class ContainerRootInstaller
{
    public static IContainer RegisterContainerRoot(this ContainerBuilder builder)
    {
        builder.RegisterType<Startup>();
        builder.RegisterServices();
        builder.RegisterAutofac();

        return builder.Build();
    }
}