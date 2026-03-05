using AIResearch.Architecture.Contracts.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace AIResearch.Architecture.Application.Mediator;

internal sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = serviceProvider.GetRequiredService(handlerType);

        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<,>.HandleAsync));

        if (handleMethod == null)
        {
            throw new InvalidOperationException($"HandleAsync method not found for {handlerType}");
        }

        var result = handleMethod.Invoke(handler, [request, cancellationToken]);

        if (result is Task<TResponse> task)
        {
            return await task;
        }

        throw new InvalidOperationException($"Handler for {requestType} did not return Task<{typeof(TResponse)}>");
    }
}
