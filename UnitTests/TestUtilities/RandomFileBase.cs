using System;
using System.Collections.Generic;
using System.IO;

namespace UnitTests.TestUtilities
{
	public abstract class RandomFileBase : IDisposable
	{
		#region members

		private readonly LinkedList<string> _randomFiles = new LinkedList<string>();

		#endregion

		/// <summary>
		/// returns a random filename
		/// </summary>
		protected string GetRandomPath(bool includeIndex = false)
		{
			var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			_randomFiles.AddLast(randomPath);
			if (includeIndex) _randomFiles.AddLast(randomPath + ".idx");
			return randomPath;
		}

		/// <summary>
		/// deletes the specified file
		/// </summary>
		public void Dispose()
		{
			GC.Collect();

			while (_randomFiles.Count > 0)
			{
				var file = _randomFiles.First.Value;
				_randomFiles.RemoveFirst();

				// skip files that no longer exist
				if (!File.Exists(file)) continue;

				// try deleting the file
				try
				{
					File.Delete(file);
				}
				catch (IOException)
				{
					_randomFiles.AddLast(file);
				}
			}
		}
	}
}