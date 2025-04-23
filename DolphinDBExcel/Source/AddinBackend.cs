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

    class TableResult
    {
        public DataTable table;
        public IList<DATA_TYPE> columnSrcType;
        public DATA_FORM srcForm;
        public IList<string> matrix_ColumnLabels;
        public IList<string> matrix_RowLabels;
    }

    class AddinBackend
    {
        public static IEntity RunScript(DBConnection conn, string script)
        {
            IEntity entity = conn.run(script);
            return entity;
        }

        public static bool IsConnected(DBConnection conn)
        {
            return conn.isConnected;
        }

        public static bool IsBusy(DBConnection conn)
        {
            return conn.isConnected ? conn.isBusy() : false;
        }

        public static DataTable RunScriptAndFetchResultAsTable(DBConnection conn, string script)
        {
            BasicTable tb = RunScriptAndFetchResultAsBasicTable(conn, script);
            return tb.toDataTable();
        }

        private static IList<DbObjectInfo> UpdateSessionObjs(DBConnection conn)
        {
            BasicTable objs = (BasicTable)conn.tryRun("objs(true)");
            if (objs == null)
                return null;

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
            return listObjs;
        }

        public static IList<DbObjectInfo> TryToGetObjsInfo(DBConnection conn)
        {
            if (!conn.isConnected)
                return null;

            return UpdateSessionObjs(conn);
        }

        /*
         *  return null if get DT_VOID
         *  return table or throw exception if not DT_VOID
         */
        public static TableResult RunScriptAndFetchResultAsDataTable(DBConnection conn, string script)
        {
            IEntity entity = RunScript(conn, script);

            if (entity.getDataType() == DATA_TYPE.DT_VOID)
                return null;

            TableResult result = new TableResult
            {
                srcForm = entity.getDataForm(),
                columnSrcType = new List<DATA_TYPE>()
            };
            result.table = entity.toDataTable();

            if (entity.isTable())
            {
                BasicTable basicTable = entity as BasicTable;
                for (int i = 0; i != basicTable.columns(); i++)
                    result.columnSrcType.Add(basicTable.getColumn(i).getDataType());

                return result;
            }else if (entity.isDictionary())
            {
                BasicDictionary basicDictionary = entity as BasicDictionary;
                result.columnSrcType.Add(basicDictionary.KeyDataType);
                result.columnSrcType.Add(basicDictionary.getDataType());
                return result;
            }else if (entity.isMatrix())
            {
                for (int i = 0; i != result.table.Columns.Count; i++)
                    result.columnSrcType.Add(entity.getDataType());
                IMatrix m = entity as IMatrix;
                IVector colLabels = m.getColumnLabels();
                if (!(colLabels == null || colLabels.columns() == 0))
                {
                    result.matrix_ColumnLabels = new List<string>();
                    for (int i = 0; i != colLabels.rows(); i++)
                        result.matrix_ColumnLabels.Add(colLabels.get(i).getString());
                }

                IVector rowLabels = m.getRowLabels();
                if (!(rowLabels == null || rowLabels.columns() == 0))
                {
                    result.matrix_RowLabels = new List<string>();
                    for (int i = 0; i != rowLabels.rows(); i++)
                        result.matrix_RowLabels.Add(rowLabels.get(i).getString());
                }

            }else if (entity.isScalar())
            {
                result.columnSrcType.Add(entity.getDataType());
            }
            else
            {
                throw new Exception("Unsupported data form: " + entity.getDataForm().ToString());
            }

            return result;
        }

        public static BasicTable RunScriptAndFetchResultAsBasicTable(DBConnection conn, string script)
        {
            IEntity tb = RunScript(conn, script);

            if (!tb.isTable())
                throw new ArgumentException("Can't get table from script");
            return (BasicTable)tb;
        }
    }
}
