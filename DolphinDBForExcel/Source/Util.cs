using dolphindb.data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace DolphinDBForExcel
{
    static class Util
    {

        public static string ConvTimeSpanToString(TimeSpan span)
        {
            string s = null;
            if (span == null)
                return null;

            int ms;
            int seconds;
            int minutes;
            int hours;
            int days;

            ms = Convert.ToInt32(span.TotalMilliseconds);

            s = (ms % 1000).ToString() + "ms";
            if ((seconds = ms / 1000) == 0)
                return s;
            s = (seconds % 60).ToString() + "s " + s;

            if ((minutes = seconds / 60) == 0)
                return s;
            s = (minutes % 60).ToString() + "m " + s;

            if ((hours = minutes / 60) == 0)
                return s;
            s = (hours % 60).ToString() + "h " + s;

            if ((days = hours / 24) == 0)
                return s;
            s = (days % 60).ToString() + "d " + s;
            return s;
        }
    }
    static class ListCloner 
    {
        public static List<T> Copy<T>(IList<T> src) where T : ICloneable
        {
            if (src == null)
                return null;

            List<T> result = new List<T>(src.Count);
            foreach (var v in src)
                result.Add(v == null ? v : (T)v.Clone());
            return result;
        }
    }

    static class SQLGenerator
    {
        public class Agg1
        {
            public string func;
            public string arg1;
        };

        public class Agg2
        {
            public string func;
            public string arg1;
            public string arg2;
        }

        static public string Make(string[] selectFields,string from,string where)
        {
            StringBuilder builder = new StringBuilder("select  ");

            AppendSelectField(builder, selectFields, false);

            builder.Append(" from ").Append(from);

            if (where == null || where == "")
                return builder.ToString();

            return builder.Append(" where ").Append(where).ToString();
        }

        static public string Make(Agg1[] agg1,Agg2[] agg2,string from,string groupBy ,string where)
        {
            StringBuilder builder = new StringBuilder("select ");

            AppendAgg1(builder, agg1, agg2 != null);
            AppendAgg2(builder, agg2, false);

            builder.Append(" from ").Append(from);
            builder.Append(" group by ").Append(groupBy);

            if (where == null || where == "")
                return builder.ToString();

            return builder.Append(" where ").Append(where).ToString();
        }

        static private void AppendAgg1(StringBuilder builder,Agg1[] agg1,bool appendSeperateSym)
        {
            if (agg1 == null)
                return;

            foreach (var a in agg1)
                builder.Append(a.func).Append('(').Append(a.arg1).Append("),");

            builder.Remove(builder.Length - 1, 1);
            builder.Append(appendSeperateSym ? ',' : ' ');
        }

        static private void AppendSelectField(StringBuilder builder, string[] selectField, bool appendSeperateSym)
        {
            if (selectField == null)
                return;

            foreach (var a in selectField)
                builder.Append(a).Append(" ,");

            builder.Remove(builder.Length - 1, 1);
            builder.Append(appendSeperateSym ? ',' : ' ');
        }

        static private void AppendAgg2(StringBuilder builder, Agg2[] agg2, bool appendSeperateSym)
        {
            if (agg2 == null)
                return;

            foreach (var a in agg2)
                builder.Append(a.func).Append('(').Append(a.arg1).Append(',').Append(a.arg2).Append("),");

            builder.Remove(builder.Length - 1, 1);
            builder.Append(appendSeperateSym ? ',' : ' ');
        }
    }

    class ExcelWin32Window : IWin32Window
    {
        public ExcelWin32Window(int winHwnd)
        {
            Handle = new IntPtr(winHwnd);
        }

        public static ExcelWin32Window ActivateWin
        {
            get { return new ExcelWin32Window(Globals.ThisAddIn.Application.Hwnd); }
        }

        public IntPtr Handle { get; private set; }
    }

    static class FileUtil
    {
        static public class DataFolder
        {
            public static readonly string Dir =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "DolphinDBForExcel");

            public static string FullFilePath(string filename)
            {
                return Path.Combine(Dir, filename);
            }

            public static FileStream OpenReadFile(string filename)
            {
                return FileUtil.OpenReadFile(FullFilePath(filename));
            }

            public static FileStream CreateFile(string filename)
            {
                return FileUtil.CreateFile(FullFilePath(filename));
            }
        }

        public static FileStream OpenReadFile(string filename)
        {
            return File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public static FileStream CreateFile(string filename)
        {
            string dir = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            return File.Create(filename);
        }
    }

    static class DDBExcelNumericFormater
    {
        public static string GetFormat(DATA_TYPE dtype)
        {
            switch(dtype)
            {
                case DATA_TYPE.DT_DATE:
                    return "yyyy/m/d";
                case DATA_TYPE.DT_MONTH:
                    return "yyyy/m";
                case DATA_TYPE.DT_TIME:
                    return "hh:mm:ss";
                case DATA_TYPE.DT_MINUTE:
                    return "hh:mm";
                case DATA_TYPE.DT_SECOND:
                case DATA_TYPE.DT_NANOTIME:
                    return "hh:mm:ss";
                case DATA_TYPE.DT_DATETIME:
                case DATA_TYPE.DT_TIMESTAMP:
                case DATA_TYPE.DT_NANOTIMESTAMP:
                    return "yyyy/m/d hh:mm:ss";
                case DATA_TYPE.DT_ANY:
                    return "@";
            }
            return "General";
        }
    }

    static class BitmapToBitmapSource
    {
        public static BitmapSource Conv(Bitmap bitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(), IntPtr.Zero,System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
    }

    enum ByteUnit { B=1,KB=2,MB=3,GB=4,Limit=5 }

    static class ByteConverter
    {
        public static void ConvToNearsetUnit(long num,ByteUnit unit,out long newNum,out ByteUnit newUnit)
        {
            long n = num;
            ByteUnit u = unit;

            while (n >= 1024 && u + 1 < ByteUnit.Limit)
            {
                n /= 1024;
                u += 1;
            }

            newNum = n;
            newUnit = u;
        }
    }

    static class DDBString
    {
        public const  string TableForm = "TABLE";

        public const string MatrixForm = "MATRIX";

        public const string VectorForm = "VECTOR";

        public const string DictionaryForm = "DICTIONARY";

        public const string SetForm = "SET";

        public const string ScalarForm = "SCALAR";

        public const string PairForm = "PAIR";

        public static string FirstLetterToUpper(string s)
        {
            if (s == null)
                return null;

            if (s.Length > 1)
                return char.ToUpper(s[0]) + s.Substring(1);

            return s.ToUpper();
        }

        public static string GetValueAsStringIfScalarOrPair(IEntity entity)
        {
            if (entity == null)
                return null;

            if ((!entity.isScalar()) && (!entity.isPair()))
                return null;

            return entity.getString();
        }
    }
}
