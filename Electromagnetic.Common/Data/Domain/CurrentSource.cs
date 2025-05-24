using Electromagnetic.Common.Enums;

namespace Electromagnetic.Common.Data.Domain;

public record CurrentSource
{
    public ESourceType Type { get; set; }

    // Для линейного источника
    public Point3D? Start { get; set; }

    public Point3D? End { get; set; }

    
    // Для петли
    public Point3D? Center { get; set; }

    public double Width { get; set; } = 1.0;

    public double Height { get; set; } = 1.0;

    public ELoopPlane Plane { get; set; } = ELoopPlane.XY;

    public double Amperage { get; set; }

    public int SegmentsPerSide { get; set; }
}