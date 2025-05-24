using Direct.Core.Services.BasisFunctionProvider;
using Electromagnetic.Common.Data.Domain;

namespace Inverse.BornApproximation.Services.JacobianService;

/// <summary>
/// Сервис расчёта матрицы Якобиана для сетки ячеек и списка сенсоров.
/// </summary>
public class BornJacobianService(
    IBasisFunctionProvider basisFunctionProvider
) : IBornJacobianService
{
    /// <summary>
    /// Вычисляет Якобиан метода Борна в аналитической форме на основе фиксированного решения u0
    /// </summary>
    public double[,] BuildAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        IReadOnlyList<CurrentSegment> sources,
        Vector solutionU0
    )
    {
        int m = sensors.Count;
        int n = mesh.Elements.Count;
        var jacobian = new double[3 * m, n];

        for (int j = 0; j < n; j++)
        {
            var elem = mesh.Elements[j];
            var mu0 = elem.Mu;

            // Для каждой сенсорной точки
            for (int i = 0; i < m; i++)
            {
                var sensor = sensors[i];
                if (!elem.Contains(sensor.Position)) continue;

                Vector3D dB = Vector3D.Zero;
                for (int k = 0; k < elem.Edges.Count; k++)
                {
                    var curlWk = basisFunctionProvider.GetCurl(elem, k, sensor.Position);
                    int dofIndex = elem.Edges[k].EdgeIndex;
                    var uk = solutionU0[dofIndex];

                    dB += curlWk * uk / mu0;
                }

                jacobian[3 * i + 0, j] = dB.X;
                jacobian[3 * i + 1, j] = dB.Y;
                jacobian[3 * i + 2, j] = dB.Z;
            }
        }

        return jacobian;
    }

    private static double ComputeDeltaMu(double mu, int iteration, int maxIterations)
    {
        const double maxRelative = 2e-1;
        const double minRelative = 1e-3;

        double t = (double)iteration / maxIterations;
        double relative = maxRelative * (1.0 - t) + minRelative * t;

        double delta = relative * Math.Abs(mu);
        return Math.Max(delta, 1e-10);
    }
}