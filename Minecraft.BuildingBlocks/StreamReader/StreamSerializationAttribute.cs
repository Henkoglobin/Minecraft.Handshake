using System;

namespace Minecraft.BuildingBlocks.StreamReader {
	[AttributeUsage(AttributeTargets.Property, Inherited = true)]
	public class StreamSerializationAttribute : Attribute {
		public int Order { get; }

		public Type TypeSerializer { get; set; }

		public StreamSerializationAttribute(int order) {
			this.Order = order;
		}
	}
}
