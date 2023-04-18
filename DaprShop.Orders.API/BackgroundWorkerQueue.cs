using System.Collections.Concurrent;

namespace DaprShop.Orders.API;

public class BackgroundWorkerQueue
{
	private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new();
	private readonly SemaphoreSlim _signal = new(0);

	public async Task<Func<CancellationToken, Task>?> DequeueAsync(CancellationToken cancellationToken)
	{
		await _signal.WaitAsync(cancellationToken);
		_workItems.TryDequeue(out var workItem);
		return workItem;
	}

	public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
	{
		ArgumentNullException.ThrowIfNull(workItem, nameof(workItem));

		_workItems.Enqueue(workItem);
		_signal.Release();
	}
}