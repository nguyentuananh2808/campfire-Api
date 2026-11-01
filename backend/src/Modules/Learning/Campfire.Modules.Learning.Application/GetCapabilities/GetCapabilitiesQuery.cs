using MediatR;
using Campfire.Modules.Learning.Application.Models;
namespace Campfire.Modules.Learning.Application.GetCapabilities;
public sealed record GetCapabilitiesQuery : IRequest<LearningModels.Capabilities>;
