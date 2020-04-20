using System;
using System.IO;
using System.Text;
using Minecraft.BuildingBlocks.StreamReader;
using Minecraft.BuildingBlocks.VarInt;

namespace Minecraft.BuildingBlocks.String {
	public class StringSerializer : ITypeSerializer {
		public object ReadObject(Stream stream, Type targetType) {
			if(targetType == typeof(string)) {
				var byteLength = stream.ReadVarInt32(out _);

				// TODO: Can we somehow avoid this allocation altogether?
				// TODO: A simple improvement would be to read multiple times, but that doesn't seem perfect either.
				var buffer = new byte[byteLength];
				stream.Read(buffer, 0, byteLength);
				return Encoding.UTF8.GetString(buffer);
			} else {
				throw new InvalidOperationException($"{nameof(StringSerializer)} cannot read type {targetType.FullName}.");
			}
		}

		public void WriteObject(Stream stream, object value) {
			if(value is string stringValue) {
				stream.WriteVarInt32(stringValue.Length);
				stream.Write(Encoding.UTF8.GetBytes(stringValue));
			} else {
				throw new InvalidOperationException($"{nameof(StringSerializer)} cannot serialize type {value?.GetType()?.FullName}.");
			}
		}
	}
}
