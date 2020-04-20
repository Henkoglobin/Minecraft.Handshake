using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Minecraft.BuildingBlocks.DependencyInjection;
using Minecraft.BuildingBlocks.VarInt;

namespace Minecraft.BuildingBlocks.StreamReader {
	public class ObjectStreamSerializer {
		private readonly Dictionary<Type, ITypeSerializer> typeSerializers = new Dictionary<Type, ITypeSerializer>();
		private readonly IServiceProvider serviceProvider;
		private readonly byte[] byteBuffer = new byte[sizeof(long)];

		public ObjectStreamSerializer()
			: this(new ActivatorServiceProvider()) { }

		public ObjectStreamSerializer(IServiceProvider serviceProvider) {
			this.serviceProvider = serviceProvider;
		}

		public T ReadObject<T>(Stream stream)
			=> (T)this.ReadObject(stream, typeof(T));

		public object ReadObject(Stream stream, Type type) {
			var instance = Activator.CreateInstance(type);
			
			// TODO: This will get relevant if we ever implement gzip...
			_ = stream.ReadVarInt32(out _);
			foreach ((var property, var attribute) in this.GetSerializationProperties(type)) {
				var value = this.ReadValueFromStream(stream, property.PropertyType, attribute.TypeSerializer);

				if (!property.CanWrite) {
					throw new InvalidOperationException($"Cannot write to read-only property {property.Name} on type {type.FullName}");
				}

				property.SetValue(instance, value);
			}

			return instance;
		}

		public void WriteObject(Stream stream, object objectToWrite) {
			if(objectToWrite is null) {
				throw new ArgumentNullException(nameof(objectToWrite));
			}

			var memoryStream = new MemoryStream();

			foreach((var property, var attribute) in this.GetSerializationProperties(objectToWrite.GetType())) {
				var value = property.GetValue(objectToWrite);
				this.WriteValueToStream(memoryStream, value, attribute.TypeSerializer);
			}

			stream.WriteVarInt32((int)memoryStream.Position);
			memoryStream.Seek(0, SeekOrigin.Begin);
			memoryStream.CopyTo(stream);
		}

		private IEnumerable<(PropertyInfo property, StreamSerializationAttribute attribute)> GetSerializationProperties(Type type)
			=> from property in type.GetProperties()
			   let attribute = property.GetCustomAttributes(typeof(StreamSerializationAttribute), true)
				  .Cast<StreamSerializationAttribute>()
				  .SingleOrDefault()
			   where attribute != null
			   orderby attribute.Order
			   select (property, attribute);

		private object ReadValueFromStream(Stream stream, Type propertyType, Type typeSerializerType) {
			if (typeSerializerType != null) {
				var typeSerializer = this.GetTypeSerializer(typeSerializerType);
				return typeSerializer.ReadObject(stream, propertyType);
			}

			if (propertyType == typeof(byte)) {
				return stream.ReadByte();
			} else if (propertyType == typeof(short)) {
				return BinaryPrimitives.ReadInt16BigEndian(this.ReadBytes(stream, sizeof(short)));
			} else if (propertyType == typeof(ushort)) {
				return BinaryPrimitives.ReadUInt16BigEndian(this.ReadBytes(stream, sizeof(ushort)));
			} else if (propertyType == typeof(int)) {
				return BinaryPrimitives.ReadInt32BigEndian(this.ReadBytes(stream, sizeof(int)));
			} else if (propertyType == typeof(uint)) {
				return BinaryPrimitives.ReadUInt32BigEndian(this.ReadBytes(stream, sizeof(uint)));
			} else if (propertyType == typeof(long)) {
				return BinaryPrimitives.ReadInt64BigEndian(this.ReadBytes(stream, sizeof(long)));
			} else if (propertyType == typeof(ulong)) {
				return BinaryPrimitives.ReadUInt64BigEndian(this.ReadBytes(stream, sizeof(ulong)));
			} else {
				throw new InvalidOperationException($"Cannot read type {propertyType.FullName} from stream without explicit serializer.");
			}
		}

		private Span<byte> ReadBytes(Stream stream, int length) {
			stream.Read(this.byteBuffer, 0, length);
			return new Span<byte>(this.byteBuffer, 0, length);
		}

		private void WriteValueToStream(Stream stream, object value, Type typeSerializerType) {
			if (typeSerializerType != null) {
				var typeSerializer = this.GetTypeSerializer(typeSerializerType);
				typeSerializer.WriteObject(stream, value);

				return;
			}

			var span = new Span<byte>(this.byteBuffer);
			if (value is byte byteValue) {
				stream.WriteByte(byteValue);
			} else if (value is short shortValue) {
				BinaryPrimitives.WriteInt16BigEndian(span, shortValue);
				stream.Write(span.Slice(0, sizeof(short)));
			} else if (value is ushort ushortValue) {
				BinaryPrimitives.WriteUInt16BigEndian(span, ushortValue);
				stream.Write(span.Slice(0, sizeof(ushort)));
			} else if (value is int intValue) {
				BinaryPrimitives.WriteInt32BigEndian(span, intValue);
				stream.Write(span.Slice(0, sizeof(int)));
			} else if (value is uint uintValue) {
				BinaryPrimitives.WriteUInt32BigEndian(span, uintValue);
				stream.Write(span.Slice(0, sizeof(uint)));
			} else if (value is long longValue) {
				BinaryPrimitives.WriteInt64BigEndian(span, longValue);
				stream.Write(span.Slice(0, sizeof(long)));
			} else if (value is ulong ulongValue) {
				BinaryPrimitives.WriteUInt64BigEndian(span, ulongValue);
				stream.Write(span.Slice(0, sizeof(ulong)));
			} else {
				throw new InvalidOperationException($"Cannot write type {value?.GetType()?.FullName ?? "null"} to stream without explicit serializer.");
			}
		}

		private ITypeSerializer GetTypeSerializer(Type typeSerializerType) {
			if (!typeof(ITypeSerializer).IsAssignableFrom(typeSerializerType)) {
				throw new ArgumentException($"{typeSerializerType.FullName} does not implement {nameof(ITypeSerializer)}.");
			}

			if (this.typeSerializers.TryGetValue(typeSerializerType, out var cachedSerializer)) {
				return cachedSerializer;
			}

			var serializer = (ITypeSerializer)this.serviceProvider.GetService(typeSerializerType);
			this.typeSerializers[typeSerializerType] = serializer;

			return serializer;
		}
	}
}
