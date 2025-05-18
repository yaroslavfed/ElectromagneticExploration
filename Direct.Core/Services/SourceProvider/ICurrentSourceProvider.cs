using Electromagnetic.Common.Data.Domain;

namespace Direct.Core.Services.SourceProvider;

public interface ICurrentSourceProvider
{
    Task<IReadOnlyList<CurrentSegment>> GetSourcesAsync(CurrentSource source);
}