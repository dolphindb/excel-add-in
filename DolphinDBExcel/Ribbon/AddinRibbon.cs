using ExcelDna.Integration.CustomUI;
using System.Data.Common;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DolphinDBForExcel;
using System.Collections.Generic;
using ExcelDna.Integration;
using DolphinDBForExcel.Forms;
using DolphinDBForExcel.WPFControls;
using System;
using static DolphinDBForExcel.WPFControls.DDBScriptEditorConfig;
namespace DolphinDBForExcel.Ribbon
{
    [ComVisible(true)]
    public class AddinRibbon : ExcelRibbon
    {
        [ComVisible(true)]
        public class RibbonController : ExcelRibbon
        {
            public override string GetCustomUI(string RibbonID)
            {
                return DolphinDBForExcel.Resource.serverSelect;
            }

            //public void connectToServer(IRibbonControl control)
            //{
            //    using (.Forms.Connection editServer = new .Forms.Connection())
            //    {
            //        editServer.ShowDialog();
            //    }
            //    _ribbon.Invalidate();
            //}

            public int getConfigServerItemCount(IRibbonControl control)
            {
                return 2;
            }

            public string getConfigServerLabel(IRibbonControl control, int index)
            {
                if(index == 0)
                {
                    return "Add Server";
                }
                else
                {
                    return "Edit Server";
                }
            }

            public void actionConfigServer(IRibbonControl control, string value, int index)
            {
                if(index == 0)
                {
                    DolphinDBForExcel.Forms.AddServer addServer = new DolphinDBForExcel.Forms.AddServer();
                    addServer.TopMost = true;
                    addServer.Show();
                }
                else
                {
                    DolphinDBForExcel.Forms.EditServer editServer = new DolphinDBForExcel.Forms.EditServer();
                    editServer.TopMost = true;
                    editServer.Show();
                }
                ribbon_.Invalidate();
            }

            public void queryFromServer(IRibbonControl control)
            {
                DolphinDBForExcel.Forms.ScriptEditor scriptEditor = new DolphinDBForExcel.Forms.ScriptEditor();
                scriptEditor.Show();
            }

            public void config(IRibbonControl control)
            {
                DolphinDBForExcel.Forms.ScriptEditorConfiguration scriptEditor = new DolphinDBForExcel.Forms.ScriptEditorConfiguration();
                scriptEditor.Show();
            }

            public override object LoadImage(string id)
            {
                if(id == "server")
                {
                    return DolphinDBForExcel.Resource.server40;
                }
                else if(id == "config"){
                    return DolphinDBForExcel.Resource.config40;
                }
                else
                {
                    return DolphinDBForExcel.Resource.query40;
                }
            }

            public int getServerListItemCount(IRibbonControl control)
            {
                List<ServerInfo> infos = ConnectionController.Instance.LoadServerInfos();
                return infos.Count;
            }

            public string getServerListLabel(IRibbonControl control, int index)
            {
                List<ServerInfo> infos = ConnectionController.Instance.LoadServerInfos();
                return infos[index].Name;
            }

            public int getServerListIndexDefault(IRibbonControl control)
            {
                List<ServerInfo> infos = ConnectionController.Instance.LoadServerInfos();
                ServerInfo info = ConnectionController.Instance.getCurrentServerInfo();
                for(int i = 0; i <  infos.Count; i++)
                {
                    ServerInfo info2 = infos[i];
                    if(info.Name == info2.Name)
                    {
                        return i;
                    }
                }
                return 0;
            }

            public void reconnectServer(IRibbonControl control, string value, int index)
            {
                List<ServerInfo> infos = ConnectionController.Instance.LoadServerInfos();
                ServerInfo info2 = infos[index];
                ConnectionController.Instance.setConnection(info2);
            }

            public void OnRibbonLoad(IRibbonUI ribbon)

            {
                InitEditerConfigValue();
                ribbon_ = ribbon;
            }

            public static void Invalidate()

            {
                ribbon_.Invalidate();
            }

            private void InitEditerConfigValue()
            {
                try
                {
                    CONFIG = Config.ReadConfigFromDefaultFile();
                }
                catch (Exception)
                {
                    CONFIG = new Config
                    {
                        fontSource = "Microsoft YaHei UI",
                        fontSize = 14,
                        overwrite = false,
                        maxRowsToImportInto = 65536,
                        autoLimitMaxRowsToImport = true
                    };
                }
            }

            static IRibbonUI ribbon_;
            public static string VERSION = "V1.0.0 2024.07.19";
            public static DDBScriptEditorConfig.Config CONFIG;
        }
    }
}
