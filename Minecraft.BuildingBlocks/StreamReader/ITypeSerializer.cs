using System;
using System.IO;

namespace Minecraft.BuildingBlocks.StreamReader {
	public interface ITypeSerializer {
		void WriteObject(Stream stream, object value);
		object ReadObject(Stream stream, Type targetType);
	}
}
