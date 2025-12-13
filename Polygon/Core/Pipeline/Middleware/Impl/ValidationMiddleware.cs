using Core.Features.Pipeline.Abstractions.Middleware;
using Polygon.Core.Context;

namespace Polygon.Core.Pipeline.Middleware.Impl;

/// <summary>
/// Middleware that performs strict validation on the roles of the current <see cref="MyContext"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="IMiddlewareDescriptor"/> to expose metadata about this middleware.</item>
/// <item>Marked as <see cref="MiddlewareKind.Conditional"/>; it runs only if the context contains roles.</item>
/// <item>Includes metadata specifying validation level and target property.</item>
/// <item>Non-terminal middleware; execution continues to the next middleware if validation passes.</item>
/// </list>
/// </remarks>
public class ValidationMiddleware : IMiddleware<MyContext>, IMiddlewareDescriptor
{
    /// <inheritdoc />
    public string Name => "Validation";

    /// <inheritdoc />
    public MiddlewareKind Kind => MiddlewareKind.Conditional;

    /// <inheritdoc />
    public bool IsTerminal => false;

    /// <inheritdoc />
    public bool IsConditional => true;

    /// <inheritdoc />
    public Type MiddlewareType => GetType();

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Metadata =>
        new Dictionary<string, object?>
        {
            ["ValidationLevel"] = "Strict",
            ["Target"] = "Roles"
        };

    /// <inheritdoc />
    public async Task InvokeAsync(MyContext context, Func<Task> next)
    {
        if (!context.Roles.Contains("User"))
        {
            Console.WriteLine("Skipping ValidationMiddleware because user is not User");
            await next();
            return;
        }

        Console.WriteLine("Running ValidationMiddleware");
        await next();
    }
}
