using Autofac;
using Inverse.BornApproximation.Services.BornInversionService;
using Inverse.BornApproximation.Services.JacobianService;

namespace Inverse.BornApproximation.Installers;

public static class ServiceInstaller
{
    public static void RegisterBornServices(this ContainerBuilder builder)
    {
        builder.RegisterType<BornInversionService>().As<IBornInversionService>();
        builder.RegisterType<BornJacobianCacheService>().As<IBornJacobianCacheService>();
    }
}