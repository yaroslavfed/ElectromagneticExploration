using Autofac;
using Inverse.Core;
using Inverse.Core.Installers;

await new ContainerBuilder().RegisterContainerRoot().Resolve<Startup>().Run();