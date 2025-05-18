using Electromagnetic.Common.Data.Domain;
using Electromagnetic.Common.Models;

namespace Inverse.SharedCore.MeshRefinerService;

public class MeshRefinerService : IMeshRefinerService
{
    public List<Cell> RefineOrMergeCellsAdvanced(
        InverseMesh inverseMesh,
        List<Sensor> sensors,
        double[] residuals,
        double thresholdRefine,
        double thresholdMerge,
        double maxResidual,
        MeshRefinementOptions refinementOptions
    )
    {
        //     var refinedCells = new List<Cell>();
        //
        //     // Индексация сенсоров
        //     var sensorPositions = sensors.Select(s => new Vector3((float)s.X, (float)s.Y, (float)s.Z)).ToArray();
        //
        //     int refinedCount = 0;
        //     int mergedCount = 0;
        //     int keptCount = 0;
        //
        //     foreach (var cell in inverseMesh.Cells)
        //     {
        //         // 1. Вычисляем локальную невязку вокруг ячейки
        //         double localResidual = CalculateLocalResidual(cell, sensorPositions, residuals);
        //
        //         // 2. Условия дробления
        //         bool canRefine = localResidual > thresholdRefine
        //                          && localResidual > 0.2 * maxResidual
        //                          && cell.BoundX
        //                          > refinementOptions.MinCellSizeFraction * (sensorGrid.EndX - sensorGrid.StartX)
        //                          && cell.SubdivisionLevel < refinementOptions.MaxSubdivisionLevel;
        //
        //         bool canMerge = localResidual < thresholdMerge
        //                         && cell.BoundX * 2
        //                         <= refinementOptions.MaxCellSizeFraction * (sensorGrid.EndX - sensorGrid.StartX);
        //
        //         if (canRefine)
        //         {
        //             Console.WriteLine(
        //                 $"→ Refining cell at ({cell.CenterX}, {cell.CenterY}, {cell.CenterZ}) | Residual: {localResidual:E5}"
        //             );
        //             refinedCells.AddRange(SplitCell(cell));
        //             refinedCount++;
        //         }
        //         else if (canMerge && CanMergeWithNeighbors(cell, sensorPositions, residuals, thresholdMerge))
        //         {
        //             refinedCells.Add(MergeCell(cell));
        //             mergedCount++;
        //         }
        //         else
        //         {
        //             refinedCells.Add(cell);
        //             keptCount++;
        //         }
        //     }
        //
        //     Console.WriteLine($"→ Refined: {refinedCount}, Merged: {mergedCount}, Kept: {keptCount}");
        //     return refinedCells;
        // }
        //
        // private static double CalculateLocalResidual(Cell cell, Vector3[] sensorPositions, double[] residuals)
        // {
        //     var center = new Vector3((float)cell.CenterX, (float)cell.CenterY, (float)cell.CenterZ);
        //     double sum = 0;
        //     double totalWeight = 0;
        //     const double epsilon = 1e-3f;
        //
        //     for (int i = 0; i < sensorPositions.Length; i++)
        //     {
        //         double distance = Vector3.Distance(center, sensorPositions[i]);
        //         double weight = 1.0 / (distance + epsilon); // вес по расстоянию
        //         sum += weight * Math.Abs(residuals[i]);
        //         totalWeight += weight;
        //     }
        //
        //     return totalWeight > 0
        //         ? sum / totalWeight
        //         : 0;
        // }
        //
        // private static List<Cell> SplitCell(Cell cell)
        // {
        //     var children = new List<Cell>();
        //     double hx = cell.BoundX / 2;
        //     double hy = cell.BoundY / 2;
        //     double hz = cell.BoundZ / 2;
        //
        //     for (int dx = -1; dx <= 1; dx += 2)
        //         for (int dy = -1; dy <= 1; dy += 2)
        //             for (int dz = -1; dz <= 1; dz += 2)
        //             {
        //                 children.Add(
        //                     new()
        //                     {
        //                         CenterX = cell.CenterX + dx * hx / 2,
        //                         CenterY = cell.CenterY + dy * hy / 2,
        //                         CenterZ = cell.CenterZ + dz * hz / 2,
        //                         BoundX = hx,
        //                         BoundY = hy,
        //                         BoundZ = hz,
        //                         Mu = cell.Mu,
        //                         SubdivisionLevel = cell.SubdivisionLevel + 1
        //                     }
        //                 );
        //             }
        //
        //     return children;
        return [];
    }
    //
    // private static Cell MergeCell(Cell cell)
    // {
    //     return cell with
    //     {
    //         BoundX = cell.BoundX * 2,
    //         BoundY = cell.BoundY * 2,
    //         BoundZ = cell.BoundZ * 2,
    //         SubdivisionLevel = Math.Max(0, cell.SubdivisionLevel - 1)
    //     };
    // }
    //
    // private static bool CanMergeWithNeighbors(
    //     Cell cell,
    //     Vector3[] sensorPositions,
    //     double[] residuals,
    //     double thresholdMerge
    // )
    // {
    //     double influenceRadius = Math.Max(cell.BoundX, Math.Max(cell.BoundY, cell.BoundZ)) * 2.5;
    //     var center = new Vector3((float)cell.CenterX, (float)cell.CenterY, (float)cell.CenterZ);
    //
    //     for (int i = 0; i < sensorPositions.Length; i++)
    //     {
    //         if (Vector3.Distance(center, sensorPositions[i]) <= influenceRadius)
    //         {
    //             if (Math.Abs(residuals[i]) > thresholdMerge)
    //                 return false;
    //         }
    //     }
    //
    //     return true;
    // }
}