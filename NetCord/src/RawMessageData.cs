using System.Text;

namespace src;

public sealed class RawMessageData
{
    public sealed record class Attachment
        (string fileName, u8[] data);


    public u64 guildId, channelId, messageId;
    public i64 timestamp;
    public u64 author;
    public string content;
    public Attachment[] attachments;


    public u8[] GetBytes()
    {
        List<u8[]> bytes = [];

        bytes.Add(BitConverter.GetBytes(guildId));
        bytes.Add(BitConverter.GetBytes(channelId));
        bytes.Add(BitConverter.GetBytes(messageId));

        bytes.Add(BitConverter.GetBytes(timestamp));
        bytes.Add(BitConverter.GetBytes(author));

        u8[] contentBytes = Encoding.UTF8.GetBytes(content);
        bytes.Add(BitConverter.GetBytes(contentBytes.Length));
        bytes.Add(contentBytes);

        bytes.Add(BitConverter.GetBytes(attachments.Length));
        foreach(Attachment att in attachments)
        {
            u8[] fileNameBytes = Encoding.UTF8.GetBytes(att.fileName);

            bytes.Add(BitConverter.GetBytes(fileNameBytes.Length));
            bytes.Add(fileNameBytes);

            bytes.Add(BitConverter.GetBytes(att.data.Length));
            bytes.Add(att.data);
        }

        return bytes
            .SelectMany(bytes => bytes)
            .ToArray();
    }

    public void WriteToReadable()
    {
        string path = App.GetPath($"deleted/{guildId}/{channelId}/{messageId}");
        Directory.CreateDirectory(path);

        File.WriteAllText($"{path}/content.txt", content);
        File.WriteAllText($"{path}/timestamp.txt", "[UTC] " + App.GetTimeStr(new DateTime(timestamp)));

        if(attachments.Length > 0)
        {
            path = $"{path}/attachments";
            Directory.CreateDirectory(path);

            foreach(Attachment att in attachments)
                File.WriteAllBytes($"{path}/{att.fileName}", att.data);
        }
    }


    public static RawMessageData FromBytes(u8[] bytes)
    {
        using MemoryStream stream = new(bytes);
        using BinaryReader reader = new(stream);

        RawMessageData msg = new()
        {
            guildId = reader.ReadUInt64(),
            channelId = reader.ReadUInt64(),
            messageId = reader.ReadUInt64(),
            timestamp = reader.ReadInt64(),
            author = reader.ReadUInt64()
        };

        i32 contentLen = reader.ReadInt32();
        msg.content = Encoding.UTF8.GetString(reader.ReadBytes(contentLen));

        msg.attachments = new Attachment[reader.ReadInt32()];
        for(i32 i = 0; i < msg.attachments.Length; i++)
        {
            i32 fileNameLen = reader.ReadInt32();
            string fileName = Encoding.UTF8.GetString(reader.ReadBytes(fileNameLen));

            i32 dataLen = reader.ReadInt32();
            u8[] data = reader.ReadBytes(dataLen);

            msg.attachments[i] = new(fileName, data);
        }

        return msg;
    }
}
