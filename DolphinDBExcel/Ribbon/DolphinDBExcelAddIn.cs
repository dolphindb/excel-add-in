using ExcelDna.Integration;
using ExcelDna.Registration;
using System;
using dolphindb;
using System.Collections.Generic;
using dolphindb.data;
using System.Data;
using System.Data.Common;
using System.Windows;
using System.Windows.Forms;
using DolphinDBForExcel;
public class DolphinDBRunFuctionAddIn : IExcelAddIn
{
    public void AutoOpen()
    {
        ExcelRegistration
            .GetExcelFunctions()
            .ProcessParamsRegistrations()
            .RegisterFunctions();

        // ...
    }

    public void AutoClose()
    {
        // ...
    }

    [ExcelFunction(Name = "RUNFUNCTION", Description = "run the function in DolphinDB", Category = "DolphinDB")]
    public static object[,] RUNFUNCTION(params object[] values)
    {
        try
        {
            object[,] ret = Utils.runFunctionWithFuncName(values);
            return ret;
        }catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            throw;
        }
    }
}


public class Utils
{
    class DataEmptyException : Exception
    {
        public DataEmptyException(string message):base(message)
        {
        }
    };

    class UnsupportTypeException : Exception
    {
        public UnsupportTypeException(string message) : base(message)
        {
        }
    };

    public static object[,] runFunctionWithFuncName(object[] values)
    {
        if(values.Length == 0)
        {
            throw new Exception("RUNFUNCTION: the number of arguments must be greater than 0. ");
        }
        if (!(values[0] is string))
        {
            throw new Exception("RUNFUNCTION: The first argument must be a string of function name.");
        }
        string functionName = values[0] as string;
        return runFunction(functionName, 1, values);
    }

    public static object[,] runFunction(string functionName, int index, object[] values)
    {
        int paramCount = values.Length;
        List<IEntity> param = new List<IEntity>();
        for(int i = index; i < paramCount; i++) 
        {
            param.Add(convertData(values[i], DATA_TYPE.DT_VOID));
        }
        try
        {
            IEntity ret = ConnectionController.Instance.getConnection().run(functionName, param);
            return getData(ret);
        }catch(Exception e)
        {
            System.Windows.MessageBox.Show(e.Message, "error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw e;
        }
    }

    public static object[,] getData(IEntity t)
    {
        dynamic xlApp = ExcelDnaUtil.Application;
        DataTable data;
        if (t.isTable())
        {
            data = ((BasicTable)t).toDataTable();
        }else if (t.isVector())
        {
            data = ((AbstractVector)t).toDataTable();
        }else if (t.isScalar())
        {
            object[,] tmp = new object[1,1];
            tmp[0, 0] = t.getString();
            return tmp;
        }else if (t.isDictionary())
        {
            data = ((BasicDictionary)t).toDataTable();
        }
        else
        {
            throw new Exception("Unsupported data form: " + t.getDataForm().ToString());
        }
        int rows = data.Rows.Count;
        int cols = data.Columns.Count;
        object[,] ret = new object[rows + 1, cols];
        for(int i = 0; i < cols; ++i)
        {
            ret[0, i] = data.Columns[i].ColumnName;
        }
        for(int i = 0; i < rows; ++i)
        {
            DataRow rowData = data.Rows[i];
            for(int j = 0; j < cols; ++j)
            {
                if (rowData[j] is TimeSpan || rowData[j] is DateTime)
                {
                    if (t is BasicTable)
                    {
                        ret[i + 1, j] = ((BasicTable)t).getColumn(j).get(i).getString();
                    }
                    else if (t is AbstractVector)
                    {
                        ret[i + 1, j] = ((AbstractVector)t).get(j).getString();
                    }
                    else
                    {
                        ret[i + 1, j] = rowData[j].ToString();
                    }
                }else if (rowData[j] is DBNull)
                {
                    ret[i + 1, j] = "";
                }
                else
                {
                    ret[i + 1, j] = rowData[j];
                }
            }
        }
        return ret;
    }

    public static DATA_TYPE getDDBDataType(object value)
    {
        if (value is int) { return DATA_TYPE.DT_INT; }
        else if (value is long) { return DATA_TYPE.DT_LONG; }
        else if (value is float) { return DATA_TYPE.DT_FLOAT; }
        else if (value is double) { return DATA_TYPE.DT_DOUBLE; }
        else if (value is string) { return DATA_TYPE.DT_STRING; }
        else if (value is DateTime) { return DATA_TYPE.DT_TIMESTAMP; }
        else return DATA_TYPE.DT_VOID;
    }

    public static IEntity convertData(object data, DATA_TYPE ddbType)
    {
        if(data is object[])
        {
            object[] values = data as object[];
            if(values.Length == 0)
            {
                return null;
            }
            DATA_TYPE type = DATA_TYPE.DT_VOID;
            for (int i = 0; i < values.Length; i++)
            {
                type = getDDBDataType(values[i]);
                if (type != DATA_TYPE.DT_VOID)
                    break;
            }
            if (type == DATA_TYPE.DT_VOID)
                throw new Exception("RUNFUNCTION: each argument must have a specific data type. At least one cell of the argument in Excel is not empty.");
            IVector vector = BasicEntityFactory.instance().createVectorWithDefaultValue(type, 0);
            for(int i = 0; i < values.Length; i++)
            {
                vector.append((IScalar)convertData(values[i], type));
            }
            return vector;
        }
        if(data is object[,])
        {
            object[,] values = data as object[,];
            if (values.Length == 0)
            {
                return null;
            }
            DATA_TYPE type = DATA_TYPE.DT_VOID;
            for (int column = 0; column < values.GetLength(1); ++column)
            {
                for (int row = 0; row < values.GetLength(0); ++row)
                {
                    type = getDDBDataType(values[row, column]);
                    if (type != DATA_TYPE.DT_VOID)
                        break;
                }
                if (type != DATA_TYPE.DT_VOID)
                    break;
            }
            if (type == DATA_TYPE.DT_VOID)
                throw new Exception("RUNFUNCTION: each argument must have a specific data type. At least one cell of the argument in Excel is not empty.");
            IVector vector = BasicEntityFactory.instance().createVectorWithDefaultValue(type, 0);
            for (int column = 0; column < values.GetLength(1); column++)
            {
                for(int row = 0; row < values.GetLength(0); ++row)
                {
                    vector.append((IScalar)convertData(values[row, column], type));
                }
            }
            return vector;
        }
        if(data is int)
        {
            return new BasicInt((int)data);
        }else if(data is long)
        {
            return new BasicLong((long)data);
        }else if(data is double)
        {
            return new BasicDouble((double)data);
        }else if (data is float)
        {
            return new BasicFloat((float)data);
        }else if (data is string)
        {
            return new BasicString((string)data);
        }
        else if (data is DateTime)
        {
            return new BasicTimestamp((DateTime)data);
        }else if(data is ExcelDna.Integration.ExcelEmpty && ddbType != DATA_TYPE.DT_VOID)
        {
            IScalar ret = BasicEntityFactory.instance().createScalarWithDefaultValue(ddbType);
            ret.setNull();
            return ret;
        }
       else
        {
            throw new Exception("Unsupported data type: " + data.GetType().ToString());
        }
    }

}