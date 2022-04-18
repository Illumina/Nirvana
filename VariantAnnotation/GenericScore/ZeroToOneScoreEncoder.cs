using System;
using ErrorHandling.Exceptions;
using IO;

namespace VariantAnnotation.GenericScore
{
    public sealed class ZeroToOneScoreEncoder : IScoreEncoder
    {
        private readonly byte[] _encodedArray;
        private readonly int    _numberOfDigits;
        private readonly uint   _maxNumber;
        private readonly double _maxScore;

        public ushort BytesRequired { get; }

        public ZeroToOneScoreEncoder(int numberOfDigits, double maxScore)
        {
            _numberOfDigits = numberOfDigits;
            _maxScore       = maxScore;

            _maxNumber    = (uint) Math.Pow(10, _numberOfDigits);
            BytesRequired = (ushort) Math.Ceiling(_numberOfDigits / Math.Log10(256));

            _encodedArray = new byte[BytesRequired];
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOpt(_numberOfDigits);
            writer.Write(_maxScore);
        }

        public static ZeroToOneScoreEncoder Read(ExtendedBinaryReader reader)
        {
            return new ZeroToOneScoreEncoder(reader.ReadOptInt32(), reader.ReadDouble());
        }

        public byte[] EncodeToBytes(double number)
        {
            Array.Clear(_encodedArray, 0, _encodedArray.Length);
            if (double.IsNaN(number))
            {
                Array.Fill(_encodedArray, byte.MaxValue);
                return _encodedArray;
            }

            uint transformedNumber = TransformToUint(number);

            // BitConverter is used as a convenient means of transforming the number into bytes
            // Only the `BytesRequred` portion is saved, because the converted bytes will not exceed it.
            Array.Copy(BitConverter.GetBytes(transformedNumber), _encodedArray, BytesRequired);
            return _encodedArray;
        }


        public double DecodeFromBytes(ReadOnlySpan<byte> encodedArray)
        {
            if (encodedArray[^1] == byte.MaxValue) return double.NaN;

            var count = 0;
            var shift = 0;

            // because a variable lenght enodedarray is received, the BitConverter cannot be used directly
            foreach (byte b in encodedArray)
            {
                count |= (b & byte.MaxValue) << shift;
                shift += 8;
            }

            return TransformToDouble((uint) count);
        }

        private uint TransformToUint(double number)
        {
            if (number > _maxScore) throw new UserErrorException("Score may not be larger than maximum score");
            return (uint) Math.Round(number * _maxNumber / _maxScore);
        }

        private double TransformToDouble(uint encodedNumber)
        {
            return encodedNumber * _maxScore / _maxNumber;
        }
    }
}