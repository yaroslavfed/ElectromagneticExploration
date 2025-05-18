using Autofac;
using Direct.Core.Models.BasicFunction;
using Direct.Core.Services.AssemblyService;
using Direct.Core.Services.BasisFunctionProvider;
using Direct.Core.Services.BoundaryConditionService;
using Direct.Core.Services.DirectTaskService;
using Direct.Core.Services.MeshService;
using Direct.Core.Services.NumberingService.EdgesNumberingService;
using Direct.Core.Services.NumberingService.NodesNumberingService;
using Direct.Core.Services.PlotService;
using Direct.Core.Services.ProblemService;
using Direct.Core.Services.SensorEvaluator;
using Direct.Core.Services.SolutionExportService;
using Direct.Core.Services.SourceProvider;
using Direct.Core.Services.TestSessionService;
using Direct.Core.Services.VisualizerService;

namespace Direct.Core.Installers;

public static class ServicesInstaller
{
    public static void RegisterServices(this ContainerBuilder builder)
    {
        builder.RegisterType<MeshService>().As<IMeshService>();
        builder.RegisterType<NodesNumberingService>().As<INodesNumberingService>();
        builder.RegisterType<EdgesNumberingService>().As<IEdgesNumberingService>();
        builder.RegisterType<ProblemService>().As<IProblemService>();
        builder.RegisterType<BasicFunctionProvider>().As<IBasisFunctionProvider>();
        builder.RegisterType<SensorEvaluator>().As<ISensorEvaluator>();
        builder.RegisterType<TestSessionService>().As<ITestSessionService>();
        builder.RegisterType<FirstBoundaryConditionService>().As<IBoundaryConditionService>();
        builder.RegisterType<PlotService>().As<IPlotService>();
        builder.RegisterType<VisualizerService>().As<IVisualizerService>();
        builder.RegisterType<SolutionExportService>().As<ISolutionExportService>();
        builder.RegisterType<BasicFunction>().As<IBasicFunction>();
        builder.RegisterType<CurrentSourceProvider>().As<ICurrentSourceProvider>();
        builder.RegisterType<AssemblyService>().As<IAssemblyService>();
        builder.RegisterType<DirectTaskService>().As<IDirectTaskService>();
    }
}