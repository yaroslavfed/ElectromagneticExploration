using Direct.Core.Services.MeshService;
using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Data.TestSession;

namespace Direct.Core.Services.TestSessionService;

/// <inheritdoc />
public class TestSessionService : ITestSessionService
{
    private readonly IMeshService _meshService;

    public TestSessionService(IMeshService meshService)
    {
        _meshService = meshService;
    }

    /// <inheritdoc />
    public async Task<TestSession<Mesh>> CreateTestSessionAsync(TestSession testSession)
    {
        var testConfiguration = await _meshService.GenerateTestConfiguration(testSession);
        var mesh = await _meshService.GenerateMeshAsync(testConfiguration);

        await _meshService.AssignMuesAsync(mesh, testConfiguration);

        return await Task.FromResult(
            new TestSession<Mesh>
            {
                Mesh = mesh,
                Gamma = testConfiguration.AdditionalParameters.Gamma,
                BoundaryCondition = testConfiguration.AdditionalParameters.BoundaryCondition
            }
        );
    }
}