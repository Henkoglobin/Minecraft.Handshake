using System;
using Minecraft.BuildingBlocks.StreamReader;

namespace Minecraft.Handshake {
	class Program {
		static void Main(string[] args) {
			using var socket = new System.Net.Sockets.TcpClient("timo-linde.de", 25565);
			using var stream = socket.GetStream();

			stream.WriteObject(new Model.Handshake() {
				PacketId = 0x00,
				NextState = Model.HandshakeTargetState.Status,
				ProtocolVersion = 578,
				ServerAddress = "timo-linde.de",
				ServerPort = 25565,
			});

			stream.WriteObject(new Model.Request() {
				PacketId = 0x00,
			});

			var now = DateTimeOffset.Now.ToUnixTimeSeconds();
			stream.WriteObject(new Model.Ping() {
				PacketId = 0x01,
				Payload = now
			});

			var response = stream.ReadObject<Model.Response>();
			var pong = stream.ReadObject<Model.Pong>();

			Console.WriteLine(
				$"Payload sent: {now},\n" +
				$"Received:     {pong.Payload}"
			);
			Console.WriteLine(response.ResponseJson);
		}
	}
}
