using Autofac;
using Direct.Core.Installers;
using Inverse.SharedCore.Services.DirectTaskService;
using Inverse.SharedCore.Services.InverseService;
using Inverse.SharedCore.Services.MeshRefinerService;

namespace Inverse.Core.Installers;

public static class ServiceInstaller
{
    public static void RegisterServices(this ContainerBuilder builder)
    {
        builder.RegisterType<DirectTaskService>().As<IDirectTaskService>();
        builder.RegisterType<MeshRefinerService>().As<IMeshRefinerService>();
        builder.RegisterType<InversionService>().As<IInversionService>();

        ServicesInstaller.RegisterServices(builder);

        GaussNewton.Installers.ServiceInstaller.RegisterGaussNewtonServices(builder);
        BornApproximation.Installers.ServiceInstaller.RegisterBornServices(builder);
    }
}