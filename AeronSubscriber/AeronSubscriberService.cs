using System;
using Adaptive.Aeron;
using Adaptive.Archiver;

namespace AeronSubscriber;

public class AeronSubscriberService
{
    private const string ControlRequestChannel = "aeron:udp?endpoint=localhost:8010";
    private const string ControlResponseChannel = "aeron:udp?endpoint=localhost:8020";

    public void Start()
    {
        using var context = new Aeron.Context();
        using var aeron = Aeron.Connect(context);
        var archiveContext = new AeronArchive.Context()
            .ControlRequestChannel(ControlRequestChannel)
            .ControlResponseChannel(ControlResponseChannel);

        using var archive = AeronArchive.Connect(archiveContext);

        var replayChannel = "aeron:udp?endpoint=localhost:40123";
        var streamId = 1001;

        // Find the recorded stream
        long recordingId = archive.ListRecordings(0, 1, new RecordingDescriptorConsumer());

        if (recordingId != -1)
        {
            // Start a replay of the recorded stream
            archive.StartReplay(recordingId, 0, long.MaxValue, replayChannel, streamId);

            using var subscription = aeron.AddSubscription(replayChannel, streamId);

            var fragmentAssembler = new FragmentAssembler((buffer, offset, length, header) =>
            {
                var message = buffer.GetStringWithoutLengthAscii(offset, length);
                Console.WriteLine("Replayed Message: " + message);
            });

            while (true)
            {
                subscription.Poll(fragmentAssembler, 10);
            }
        }
        else
        {
            Console.WriteLine("No recordings found.");
        }
    }
}

public class RecordingDescriptorConsumer : IRecordingDescriptorConsumer
{
    public void OnRecordingDescriptor(long controlSessionId, long correlationId, long recordingId, long startTimestamp,
        long stopTimestamp, long startPosition, long stopPosition, int initialTermId, int segmentFileLength,
        int termBufferLength, int mtuLength, int sessionId, int streamId, string strippedChannel, string originalChannel,
        string sourceIdentity)
    {
        Console.WriteLine(recordingId);
    }
}