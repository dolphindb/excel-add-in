using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;

namespace DolphinDBForExcel.WPFControls
{
    public partial class DDBScriptEditor : UserControl
    {
        [Serializable]
        public class Config : ICloneable
        {
            public static readonly string CfgSaveFile = "EditorCfg.xml";

            public string fontSource;
            public double fontSize;
            public double lineHeight;
            public bool overwrite;
            public int maxRowsToImportInto;
            public bool autoLimitMaxRowsToImport;
       
            public object Clone()
            {
                return new Config
                {
                    fontSource = fontSource,
                    fontSize = fontSize,
                    lineHeight = lineHeight,
                    overwrite = overwrite,
                    maxRowsToImportInto = maxRowsToImportInto,
                    autoLimitMaxRowsToImport = autoLimitMaxRowsToImport
                };
            }

            public static Config ReadConfigFromDefaultFile()
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Config));
                using (FileStream fs = FileUtil.DataFolder.OpenReadFile(CfgSaveFile))
                {
                    return serializer.Deserialize(XmlReader.Create(fs)) as Config;
                }
            }

            public void SaveConfigToDefaultFile()
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Config));
                using (FileStream fs = FileUtil.DataFolder.CreateFile(CfgSaveFile))
                {
                    serializer.Serialize(XmlWriter.Create(fs), this);
                }
            }
        }


    }
}
