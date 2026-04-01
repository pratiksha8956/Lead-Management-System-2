using LeadManagement.Application.Contracts;
using MediatR;

namespace LeadManagement.Application.Features.Analytics.Queries.GetAnalytics;

public record GetAnalyticsQuery : IRequest<AnalyticsSnapshotDto>;
