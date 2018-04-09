using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security;
using DolphinDBForExcel;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace DolphinDBForExcelTest
{
    [TestFixture]
    public class TestClass
    {
        [Test]
        public void TestServerInfoSerializerWithNull()
        {
            MemoryStream input;
            MemoryStream output;
            List<ServerInfo> newServers;

            output = new MemoryStream();
            ServerInfosXmlSerializer.Serialize(null, XmlWriter.Create(output));

            input = new MemoryStream(output.ToArray());
            newServers = ServerInfosXmlSerializer.Deserialize(XmlReader.Create(input));

            Assert.AreEqual(newServers.Count, 0);

            output = new MemoryStream();
            ServerInfosXmlSerializer.Serialize(null, XmlWriter.Create(output));

            input = new MemoryStream(output.ToArray());
            newServers = ServerInfosXmlSerializer.Deserialize(XmlReader.Create(input));

            Assert.AreEqual(newServers.Count, 0);
        }

        [Test]
        public void TestServerInfoSerializerWithEmptyList()
        {
            List<ServerInfo> servers = new List<ServerInfo>();

            MemoryStream output = new MemoryStream();
            ServerInfosXmlSerializer.Serialize(servers, XmlWriter.Create(output));

            MemoryStream input = new MemoryStream(output.ToArray());
            List<ServerInfo> newServers = ServerInfosXmlSerializer.Deserialize(XmlReader.Create(input));

            Assert.AreEqual(newServers.Count, 0);
        }

        [Test]
        public void TestServerInfoSerializerWithNormalList()
        {
            List<ServerInfo> servers = new List<ServerInfo>();
            ServerInfo s = new ServerInfo
            {
                Port = 5,
                Host = "127"
            };
            servers.Add(s);
            servers.Add(s);
            servers.Add(s);

            MemoryStream output = new MemoryStream();
            ServerInfosXmlSerializer.Serialize(servers, XmlWriter.Create(output));
            MemoryStream input = new MemoryStream(output.ToArray());
            List<ServerInfo> newServers = ServerInfosXmlSerializer.Deserialize(XmlReader.Create(input));

            Assert.AreEqual(newServers.Count, servers.Count);
          
            Assert.AreEqual(newServers[0].Port, 5);
            Assert.AreEqual(newServers[0].Host, "127");

          
            Assert.AreEqual(newServers[1].Port, 5);
            Assert.AreEqual(newServers[1].Host, "127");
            
                                                 
            Assert.AreEqual(newServers[2].Port, 5);
            Assert.AreEqual(newServers[2].Host, "127");
        }

        [Test]
        public void TestServerInfoSerializerWithFile()
        {
            List<ServerInfo> servers = new List<ServerInfo>();
            ServerInfo s = new ServerInfo
            {
              
                Port = 5,
                Host = "127"
            };
            servers.Add(s);

            string temp = Environment.GetEnvironmentVariable("TEMP");

            XmlWriter writer = XmlWriter.Create(temp+"/test.xml");
            ServerInfosXmlSerializer.Serialize(servers, writer);

            FileStream fs = File.Open(temp + "/test.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            XmlReader reader = XmlReader.Create(fs);
            List<ServerInfo> newServers = ServerInfosXmlSerializer.Deserialize(reader);

            Assert.AreEqual(newServers.Count, servers.Count);
         
            Assert.AreEqual(newServers[0].Port, 5);
            Assert.AreEqual(newServers[0].Host, "127");
        }

        [Test]
        public void TestServerInfoDerializerWithErrorContent()
        {

            ServerInfosXmlSerializer.FromXmlFile(FileUtil.DataFolder.FullFilePath("servers.xml"));
        }
    }
}
