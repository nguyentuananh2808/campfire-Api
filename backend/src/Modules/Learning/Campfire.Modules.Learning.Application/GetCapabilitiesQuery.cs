using MediatR;
namespace Campfire.Modules.Learning.Application;
public sealed record GetCapabilitiesQuery : IRequest<LearningModels.Capabilities>;
