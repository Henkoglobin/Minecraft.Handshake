using Minecraft.BuildingBlocks.StreamReader;
using Minecraft.BuildingBlocks.String;

namespace Minecraft.Handshake.Model {
	public class Response : BasePacket {
		[StreamSerialization(0, TypeSerializer = typeof(StringSerializer))]
		public string ResponseJson { get; set; }
	}
}
