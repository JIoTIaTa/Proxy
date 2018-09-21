using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Proxy
{
    [Serializable]
    class SerialazebleParametrs
    {
        private string FileUrl;
        private string localPath;
        private int rowsCount;
        private int allReqeustsTimeInterval;
        private int oneRequestTimeInterval;
        public string gDriveFileId;
        private string iPAddress;
        private int port;
        private string login;
        private string password;
        public string IPAddress
        {
            get { return iPAddress; }
            set { iPAddress = value; }
        }
        public string Login
        {
            get { return login; }
            set { login = value; }
        }
        public int Port
        {
            get { return port; }
            set { port = value; }
        }
        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        public string FileUrl1 { get => FileUrl; set => FileUrl = value; }
        public string LocalPath { get => localPath; set => localPath = value; }
        public int RowsCount { get => rowsCount; set => rowsCount = value; }
        public int AllReqeustsTimeInterval { get => allReqeustsTimeInterval; set => allReqeustsTimeInterval = value; }
        public int OneRequestTimeInterval { get => oneRequestTimeInterval; set => oneRequestTimeInterval = value; }

        public SerialazebleParametrs(string FileUrl, string LocalPath, int RowsCount, int allReqeustsTimeInterval, string gDriveFileId, int oneRequestTimeInterval,
                                     string address, int port, string login, string password)
        {
            this.FileUrl1 = FileUrl;
            this.LocalPath = LocalPath;
            this.RowsCount = RowsCount;
            this.AllReqeustsTimeInterval = allReqeustsTimeInterval;
            this.OneRequestTimeInterval = oneRequestTimeInterval;
            this.gDriveFileId = gDriveFileId;
            this.IPAddress = address;
            this.port = port;
            this.login = login;
            this.password = password;
        }
    }
}