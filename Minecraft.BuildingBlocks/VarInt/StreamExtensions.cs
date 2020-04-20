using System;
using System.IO;

namespace Minecraft.BuildingBlocks.VarInt {
	public static class StreamExtensions {
		private static readonly Lazy<VarIntConverter> converter = new Lazy<VarIntConverter>();

		public static int ReadVarInt32(this Stream stream, out int bytesRead)
			=> converter.Value.ReadInt32(stream, out bytesRead);

		public static long ReadVarInt64(this Stream stream, out int bytesRead)
			=> converter.Value.ReadInt64(stream, out bytesRead);

		public static void WriteVarInt32(this Stream stream, int value)
			=> converter.Value.WriteInt32(stream, value);

		public static void WriteVarInt64(this Stream stream, long value)
			=> converter.Value.WriteInt64(stream, value);
	}
}
