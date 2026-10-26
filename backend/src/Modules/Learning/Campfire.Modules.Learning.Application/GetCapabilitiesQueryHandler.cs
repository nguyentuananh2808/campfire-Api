using MediatR;
namespace Campfire.Modules.Learning.Application;
public sealed class GetCapabilitiesQueryHandler(ILearningService service) : IRequestHandler<GetCapabilitiesQuery, LearningModels.Capabilities>
{
    public Task<LearningModels.Capabilities> Handle(GetCapabilitiesQuery request, CancellationToken cancellationToken) => Task.FromResult(service.GetCapabilities());
}
