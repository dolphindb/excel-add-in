using DolphinDBForExcel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DolphinDBForExcel
{

    [Serializable]
    public class ServerInfo : ICloneable
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Host { get; set; }
        [XmlAttribute]
        public int Port { get; set; }

        [XmlAttribute]
        public string Username { get; set; }

        [XmlAttribute]
        public string Password { get; set; }

        public object Clone()
        {
            return new ServerInfo
            {
                Name = Name,
                Host = Host,
                Port = Port,
                Username = Username,
                Password = Password
            };
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            ServerInfo sinfo = obj as ServerInfo;
            if (sinfo == null)
                return false;

            return string.Equals(sinfo.Host, Host) && sinfo.Port.Equals(Port);
        }

        public override int GetHashCode()
        {
            if (Host == null)
                return Port.GetHashCode();
            return Port.GetHashCode() ^ Host.GetHashCode();
        }


    }

    public class ServerInfosXmlSerializer
    {

        [Serializable]
        public class _ServerInfoList
        {
            [XmlArray]
            public List<ServerInfo> items;

            public static _ServerInfoList FromList(List<ServerInfo> items)
            {
                return new _ServerInfoList
                {
                    items = items
                };
            }
        }

        public static void ToXmlFile(List<ServerInfo> serverInfos, string filename)
        {
            using (FileStream fs = FileUtil.CreateFile(filename))
            {
                Serialize(serverInfos, XmlWriter.Create(fs));
            }
        }

        public static List<ServerInfo> FromXmlFile(string filename)
        {
            using (FileStream fs = FileUtil.OpenReadFile(filename))
            {
                return Deserialize(XmlReader.Create(fs));
            }
        }

        public static void Serialize(List<ServerInfo> serverInfos, XmlWriter writer)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(_ServerInfoList));
            serializer.Serialize(writer, _ServerInfoList.FromList(serverInfos));
        }

        public static List<ServerInfo> Deserialize(XmlReader reader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(_ServerInfoList));
            _ServerInfoList s = serializer.Deserialize(reader) as _ServerInfoList;

            if (s == null || s.items == null)
                return new List<ServerInfo>();

            return s.items;
        }
    }

}
