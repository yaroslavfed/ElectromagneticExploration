using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Models;

namespace Inverse.SharedCore.MeshRefinerService;

public interface IMeshRefinerService
{
    List<Cell> RefineOrMergeCellsAdvanced(
        InverseMesh inverseMesh,
        List<Sensor> sensors,
        double[] residuals,
        double thresholdRefine,
        double thresholdMerge,
        double maxResidual,
        MeshRefinementOptions refinementOptions
    );
}