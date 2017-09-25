using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ErrorHandling.Exceptions;

namespace SAUtils.InputFileParsers.MitoMAP
{
    public class MitoMapSvReader
    {
        private readonly FileInfo _mitoMapFileInfo;

        private readonly Dictionary<string, int[]> _mitoMapSvDataTypes = new Dictionary<string, int[]>()
        {
            {"DeletionsSingle", new[] {0, 2, 3, 6, 7, 8, -1}},
            {"InsertionsSimple", new[] {0, 2, 3, 5, 6, 7, 8}}
        };

        public MitoMapSvReader(FileInfo mitoMapFileInfo)
        {
            _mitoMapFileInfo = mitoMapFileInfo;
        }

        private string GetDataType()
        {
            var dataType = _mitoMapFileInfo.Name.Replace(".html", null);
            if (!_mitoMapSvDataTypes.ContainsKey(dataType)) throw new InvalidFileFormatException($"Unexpected data file: {_mitoMapFileInfo.Name}");
            return dataType;
        }
    }
}
