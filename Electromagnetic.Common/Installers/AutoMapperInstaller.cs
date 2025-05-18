using Autofac;
using AutoMapper;

namespace Electromagnetic.Common.Installers;

public static class AutoMapperInstaller
{
    public static void RegisterAutoMapper(this ContainerBuilder builder)
    {
        builder
            .Register(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    var config = context.Resolve<MapperConfiguration>();

                    return config.CreateMapper(context.Resolve);
                }
            )
            .As<IMapper>()
            .InstancePerLifetimeScope();
    }
}