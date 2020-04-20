using System;
using System.IO;
using FluentAssertions;
using Minecraft.BuildingBlocks.VarInt;
using Xunit;

namespace Minecraft.BuildingBlocks.Test.VarInt {
	public class VarIntConverterTest {
		public class Int32 {
			[Theory]
			[InlineData(new object[] { 0, new byte[] { 0x00 } })]
			[InlineData(new object[] { 1, new byte[] { 0x01 } })]
			[InlineData(new object[] { 127, new byte[] { 0x7f } })]
			[InlineData(new object[] { 128, new byte[] { 0x80, 0x01 } })]
			[InlineData(new object[] { 255, new byte[] { 0xff, 0x01 } })]
			[InlineData(new object[] { 2147483647, new byte[] { 0xff, 0xff, 0xff, 0xff, 0x07 } })]
			public void GetBytes_PositiveValues_EncodedCorrectly(int value, byte[] expected)
				=> new VarIntConverter().GetBytes(value)
					.Should().Equal(expected);

			[Theory]
			[InlineData(new object[] { -1, new byte[] { 0xff, 0xff, 0xff, 0xff, 0x0f } })]
			[InlineData(new object[] { -2147483648, new byte[] { 0x80, 0x80, 0x80, 0x80, 0x08 } })]
			public void GetBytes_NegativeValues_EncodedCorrectly(int value, byte[] expected)
				=> new VarIntConverter().GetBytes(value)
					.Should().Equal(expected);

			[Theory]
			[InlineData(new object[] { new byte[] { 0x00 }, 0 })]
			[InlineData(new object[] { new byte[] { 0x01 }, 1 })]
			[InlineData(new object[] { new byte[] { 0x7f }, 127 })]
			[InlineData(new object[] { new byte[] { 0x80, 0x01 }, 128 })]
			[InlineData(new object[] { new byte[] { 0xff, 0x01 }, 255 })]
			[InlineData(new object[] { new byte[] { 0xff, 0xff, 0xff, 0xff, 0x07 }, 2147483647 })]
			public void ToInt32_PositiveValues_DecodedCorrectly(byte[] value, int expected) {
				new VarIntConverter().ToInt32(value, out var bytesRead)
								.Should().Be(expected);

				bytesRead.Should().Be(value.Length, "all Bytes should be read");
			}

			[Theory]
			[InlineData(new object[] { new byte[] { 0xff, 0xff, 0xff, 0xff, 0x0f }, -1 })]
			[InlineData(new object[] { new byte[] { 0x80, 0x80, 0x80, 0x80, 0x08 }, -2147483648 })]
			public void ToInt32_NegativeValues_DecodedCorrectly(byte[] value, int expected) {
				new VarIntConverter().ToInt32(value, out var bytesRead)
								.Should().Be(expected);

				bytesRead.Should().Be(value.Length, "all Bytes should be read");
			}

			[Fact]
			public void ToInt32_TooManyBytes_ThrowsException()
				=> new VarIntConverter()
					.Invoking(converter => converter.ToInt32(new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x01 }, out _))
					.Should().Throw<FormatException>("Int32 cannot hold values encoded in more than five bytes");

			[Theory]
			[InlineData(new object[] { new byte[] { 0x00 }, 0 })]
			[InlineData(new object[] { new byte[] { 0x01 }, 1 })]
			[InlineData(new object[] { new byte[] { 0x7f }, 127 })]
			[InlineData(new object[] { new byte[] { 0x80, 0x01 }, 128 })]
			[InlineData(new object[] { new byte[] { 0xff, 0x01 }, 255 })]
			[InlineData(new object[] { new byte[] { 0xff, 0xff, 0xff, 0xff, 0x07 }, 2147483647 })]
			[InlineData(new object[] { new byte[] { 0xff, 0xff, 0xff, 0xff, 0x0f }, -1 })]
			[InlineData(new object[] { new byte[] { 0x80, 0x80, 0x80, 0x80, 0x08 }, -2147483648 })]
			public void ReadInt32_ReadSuccessfully(byte[] value, int expected) {
				var stream = new MemoryStream(value);

				new VarIntConverter().ReadInt32(stream, out var bytesRead)
					.Should().Be(expected);

				bytesRead.Should().Be(value.Length);
				stream.Position.Should().Be(value.Length);
			}

			[Fact]
			public void ReadInt32_TooManyBytes_ThrowsException() {
				var stream = new MemoryStream(new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x01 });

				new VarIntConverter()
					.Invoking(converter => converter.ReadInt32(stream, out _))
					.Should().Throw<FormatException>("Int32 cannot hold values encoded in more than five bytes");
			}
		}

		public class Int64 {
			[Theory]
			[InlineData(new object[] { 0L, new byte[] { 0x00 } })]
			[InlineData(new object[] { 1L, new byte[] { 0x01 } })]
			[InlineData(new object[] { 127L, new byte[] { 0x7f } })]
			[InlineData(new object[] { 128L, new byte[] { 0x80, 0x01 } })]
			[InlineData(new object[] { 255L, new byte[] { 0xff, 0x01 } })]
			[InlineData(new object[] { 2147483647L, new byte[] { 0xff, 0xff, 0xff, 0xff, 0x07 } })]
			[InlineData(new object[] {
				9223372036854775807L,
				new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f },
			})]
			public void GetBytes_Int64_PositiveValues_EncodedCorrectly(long value, byte[] expected)
				=> new VarIntConverter().GetBytes(value)
					.Should().Equal(expected);

			[Theory]
			[InlineData(new object[] {
				-1L,
				new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x01 },
			})]
			[InlineData(new object[] {
				-2147483648L,
				new byte[] { 0x80, 0x80, 0x80, 0x80, 0xf8, 0xff, 0xff, 0xff, 0xff, 0x01 },
			})]
			[InlineData(new object[] {
				-9223372036854775808L,
				new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x01 },
			})]
			public void GetBytes_Int64_NegativeValues_EncodedCorrectly(long value, byte[] expected)
				=> new VarIntConverter().GetBytes(value)
					.Should().Equal(expected);

			[Theory]
			[InlineData(new object[] { new byte[] { 0x00 }, 0 })]
			[InlineData(new object[] { new byte[] { 0x01 }, 1 })]
			[InlineData(new object[] { new byte[] { 0x7f }, 127 })]
			[InlineData(new object[] { new byte[] { 0x80, 0x01 }, 128 })]
			[InlineData(new object[] { new byte[] { 0xff, 0x01 }, 255 })]
			[InlineData(new object[] { new byte[] { 0xff, 0xff, 0xff, 0xff, 0x07 }, 2147483647 })]
			[InlineData(new object[] {
				new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f },
				9223372036854775807L,
			})]
			public void ToInt64_PositiveValues_DecodedCorrectly(byte[] value, long expected) {
				new VarIntConverter().ToInt64(value, out var bytesRead)
							   .Should().Be(expected);

				bytesRead.Should().Be(value.Length);
			}

			[Theory]
			[InlineData(new object[] {
				new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x01 },
				-1L,
			})]
			[InlineData(new object[] {
				new byte[] { 0x80, 0x80, 0x80, 0x80, 0xf8, 0xff, 0xff, 0xff, 0xff, 0x01 },
				-2147483648L,
			})]
			[InlineData(new object[] {
				new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x01 },
				-9223372036854775808L,
			})]
			public void ToInt64_NegativeValues_DecodedCorrectly(byte[] value, long expected) {
				new VarIntConverter().ToInt64(value, out var bytesRead)
							   .Should().Be(expected);

				bytesRead.Should().Be(value.Length);
			}

			[Theory]
			[InlineData(new object[] { new byte[] { 0x00 }, 0 })]
			[InlineData(new object[] { new byte[] { 0x01 }, 1 })]
			[InlineData(new object[] { new byte[] { 0x7f }, 127 })]
			[InlineData(new object[] { new byte[] { 0x80, 0x01 }, 128 })]
			[InlineData(new object[] { new byte[] { 0xff, 0x01 }, 255 })]
			[InlineData(new object[] { new byte[] { 0xff, 0xff, 0xff, 0xff, 0x07 }, 2147483647 })]
			[InlineData(new object[] {
				new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f },
				9223372036854775807L,
			})]
			[InlineData(new object[] {
				new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x01 },
				-1L,
			})]
			[InlineData(new object[] {
				new byte[] { 0x80, 0x80, 0x80, 0x80, 0xf8, 0xff, 0xff, 0xff, 0xff, 0x01 },
				-2147483648L,
			})]
			[InlineData(new object[] {
				new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x01 },
				-9223372036854775808L,
			})]
			public void ReadInt64_ReadSuccessfully(byte[] value, long expected) {
				var stream = new MemoryStream(value);

				new VarIntConverter().ReadInt64(stream, out var bytesRead)
					.Should().Be(expected);

				bytesRead.Should().Be(value.Length);
				stream.Position.Should().Be(value.Length);
			}
		}
	}
}
