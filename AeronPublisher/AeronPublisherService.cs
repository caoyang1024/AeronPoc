using System;
using System.Threading;
using Adaptive.Aeron;
using Adaptive.Agrona.Concurrent;

namespace AeronPublisher;

public class AeronPublisherService
{
    private const string Channel = "aeron:ipc";
    private const int StreamId = 1001;

    private const string Message = "hello world from aeron";

    public void Start()
    {
        using var ctx = new Aeron.Context();
        using var aeron = Aeron.Connect();
        using var publication = aeron.AddPublication(Channel, StreamId);

        UnsafeBuffer buffer = new UnsafeBuffer(new byte[256]);

        for (int i = 0; i < 10; i++)
        {
            int length = buffer.PutStringWithoutLengthUtf8(0, $"{Message} {DateTimeOffset.UtcNow:O}");
            Thread.Sleep(1);
            publication.Offer(buffer, 0, length);
        }
    }
}