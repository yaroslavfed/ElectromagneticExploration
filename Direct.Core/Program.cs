using Autofac;
using Direct.Core;
using Direct.Core.Installers;

await new ContainerBuilder().RegisterContainerRoot().Resolve<Startup>().Run();