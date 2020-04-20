using System;
using System.Collections.Generic;
using System.IO;

namespace Minecraft.BuildingBlocks.VarInt {

	public class VarIntConverter {
		private enum VarIntType {
			VarInt32, VarInt64
		}

		public IEnumerable<byte> GetBytes(int value) {
			var current = (uint)value;
			do {
				var prefix = (byte)(current > 0x7f ? 0x80 : 0x00);
				yield return (byte)(prefix | current & 0x7f);

				current >>= 7;
			} while (current != 0);
		}

		public IEnumerable<byte> GetBytes(long value) {
			var current = (ulong)value;
			do {
				var prefix = (byte)(current > 0x7f ? 0x80 : 0x00);
				yield return (byte)(prefix | current & 0x7f);

				current >>= 7;
			} while (current != 0);
		}

		public int ToInt32(byte[] value, out int bytesRead)
			=> (int)this.ToInt64Impl(value, out bytesRead, VarIntType.VarInt32);

		public long ToInt64(byte[] value, out int bytesRead)
			=> this.ToInt64Impl(value, out bytesRead, VarIntType.VarInt64);

		private long ToInt64Impl(byte[] value, out int bytesRead, VarIntType varIntType) {
			long current = 0x00;

			var byteIndex = 0;
			while (true) {
				(var byteValue, var readMore) = this.DecodeSingleByte(value[byteIndex], byteIndex++, varIntType);

				current |= byteValue;
				if (!readMore) {
					break;
				}
			}

			bytesRead = byteIndex;
			return current;
		}

		public int ReadInt32(Stream stream, out int bytesRead)
			=> (int)this.ReadInt64Impl(stream, out bytesRead, VarIntType.VarInt32);

		public long ReadInt64(Stream stream, out int bytesRead)
			=> this.ReadInt64Impl(stream, out bytesRead, VarIntType.VarInt64);

		private long ReadInt64Impl(Stream stream, out int bytesRead, VarIntType varIntType) {
			long currentValue = 0;
			var byteIndex = 0;

			while (true) {
				var currentByte = stream.ReadByte();

				if (currentByte == -1) {
					throw new EndOfStreamException();
				}

				(var byteValue, var readMore) = this.DecodeSingleByte((byte)currentByte, byteIndex++, varIntType);

				currentValue |= byteValue;

				if (!readMore) {
					break;
				}
			}

			bytesRead = byteIndex;
			return currentValue;
		}

		public void WriteInt32(Stream stream, int value) {
			foreach(var byteToWrite in this.GetBytes(value)) {
				stream.WriteByte(byteToWrite);
			}
		}

		public void WriteInt64(Stream stream, long value) {
			foreach(var byteToWrite in this.GetBytes(value)) {
				stream.WriteByte(byteToWrite);
			}
		}

		private (long byteValue, bool readMore) DecodeSingleByte(byte currentByte, int byteIndex, VarIntType varIntType) {
			if (byteIndex >= this.GetMaxLength(varIntType)) {
				throw new FormatException($"{varIntType} cannot span over more than ten bytes.");
			}

			return (
				byteValue: (currentByte & 0x7f) * (long)Math.Pow(0x80, byteIndex),
				readMore: (currentByte & 0x80) != 0
			);
		}

		private int GetMaxLength(VarIntType type)
			=> type switch
			{
				VarIntType.VarInt32 => 5,
				VarIntType.VarInt64 => 10,
				_ => throw new ArgumentOutOfRangeException(nameof(type))
			};
	}
}
