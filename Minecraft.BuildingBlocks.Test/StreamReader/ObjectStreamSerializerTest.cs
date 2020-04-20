using System;
using System.IO;
using FluentAssertions;
using Minecraft.BuildingBlocks.StreamReader;
using Xunit;

namespace Minecraft.BuildingBlocks.Test.StreamReader {
	public class ObjectStreamSerializerTest {
		public class SimpleTypes {
			[Fact]
			public void ReadObject_WithInt32_ReadSuccessfully() {
				new ObjectStreamSerializer().ReadObject<WithInt32>(new MemoryStream(new byte[] { 0x00, 0x00, 0x00, 0x80 }))
					.Value.Should().Be(128);
			}

			[Fact]
			public void ReadObject_WithUInt32_ReadSuccessfully() {
				new ObjectStreamSerializer().ReadObject<WithUInt32>(new MemoryStream(new byte[] { 0x00, 0x00, 0x00, 0x80 }))
					.Value.Should().Be(128);
			}

			private class WithInt32 {
				[StreamSerialization(10)]
				public int Value { get; private set; }
			}

			private class WithUInt32 {
				[StreamSerialization(10)]
				public uint Value { get; private set; }
			}
		}

		public class TypesWithCustomSerializers {
			[Fact]
			public void ReadObject_CustomSerializer_IsCalled() {
				new ObjectStreamSerializer().ReadObject<WithVarInt32>(new MemoryStream(new byte[4]))
					.Value.Should().Be(1);

				MyReadingSerializer.CallCount.Should().Be(1);
			}

			private class WithVarInt32 {
				[StreamSerialization(10, TypeSerializer = typeof(MyReadingSerializer))]
				public int Value { get; private set; }
			}

			private class MyReadingSerializer : ITypeSerializer {
				public static int CallCount { get; private set; }

				public object ReadObject(Stream stream, Type targetType) {
					CallCount++;

					stream.Read(new byte[4], 0, 4);
					return 1;
				}

				public void WriteObject(Stream stream, object value) => throw new NotImplementedException();
			}
		}
	}
}
