using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using dolphindb;

namespace DolphinDBForExcel.WPFControls
{
    public partial class DDBScriptEditor : UserControl
    {
        private class ObjectViewItem : INotifyPropertyChanged
        {
            bool _IsExpanded = false;

            public bool IsExpanded
            {
                get { return _IsExpanded; }
                set
                {
                    if (_IsExpanded == value)
                        return;
                    _IsExpanded = value;
                    OnPropertyChanged("IsExpanded");
                    IconImage = _IsExpanded ? ExpandImage : UnExpandImage;
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            ObservableCollection<ObjectViewItem> _Children = new ObservableCollection<ObjectViewItem>();

            public ObservableCollection<ObjectViewItem> Children
            {
                get { return _Children; }
                set
                {
                    if (_Children == value)
                        return;
                    _Children = value;
                    OnPropertyChanged("Children");
                }
            }

            string _Header = "";

            public string Header
            {
                get { return _Header; }
                set
                {
                    if (_Header.Equals(value))
                        return;
                    _Header = value;
                    OnPropertyChanged("Header");
                }
            }

            public Object Tag { get; set; }

            public BitmapSource ExpandImage;

            public BitmapSource UnExpandImage;

            BitmapSource _IconImage;

            public BitmapSource IconImage
            {
                get { return _IconImage; }
                set
                {
                    if (_IconImage == value)
                        return;
                    _IconImage = value;
                    OnPropertyChanged("IconImage");
                }
            }

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        class ObjectViewTreeHelper
        {
            public static readonly string SharedTableHeader = "Shared Tables";
            public static readonly string LocalVariablesHeader = "Local Variables";

            public static BitmapSource docBitSource = BitmapToBitmapSource.Conv(Resource.document);
            public static BitmapSource folderBitSource = BitmapToBitmapSource.Conv(Resource.folder);
            public static BitmapSource openedFolderBitSource = BitmapToBitmapSource.Conv(Resource.opened_folder);

            private void ExpandItemIfHasNewChildern(ObjectViewItem item, IList<ObjectViewItem> newSubItem)
            {
                foreach (var subitem in newSubItem)
                {
                    if (item.Children.FirstOrDefault(p => p.Header.Equals(subitem.Header)) == null)
                    {
                        item.IsExpanded = true;
                        return;
                    }
                }
            }
            private void ExpandItemIfChildrenIsExpanded(ObjectViewItem item)
            {
                foreach (var subitem in item.Children)
                {
                    if (subitem.IsExpanded)
                    {
                        item.IsExpanded = true;
                        return;
                    }
                }
            }
            public void UpdateObjectViewItem(DBConnection conn, IList<ObjectViewItem> items)
            {
                IList<DbObjectInfo> dbObjs = null;
                try
                {
                    dbObjs = AddinBackend.TryToGetObjsInfo(conn);
                }
                catch (Exception)
                {
                    return;
                }

                if (dbObjs == null)
                    return;

                ObjectViewItem localVarItems = items.FirstOrDefault(
                    p => p.Header.Equals(LocalVariablesHeader));
                ObjectViewItem sharedTableItems = items.FirstOrDefault(
                    p => p.Header.Equals(SharedTableHeader));

                if (localVarItems == null)
                {
                    localVarItems = new ObjectViewItem
                    {
                        Header = LocalVariablesHeader,
                        IsExpanded = false,
                        IconImage = folderBitSource,
                        ExpandImage = openedFolderBitSource,
                        UnExpandImage = folderBitSource
                    };
                    items.Insert(0, localVarItems);
                }

                if (sharedTableItems == null)
                {
                    sharedTableItems = new ObjectViewItem
                    {
                        Header = SharedTableHeader,
                        IsExpanded = false,
                        IconImage = folderBitSource,
                        ExpandImage = openedFolderBitSource,
                        UnExpandImage = folderBitSource
                    };
                    items.Insert(1, sharedTableItems);
                }

                UpdateLocalVariables(dbObjs, localVarItems);
                UpdateSharesTable(dbObjs, sharedTableItems);
            }

            private void UpdateLocalVariables(IList<DbObjectInfo> dbObjs, ObjectViewItem localVarItems)
            {
                List<ObjectViewItem> newItems = new List<ObjectViewItem>();

                foreach (var objWithSameForm in dbObjs.Where(p => !p.shared).GroupBy(p => p.forms))
                {
                    string form = objWithSameForm.Key;
                    string formHeader = DDBString.FirstLetterToUpper(form.ToLower());

                    ObjectViewItem formItem = localVarItems.Children.FirstOrDefault(
                    p => p.Header.Equals(formHeader));

                    if (formItem == null)
                        formItem = new ObjectViewItem
                        {
                            Header = formHeader,
                            IsExpanded = false,
                            IconImage = folderBitSource,
                            ExpandImage = openedFolderBitSource,
                            UnExpandImage = folderBitSource
                        };

                    var newVariableItem = new ObservableCollection<ObjectViewItem>();
                    foreach (var obj in objWithSameForm)
                        newVariableItem.Add(new ObjectViewItem
                        {
                            Header = FormatDbObjectStr(obj),
                            Tag = obj,
                            IconImage = docBitSource,
                            ExpandImage = docBitSource,
                            UnExpandImage = docBitSource
                        });

                    ExpandItemIfHasNewChildern(formItem, newVariableItem);
                    formItem.Children = newVariableItem;
                    newItems.Add(formItem);
                }

                localVarItems.Children.Clear();
                foreach (var obj in newItems)
                    localVarItems.Children.Add(obj);

                ExpandItemIfChildrenIsExpanded(localVarItems);
            }

            private void UpdateSharesTable(IList<DbObjectInfo> dbObjs, ObjectViewItem tables)
            {
                var newSharedTables = new ObservableCollection<ObjectViewItem>();
                foreach (var st in dbObjs.Where(p => p.shared))
                    newSharedTables.Add(new ObjectViewItem
                    {
                        Header = FormatDbObjectStr(st),
                        Tag = st,
                        IconImage = docBitSource,
                        ExpandImage = docBitSource,
                        UnExpandImage = docBitSource
                    });

                ExpandItemIfHasNewChildern(tables, newSharedTables);
                tables.Children = newSharedTables;
            }

            private string FormatDbObjectStr(DbObjectInfo obj)
            {
                ByteConverter.ConvToNearsetUnit(obj.bytes, ByteUnit.B, out long newNum, out ByteUnit newUnit);

                switch (obj.forms)
                {
                    case DDBString.TableForm:
                        return string.Format("{0} {1}x{2} [{3}{4}]", obj.name, obj.rows, obj.columns, newNum, newUnit);
                    case DDBString.DictionaryForm:
                    case DDBString.SetForm:
                        return string.Format("{0}<{1}> {2} keys [{3}{4}]", obj.name, obj.type.ToLower(), obj.rows, newNum, newUnit);
                    case DDBString.ScalarForm:
                    case DDBString.PairForm:
                        return string.Format("{0}<{1}>", obj.name, obj.type.ToLower());
                    case DDBString.VectorForm:
                        return string.Format("{0}<{1}> {2} rows [{3}{4}]", obj.name, obj.type.ToLower(), obj.rows, newNum, newUnit);
                    case DDBString.MatrixForm:
                        return string.Format("{0}<{1}> {2}x{3} [{4}{5}]", obj.name, obj.type.ToLower(), obj.rows, obj.columns, newNum, newUnit);
                }

                if (obj.rows != 1 || obj.columns != 1)
                    return string.Format("{0}[{1}x{2}]", obj.name, obj.rows, obj.columns);
                return obj.name;
            }
        }

    }
}
