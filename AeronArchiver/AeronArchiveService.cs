using System;
using System.IO;
using System.Text;
using Adaptive.Aeron;
using Adaptive.Aeron.LogBuffer;
using Adaptive.Agrona;

namespace AeronArchiver;

public class AeronArchiveService
{
    private const string Channel = "aeron:ipc";
    private const int StreamId = 1001;
    private const string ArchiveDirectory = "archive";

    public void Start()
    {
        Directory.CreateDirectory(ArchiveDirectory);

        var ctx = new Aeron.Context()
            .AeronDirectoryName(Aeron.Context.GetAeronDirectoryName())
            .AvailableImageHandler(OnAvailableImage)
            .UnavailableImageHandler(OnUnavailableImage);

        using var aeron = Aeron.Connect(ctx);
        using var subscription = aeron.AddSubscription(Channel, StreamId);

        Console.WriteLine("Listening for messages...");

        var fragmentHandler = new FragmentHandler(FragmentHandler);

        while (true)
        {
            int fragmentsRead = subscription.Poll(fragmentHandler, 10);
            if (fragmentsRead == 0)
            {
                // Sleep or yield to prevent tight loop when no messages are available
                System.Threading.Thread.Sleep(1);
            }
        }
    }

    private void FragmentHandler(IDirectBuffer buffer, int offset, int length, Header header)
    {
        byte[] data = new byte[length];
        buffer.GetBytes(offset, data);

        string message = Encoding.UTF8.GetString(data);
        Console.WriteLine($"Received message: {message}");

        ArchiveMessage(message);
    }

    private void ArchiveMessage(string message)
    {
        string filePath = Path.Combine(ArchiveDirectory, $"{Guid.NewGuid()}.txt");
        File.WriteAllText(filePath, message);
    }

    private void OnAvailableImage(Image image)
    {
        Console.WriteLine($"Available image on {image.SourceIdentity}");
    }

    private void OnUnavailableImage(Image image)
    {
        Console.WriteLine($"Unavailable image on {image.SourceIdentity}");
    }
}