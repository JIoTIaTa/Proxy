using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Proxy
{
    [Serializable]
    class FileParametrs
    {
        public string FileUrl;
        public string LocalPath;
        public int RowsCount;
        public int AllReqeustsTimeInterval;
        public int OneRequestTimeInterval;
        public string gDriveFileId;
        public FileParametrs()
        {

        }
        public FileParametrs(string FileUrl, string LocalPath, int RowsCount, int allReqeustsTimeInterval, string gDriveFileId, int oneRequestTimeInterval)
        {
            this.FileUrl = FileUrl;
            this.LocalPath = LocalPath;
            this.RowsCount = RowsCount;
            this.AllReqeustsTimeInterval = allReqeustsTimeInterval;
            this.OneRequestTimeInterval = oneRequestTimeInterval;
            this.gDriveFileId = gDriveFileId;
        }
    }
}