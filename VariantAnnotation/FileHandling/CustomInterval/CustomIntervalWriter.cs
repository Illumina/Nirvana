using System;
using System.IO;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;

namespace VariantAnnotation.FileHandling.CustomInterval
{
	public sealed class CustomIntervalWriter : IDisposable
	{
		private readonly BinaryWriter _binaryWriter;
		private readonly ExtendedBinaryWriter _writer;
		private readonly string _referenceName;
		private readonly string _intervalType;
		private int _count;
		private readonly DataSourceVersion _version;

		private bool _isDisposed;//for dispose pattern
								 // constructor
		public CustomIntervalWriter(string fileName, string referenceName, string intervalType, DataSourceVersion version)
		{
			var stream = new FileStream(fileName, FileMode.Create);
			_binaryWriter = new BinaryWriter(stream);
			_writer = new ExtendedBinaryWriter(_binaryWriter);

			_referenceName = referenceName;
			_intervalType  = intervalType;
			_version       = version;

			WriteHeader();
		}

		private void WriteHeader()
		{
			_binaryWriter.Write(CustomIntervalCommon.DataHeader);
			_binaryWriter.Write(CustomIntervalCommon.SchemaVersion);
			_binaryWriter.Write(DateTime.UtcNow.Ticks);
			_binaryWriter.Write(_referenceName);
			_binaryWriter.Write(_intervalType);

			_version.Write(_binaryWriter);

			// marking end of header with guard int
			_binaryWriter.Write(CustomIntervalCommon.GuardInt);
		}

	    public void WriteInterval(DataStructures.CustomInterval interval)
		{
			if (interval.IsEmpty())
			{
				// marks the last interval
				_writer.WriteInt(interval.Start);
				_writer.WriteInt(interval.End);
				_writer.WriteInt(0);// for the null string dictionary
				_writer.WriteInt(0);// for the null non-string dictionary
				return;
			}

			if (interval.ReferenceName != _referenceName)
			{
				throw new Exception(
					$"Unexpected interval in custom interval writer.\nExpected reference name: {_referenceName}, observed reference name: {interval.ReferenceName}");
			}
			if (interval.Type != _intervalType)
			{
				throw new Exception(
					$"Unexpected interval in custom interval writer.\nExpected interval type: {_intervalType}, observed interval type: {interval.Type}");
			}

			_writer.WriteInt(interval.Start);
			_writer.WriteInt(interval.End);

			if (interval.StringValues != null)
			{
				_writer.WriteInt(interval.StringValues.Count);

				foreach (var keyVal in interval.StringValues)
				{
					_writer.WriteUtf8String(keyVal.Key);
					_writer.WriteUtf8String(keyVal.Value);
				}
			}
			else _writer.WriteInt(0);

			if (interval.NonStringValues != null)
			{
				_writer.WriteInt(interval.NonStringValues.Count);

				foreach (var keyVal in interval.NonStringValues)
				{
					_writer.WriteUtf8String(keyVal.Key);
					_writer.WriteUtf8String(keyVal.Value);
				}
			}
			else _writer.WriteInt(0);

			_count++;

		}

		private void Close()
		{
			// write out an empty interval to mark end of intervals
			var emptyInterval = DataStructures.CustomInterval.GetEmptyInterval();
			WriteInterval(emptyInterval);

			Console.WriteLine("Wrote {0} intervals for {1}", _count, _referenceName);

		}

		public void Dispose()
		{
			Close();
			Dispose(true);
		}

		/// <summary>
		/// protected implementation of Dispose pattern. 
		/// </summary>
		private void Dispose(bool disposing)
		{
			lock (this)
			{
				if (_isDisposed) return;

				if (disposing)
				{
					// Free any other managed objects here. 
					_binaryWriter.Dispose();
				}

				// Free any unmanaged objects here. 
				_isDisposed = true;
			}
		}

	}
}
