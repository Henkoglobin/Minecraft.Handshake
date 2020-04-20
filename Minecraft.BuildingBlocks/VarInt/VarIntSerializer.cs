using System;
using System.IO;
using Minecraft.BuildingBlocks.StreamReader;

namespace Minecraft.BuildingBlocks.VarInt {
	public class VarIntSerializer : ITypeSerializer {
		public object ReadObject(Stream stream, Type targetType) {
			if(targetType == typeof(int)) {
				return stream.ReadVarInt32(out _);
			} else if(targetType == typeof(long)) {
				return stream.ReadVarInt64(out _);
			} else {
				throw new InvalidOperationException(
					$"{nameof(VarIntSerializer)} can only read into {nameof(Int32)} and {nameof(Int64)}.");
			}
		}

		public void WriteObject(Stream stream, object value) {
			if(value?.GetType().IsEnum ?? false) {
				if(value.GetType().GetEnumUnderlyingType() == typeof(int)) {
					stream.WriteVarInt32((int)value);
				} else if(value.GetType().GetEnumUnderlyingType() == typeof(long)) {
					stream.WriteVarInt64((long)value);
				} else {
					throw new InvalidOperationException(
						$"{nameof(VarIntSerializer)} can only write enums that have {nameof(Int32)} or {nameof(Int64)} as their underlying type.");
				}

				return;
			}

			if(value is int intValue) {
				stream.WriteVarInt32(intValue);
			} else if(value is long longValue) {
				stream.WriteVarInt64(longValue);
			} else {
				throw new InvalidOperationException(
					$"{nameof(VarIntSerializer)} can only write {nameof(Int32)} and {nameof(Int64)}.");
			}
		}
	}
}
