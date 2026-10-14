using System.Collections.Concurrent;

namespace Devlead.Testing.MockHttp.Tool.Inspect;

internal sealed class RequestActivity
{
    private const int Limit = 200;
    private readonly ConcurrentQueue<ActivityEntry> _entries = new();
    private int _count;
    private int _unauthorized;
    private int _notFound;

    public int Requests => _count;
    public int Unauthorized => _unauthorized;
    public int NotFound => _notFound;

    public IReadOnlyList<ActivityEntry> Snapshot() => _entries.Reverse().ToArray();

    public void Clear()
    {
        while (_entries.TryDequeue(out _))
        {
        }

        Interlocked.Exchange(ref _count, 0);
        Interlocked.Exchange(ref _unauthorized, 0);
        Interlocked.Exchange(ref _notFound, 0);
    }

    public void Add(ActivityEntry entry)
    {
        _entries.Enqueue(entry);
        Interlocked.Increment(ref _count);
        if (entry.Status == StatusCodes.Status401Unauthorized)
        {
            Interlocked.Increment(ref _unauthorized);
        }

        if (entry.Status == StatusCodes.Status404NotFound)
        {
            Interlocked.Increment(ref _notFound);
        }

        while (_entries.Count > Limit && _entries.TryDequeue(out _))
        {
        }
    }
}
