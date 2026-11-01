using MediatR;
using Campfire.Modules.Learning.Application.Abstractions;
using Campfire.Modules.Learning.Application.Models;
namespace Campfire.Modules.Learning.Application.GetCapabilities;
public sealed class GetCapabilitiesQueryHandler(ILearningService service) : IRequestHandler<GetCapabilitiesQuery, LearningModels.Capabilities>
{
    public Task<LearningModels.Capabilities> Handle(GetCapabilitiesQuery request, CancellationToken cancellationToken) => Task.FromResult(service.GetCapabilities());
}
