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
        public int TimerInterval;
        public string gDriveFileId;
        public FileParametrs()
        {

        }
        public FileParametrs(string FileUrl, string LocalPath, int RowsCount, int TimerInterval, string gDriveFileId)
        {
            this.FileUrl = FileUrl;
            this.LocalPath = LocalPath;
            this.RowsCount = RowsCount;
            this.TimerInterval = TimerInterval;
            this.gDriveFileId = gDriveFileId;
        }
    }
}