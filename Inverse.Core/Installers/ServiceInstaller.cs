using Autofac;
using Direct.Core.Installers;
using Inverse.SharedCore.DirectTaskService;
using Inverse.SharedCore.MeshRefinerService;

namespace Inverse.Core.Installers;

public static class ServiceInstaller
{
    public static void RegisterServices(this ContainerBuilder builder)
    {
        builder.RegisterType<DirectTaskService>().As<IDirectTaskService>();
        builder.RegisterType<MeshRefinerService>().As<IMeshRefinerService>();

        ServicesInstaller.RegisterServices(builder);

        GaussNewton.Installers.ServiceInstaller.RegisterGaussNewtonServices(builder);
        BornApproximation.Installers.ServiceInstaller.RegisterBornServices(builder);
    }
}