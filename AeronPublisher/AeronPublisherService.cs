using System;
using System.Diagnostics;
using System.Text.Json;
using Adaptive.Aeron;
using Adaptive.Agrona;
using Adaptive.Agrona.Concurrent;

namespace AeronPublisher;

public class AeronPublisherService
{
    private const string Content = "hello world from aeron";

    private int _id = 100000;

    public void Start()
    {
        using var ctx = new Aeron.Context();
        using var aeron = Aeron.Connect(ctx);

        string channel = "aeron:ipc";
        int streamId = 1001;

        using var publication = aeron.AddPublication(channel, streamId);

        UnsafeBuffer buffer = new UnsafeBuffer(BufferUtil.AllocateDirectAligned(65536, BitUtil.CACHE_LINE_LENGTH));

        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < 50000; i++)
        {
            var msgBytes = JsonSerializer.Serialize(new Message
            {
                Content = Content,
                Id = ++_id,
                Time = DateTimeOffset.UtcNow
            });

            int length = buffer.PutStringWithoutLengthUtf8(0, msgBytes);
            publication.Offer(buffer, 0, length);
        }

        var elapsed = sw.Elapsed;

        Console.WriteLine($"Total {elapsed}");
    }
}

public record Message
{
    public string Content { get; set; }
    public long Id { get; set; }
    public DateTimeOffset Time { get; set; }
}