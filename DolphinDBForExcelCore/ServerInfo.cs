using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DolphinDBForExcelCore
{

    [Serializable]
    public class ServerInfo : ICloneable
    {
        [XmlAttribute]
        public string Ip { get; set; }
        [XmlAttribute]
        public int Port { get; set; }

        public object Clone()
        {
            return new ServerInfo
            {
                Ip = Ip,
                Port = Port,
            };
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
