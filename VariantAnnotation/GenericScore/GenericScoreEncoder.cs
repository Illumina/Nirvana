using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using IO;

namespace VariantAnnotation.GenericScore;

public sealed class GenericScoreEncoder : IScoreEncoder
{
    private readonly byte[]                              _encodedArray;
    private readonly Dictionary<double, ushort>          _scoreMap;
    private          ImmutableDictionary<ushort, double> _scoreMapReader;
    public           ushort                              BytesRequired => 2;
    private          ushort                              _nextScoreCode;

    public GenericScoreEncoder()
    {
        _encodedArray = new byte[BytesRequired];
        _scoreMap     = new Dictionary<double, ushort>(byte.MaxValue);
    }

    public ushort AddScore(double number)
    {
        // if the score is already in the map, return the index
        // this is because the socre and the code, both should be unique
        if(_scoreMap.TryGetValue(number, out ushort code)) return code;
        
        // if the score is not in the map, add it and return the index
        code = _nextScoreCode++;
        _scoreMap.Add(number, code);
        return code;
    }

    public byte[] EncodeToBytes(double number)
    {
        Array.Clear(_encodedArray, 0, _encodedArray.Length);
        ushort transformedNumber = AddScore(number);

        // BitConverter is used as a convenient means of transforming the number into bytes
        // Only the `BytesRequred` portion is saved, because the converted bytes will not exceed it.
        Array.Copy(BitConverter.GetBytes(transformedNumber), _encodedArray, BytesRequired);
        return _encodedArray;
    }

    public double DecodeFromBytes(ReadOnlySpan<byte> encodedArray)
    {
        // Because the scoreMap uses `ushort`
        return GetScore(BitConverter.ToUInt16(encodedArray));
    }

    private double GetScore(ushort encodedNumber)
    {
        return _scoreMapReader.GetValueOrDefault(encodedNumber, double.NaN);
    }

    public void Write(ExtendedBinaryWriter writer)
    {
        writer.WriteOpt(_scoreMap.Count);
        foreach ((double score, ushort code) in _scoreMap)
        {
            writer.Write(code);
            writer.Write(score);
        }
    }

    public static GenericScoreEncoder Read(ExtendedBinaryReader reader)
    {
        int scoreCount     = reader.ReadOptInt32();
        var scoreMapReader = new Dictionary<ushort, double>(scoreCount);
        for (var i = 0; i < scoreCount; i++)
        {
            scoreMapReader.Add(reader.ReadUInt16(), reader.ReadDouble());
        }

        return new GenericScoreEncoder
        {
            _scoreMapReader = scoreMapReader.ToImmutableDictionary()
        };
    }
}