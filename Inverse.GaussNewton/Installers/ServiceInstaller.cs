using Autofac;
using Inverse.GaussNewton.Services.GaussNewtonInversionService;
using Inverse.GaussNewton.Services.JacobianService;

namespace Inverse.GaussNewton.Installers;

public static class ServiceInstaller
{
    public static void RegisterGaussNewtonServices(this ContainerBuilder builder)
    {
        builder.RegisterType<GaussNewtonJacobianService>().As<IGaussNewtonJacobianService>();
        builder.RegisterType<GaussNewtonInversionService>().As<IGaussNewtonInversionService>();
    }
}