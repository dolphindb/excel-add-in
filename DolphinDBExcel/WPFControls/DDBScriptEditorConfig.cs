using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace DolphinDBForExcel.WPFControls
{
    public class DDBScriptEditorConfig
    {
        [Serializable]
        public class Config : ICloneable
        {
            public static readonly string CfgSaveFile = "EditorCfg.xml";

            public string fontSource;
            public double fontSize;
            public bool overwrite;
            public int maxRowsToImportInto;
            public bool autoLimitMaxRowsToImport;

            public object Clone()
            {
                return new Config
                {
                    fontSource = fontSource,
                    fontSize = fontSize,
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
