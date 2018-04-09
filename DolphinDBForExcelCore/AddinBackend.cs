using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DolphinDBForExcelCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net;
    using System.Data;
    using System.IO;
    using dolphindb;
    using dolphindb.data;

    namespace DolphinDBForExcel
    {

        public class DbObjectInfo
        {
            public string name;
            public string type;
            public string forms;
            public int rows;
            public int columns;
            public long bytes;
            public bool shared;
        }

        class AddinBackend : Singleton<AddinBackend>
        {
            private DBConnection conn = new DBConnection();

            private IList<DbObjectInfo> sessionObjs = null;

            public void ResetConnection(string ip, int port, string username, string password)
            {
                conn.close();
                if (!conn.connect(ip, port))
                    throw new WebException("connect failed");
            }

            public void GetHostNameAndPort(out string hostName, out int port)
            {
                if (conn.isConnected)
                {
                    hostName = conn.HostName;
                    port = conn.Port;
                }
                else
                {
                    hostName = "";
                    port = -1;
                }
            }

            public IEntity RunScript(string script)
            {
                IEntity entity = conn.run(script);
                UpdateSessionObjs();
                return entity;
            }

            public bool IsConnected()
            {
                return conn.isConnected;
            }

            public bool IsBusy()
            {
                return conn.isConnected ? conn.isBusy() : false;
            }

            public DataTable RunScriptAndFetchResultAsTable(string script)
            {
                BasicTable tb = RunScriptAndFetchResultAsBasicTable(script);
                return tb.toDataTable();
            }

            private void UpdateSessionObjs()
            {
                sessionObjs = null;
                BasicTable objs = (BasicTable)conn.tryRun("objs(true)");
                if (objs == null)
                    return;

                var listObjs = new List<DbObjectInfo>(objs.rows());

                for (int i = 0; i != objs.rows(); i++)
                {
                    DbObjectInfo obj = new DbObjectInfo
                    {
                        name = objs.getColumn("name").get(i).getString(),
                        type = objs.getColumn("type").get(i).getString(),
                        forms = objs.getColumn("form").get(i).getString(),
                        rows = (objs.getColumn("rows").get(i) as BasicInt).getValue(),
                        columns = (objs.getColumn("columns").get(i) as BasicInt).getValue(),
                        shared = (objs.getColumn("shared").get(i) as BasicBoolean).getValue(),
                        bytes = (objs.getColumn("bytes").get(i) as BasicLong).getValue()
                    };

                    listObjs.Add(obj);
                }
                sessionObjs = listObjs;
            }

            public IList<DbObjectInfo> TryToGetObjsInfo()
            {
                if (!conn.isConnected)
                    return null;

                UpdateSessionObjs();
                return sessionObjs;
            }

            public void RunScriptAndFetchResultAsDataTable(string script, out DataTable tb, out IList<DATA_TYPE> columsSrcType)
            {
                IEntity entity = RunScript(script);

                if (entity.isTable())
                {
                    BasicTable basicTable = entity as BasicTable;
                    columsSrcType = new List<DATA_TYPE>(basicTable.columns());

                    for (int i = 0; i != basicTable.columns(); i++)
                        columsSrcType.Add(basicTable.getColumn(i).getDataType());

                    tb = basicTable.toDataTable();
                    return;
                }

                tb = entity.toDataTable();
                columsSrcType = new List<DATA_TYPE>(tb.Columns.Count);

                if (entity.isDictionary())
                {
                    BasicDictionary basicDictionary = entity as BasicDictionary;
                    columsSrcType.Add(basicDictionary.KeyDataType);
                    columsSrcType.Add(basicDictionary.getDataType());
                    return;
                }

                for (int i = 0; i != tb.Columns.Count; i++)
                    columsSrcType.Add(entity.getDataType());
            }

            public BasicTable RunScriptAndFetchResultAsBasicTable(string script)
            {
                IEntity tb = RunScript(script);

                if (!tb.isTable())
                    throw new ArgumentException("Can't get table from script");
                return (BasicTable)tb;
            }
        }
    }

}
