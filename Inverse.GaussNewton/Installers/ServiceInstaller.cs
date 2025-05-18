using Autofac;
using Inverse.GaussNewton.Services.GaussNewtonInversionService;
using Inverse.GaussNewton.Services.InverseService;
using Inverse.GaussNewton.Services.JacobianService;

namespace Inverse.GaussNewton.Installers;

public static class ServiceInstaller
{
    public static void RegisterGaussNewtonServices(this ContainerBuilder builder)
    {
        builder.RegisterType<JacobianService>().As<IJacobianService>();
        builder.RegisterType<InversionService>().As<IInversionService>();
        builder.RegisterType<GaussNewtonInversionService>().As<IGaussNewtonInversionService>();
    }
}