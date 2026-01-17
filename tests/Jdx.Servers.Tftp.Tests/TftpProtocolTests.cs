using Jdx.Servers.Tftp;
using Xunit;

namespace Jdx.Servers.Tftp.Tests;

public class TftpProtocolTests
{
    [Fact]
    public void BuildRequest_RRQ_ShouldCreateValidPacket()
    {
        // Arrange
        var filename = "test.txt";
        var mode = TftpMode.Octet;

        // Act
        var packet = TftpPacket.BuildRequest(TftpOpcode.RRQ, filename, mode);

        // Assert
        Assert.NotNull(packet);
        Assert.True(packet.Length > 0);
        Assert.Equal((byte)0, packet[0]); // Opcode high byte
        Assert.Equal((byte)1, packet[1]); // Opcode low byte (RRQ)
    }

    [Fact]
    public void BuildRequest_WRQ_ShouldCreateValidPacket()
    {
        // Arrange
        var filename = "upload.txt";
        var mode = TftpMode.Netascii;

        // Act
        var packet = TftpPacket.BuildRequest(TftpOpcode.WRQ, filename, mode);

        // Assert
        Assert.NotNull(packet);
        Assert.True(packet.Length > 0);
        Assert.Equal((byte)0, packet[0]); // Opcode high byte
        Assert.Equal((byte)2, packet[1]); // Opcode low byte (WRQ)
    }

    [Fact]
    public void BuildData_ShouldCreateValidPacket()
    {
        // Arrange
        ushort blockNumber = 1;
        var data = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello"

        // Act
        var packet = TftpPacket.BuildData(blockNumber, data);

        // Assert
        Assert.NotNull(packet);
        Assert.Equal(4 + data.Length, packet.Length); // Opcode (2) + Block# (2) + Data
        Assert.Equal((byte)0, packet[0]); // Opcode high byte
        Assert.Equal((byte)3, packet[1]); // Opcode low byte (DATA)
    }

    [Fact]
    public void BuildAck_ShouldCreateValidPacket()
    {
        // Arrange
        ushort blockNumber = 42;

        // Act
        var packet = TftpPacket.BuildAck(blockNumber);

        // Assert
        Assert.NotNull(packet);
        Assert.Equal(4, packet.Length); // Opcode (2) + Block# (2)
        Assert.Equal((byte)0, packet[0]); // Opcode high byte
        Assert.Equal((byte)4, packet[1]); // Opcode low byte (ACK)
    }

    [Fact]
    public void BuildError_ShouldCreateValidPacket()
    {
        // Arrange
        var errorCode = TftpErrorCode.FileNotFound;
        var errorMessage = "File not found";

        // Act
        var packet = TftpPacket.BuildError(errorCode, errorMessage);

        // Assert
        Assert.NotNull(packet);
        Assert.True(packet.Length > 4); // Opcode (2) + ErrorCode (2) + Message + 0
        Assert.Equal((byte)0, packet[0]); // Opcode high byte
        Assert.Equal((byte)5, packet[1]); // Opcode low byte (ERROR)
    }
}
