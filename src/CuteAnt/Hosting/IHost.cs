using System;
using System.Threading;
using System.Threading.Tasks;

namespace CuteAnt.Hosting
{
  /// <summary>Represents a host instance.</summary>
  public interface IHost : IDisposable
  {
    /// <summary>Starts this host.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the work performed.</returns>
    Task StartAsync(CancellationToken cancellationToken = default(CancellationToken));

    /// <summary>Stops this host.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the work performed.</returns>
    /// <remarks>A stopped host cannot be restarted.
    /// If the provided <paramref name="cancellationToken"/> is canceled or becomes canceled during execution,
    /// the host will terminate ungracefully.</remarks>
    Task StopAsync(CancellationToken cancellationToken = default(CancellationToken));

    /// <summary>Gets the service provider used by this host.</summary>
    IServiceProvider Services { get; }

    /// <summary>Gets a <see cref="Task"/> which completes when this host stops.</summary>
    Task Stopped { get; }
  }
}