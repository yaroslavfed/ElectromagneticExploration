using Autofac;
using Splat;
using Splat.Autofac;

namespace Electromagnetic.Common.Installers;

public static class AutofacInstaller
{
    public static void RegisterAutofac(this ContainerBuilder builder)
    {
        var resolver = builder.UseAutofacDependencyResolver();
        resolver.InitializeSplat();
        builder.RegisterInstance(resolver);
    }
}