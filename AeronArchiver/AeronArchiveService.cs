using System;
using Adaptive.Aeron;
using Adaptive.Archiver;
using Adaptive.Archiver.Codecs;

namespace AeronArchiver;

public class AeronArchiveService
{
    private const string Channel = "aeron:ipc";
    private const int StreamId = 1001;
    private const string ControlRequestChannel = "aeron:udp?endpoint=localhost:8010";
    private const string ControlResponseChannel = "aeron:udp?endpoint=localhost:8020";

    public void Start()
    {
        using var ctx = new Aeron.Context();
        using var aeron = Aeron.Connect(ctx);
        var archiveContext = new AeronArchive.Context()
            .ControlRequestChannel(ControlRequestChannel)
            .ControlResponseChannel(ControlResponseChannel);
        using var archive = AeronArchive.Connect(archiveContext);

        archive.StartRecording(Channel, StreamId, SourceLocation.LOCAL);

        using var subscription = aeron.AddSubscription(Channel, StreamId);

        var fragmentAssembler = new FragmentAssembler((buffer, offset, length, header) =>
        {
            var message = buffer.GetStringWithoutLengthAscii(offset, length);
            Console.WriteLine("Received: " + message);
        });

        while (true)
        {
            int fragmentsRead = subscription.Poll(fragmentAssembler, 10);

            if (fragmentsRead == 0)
            {
                // Sleep or yield to prevent tight loop when no messages are available
                System.Threading.Thread.Sleep(1);
            }
        }
    }
}