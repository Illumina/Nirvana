using System;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling.CustomInterval
{
	public sealed class CustomIntervalWriter : IDisposable
	{
		private readonly ExtendedBinaryWriter _writer;
		private readonly string _referenceName;
		private readonly string _intervalType;
		private int _count;
		private readonly DataSourceVersion _version;

		private bool _isDisposed;//for dispose pattern
								 // constructor
		public CustomIntervalWriter(string fileName, string referenceName, string intervalType, DataSourceVersion version)
		{
			var stream = FileUtilities.GetCreateStream(fileName);
			_writer = new ExtendedBinaryWriter(stream);

			_referenceName = referenceName;
			_intervalType  = intervalType;
			_version       = version;

			WriteHeader();
		}

		private void WriteHeader()
		{
			_writer.Write(CustomIntervalCommon.DataHeader);
            _writer.Write(CustomIntervalCommon.SchemaVersion);
            _writer.Write(DateTime.UtcNow.Ticks);
            _writer.Write(_referenceName);
            _writer.Write(_intervalType);

			_version.Write(_writer);

            // marking end of header with guard int
            _writer.Write(CustomIntervalCommon.GuardInt);
		}

		public void WriteInterval(DataStructures.CustomInterval interval)
		{
			if (interval.IsEmpty())
			{
				// marks the last interval
				_writer.WriteOpt(interval.Start);
				_writer.WriteOpt(interval.End);
				_writer.WriteOpt(0);// for the null string dictionary
				_writer.WriteOpt(0);// for the null non-string dictionary
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

			_writer.WriteOpt(interval.Start);
			_writer.WriteOpt(interval.End);

			if (interval.StringValues != null)
			{
				_writer.WriteOpt(interval.StringValues.Count);

				foreach (var keyVal in interval.StringValues)
				{
					_writer.WriteOptUtf8(keyVal.Key);
					_writer.WriteOptUtf8(keyVal.Value);
				}
			}
			else _writer.WriteOpt(0);

			if (interval.NonStringValues != null)
			{
				_writer.WriteOpt(interval.NonStringValues.Count);

				foreach (var keyVal in interval.NonStringValues)
				{
					_writer.WriteOptUtf8(keyVal.Key);
					_writer.WriteOptUtf8(keyVal.Value);
				}
			}
			else _writer.WriteOpt(0);

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
                    _writer.Dispose();
				}

				// Free any unmanaged objects here. 
				_isDisposed = true;
			}
		}

	}
}
