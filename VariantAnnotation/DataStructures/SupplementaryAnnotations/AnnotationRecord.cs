using System;
using System.Collections.Generic;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public enum AnnotationRecordDataType : byte
    {
        String,
		Int64List,
        Boolean,
		Int32
    }

    public static class AnnotationRecordCommon
    {
        /// <summary>
        /// returns the data type given the encoded data
        /// 
        /// [8][7]                 [6][5][4][3][2][1]
        /// ======                 ==================
        /// encode the data type   encode the ID
        /// </summary>
        public static AnnotationRecordDataType GetDataType(byte encodedData)
        {
            var dataTypeIndex = (byte)((encodedData & 192) >> 6);

            // sanity check: make sure we can convert our byte to an annotation type
            if (!Enum.IsDefined(typeof(AnnotationRecordDataType), dataTypeIndex))
            {
                throw new IndexOutOfRangeException($"Unable to convert a byte ({dataTypeIndex}) to an AnnotationDataType.");
            }

            // return the appropriate annotation record
            return (AnnotationRecordDataType)dataTypeIndex;
        }

        /// <summary>
        /// returns the id given the encoded data
        /// </summary>
        public static byte GetId(byte encodedData)
        {
            return (byte)(encodedData & 63);
        }

        /// <summary>
        /// returns an encoded ID given an annotation record data type and an ID
        /// </summary>
        public static byte GetEncodedData(AnnotationRecordDataType dataType, byte id)
        {
            // sanity check: make sure our IDs are not higher than
            if (id > 63) throw new ArgumentOutOfRangeException($"The supplied ID is greater than the 6-bit upper limit (63): {id}");

            int dataTypeIndex = (int)dataType;
            return (byte)((dataTypeIndex << 6) | id);
        }
    }

    public abstract class AbstractAnnotationRecord
    {
        #region members

        protected readonly byte EncodedData;

        public AnnotationRecordDataType DataType { get; }

        public byte Id { get; private set; }

        #endregion

        // constructor
        protected AbstractAnnotationRecord(AnnotationRecordDataType dataType, byte id)
        {
            DataType    = dataType;
            Id          = id;
            EncodedData = AnnotationRecordCommon.GetEncodedData(DataType, id);
        }

        /// <summary>
        /// reads an annotation record from the specified reader
        /// </summary>
        public static AbstractAnnotationRecord Read(ExtendedBinaryReader reader)
        {
            // read the encoded data
            var encodedData = reader.ReadByte();
            var dataType    = AnnotationRecordCommon.GetDataType(encodedData);
            var id          = AnnotationRecordCommon.GetId(encodedData);

            switch (dataType)
            {
                case AnnotationRecordDataType.String:
                    return StringRecord.Read(id, reader);

                case AnnotationRecordDataType.Boolean:
                    return BooleanRecord.Read(id, reader);

				case AnnotationRecordDataType.Int32:
					return Int32Record.Read(id, reader);
				
				case AnnotationRecordDataType.Int64List:
					return Int64ListRecord.Read(id, reader);
				
            }

            throw new GeneralException($"Encountered an unknown AnnotationRecordDataType: {dataType}");
        }

        /// <summary>
        /// writes the annotation record to the specified writer
        /// </summary>
        public abstract void Write(ExtendedBinaryWriter writer);
    }

    public class StringRecord : AbstractAnnotationRecord
    {
        #region members

        public string Value { get; }

        #endregion

        // constructor
        public StringRecord(byte id, string s) : base(AnnotationRecordDataType.String, id)
        {
            Value = s;
        }

        /// <summary>
        /// reads the annotation record from the specified reader
        /// </summary>
        public static StringRecord Read(byte id, ExtendedBinaryReader reader)
        {
            return new StringRecord(id, reader.ReadAsciiString());
        }

        /// <summary>
        /// writes the annotation record to the specified writer
        /// </summary>
        public override void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteByte(EncodedData);
            writer.WriteAsciiString(Value);
        }
    }

	public class Int32Record : AbstractAnnotationRecord
	{
		#region members

		public int Value { get; }

		#endregion

		// constructor
		public Int32Record(byte id, int i)
			: base(AnnotationRecordDataType.Int32, id)
		{
			Value = i;
		}

		/// <summary>
		/// reads the annotation record from the specified reader
		/// </summary>
		public static Int32Record Read(byte id, ExtendedBinaryReader reader)
		{
			return new Int32Record(id, reader.ReadInt());
		}

		/// <summary>
		/// writes the annotation record to the specified writer
		/// </summary>
		public override void Write(ExtendedBinaryWriter writer)
		{
			writer.WriteByte(EncodedData);
			writer.WriteInt(Value);
		}
	}

	public class Int64ListRecord : AbstractAnnotationRecord
	{
		#region members

		public List<long> Values { get; }

		#endregion

		// constructor
		public Int64ListRecord(byte id, List<long> values)
			: base(AnnotationRecordDataType.Int64List, id)
		{
			Values = values; //new List<long>(values);
		}

		/// <summary>
		/// reads the annotation record from the specified reader
		/// </summary>
		public static Int64ListRecord Read(byte id, ExtendedBinaryReader reader)
		{
			var numEntries = reader.ReadInt();
			var entries = new List<long>(numEntries);

			for (int entryIndex = 0; entryIndex < numEntries; entryIndex++)
			{
				entries.Add(reader.ReadLong());
			}

			return new Int64ListRecord(id, entries);
		}

		/// <summary>
		/// writes the annotation record to the specified writer
		/// </summary>
		public override void Write(ExtendedBinaryWriter writer)
		{
			writer.WriteByte(EncodedData);
			writer.WriteInt(Values.Count);
			foreach (var l in Values) writer.WriteLong(l);
		}
	}

    public class BooleanRecord : AbstractAnnotationRecord
    {
        #region members

        public bool Value { get; }

        #endregion

        // constructor
        public BooleanRecord(byte id, bool b) : base(AnnotationRecordDataType.Boolean, id)
        {
            Value = b;
        }

        /// <summary>
        /// reads the annotation record from the specified reader
        /// </summary>
        public static BooleanRecord Read(byte id, ExtendedBinaryReader reader)
        {
            return new BooleanRecord(id, reader.ReadBoolean());
        }

        /// <summary>
        /// writes the annotation record to the specified writer
        /// </summary>
        public override void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteByte(EncodedData);
            writer.WriteBoolean(Value);
        }
    }
}
