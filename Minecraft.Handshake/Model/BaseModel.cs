using Minecraft.BuildingBlocks.StreamReader;
using Minecraft.BuildingBlocks.VarInt;

namespace Minecraft.Handshake.Model {
	public class BasePacket {
		//[StreamSerialization(-2, TypeSerializer =typeof(VarIntSerializer))]
		//public int Length { get; set; }

		[StreamSerialization(-1, TypeSerializer = typeof(VarIntSerializer))]
		public int PacketId { get; set; }
	}
}
