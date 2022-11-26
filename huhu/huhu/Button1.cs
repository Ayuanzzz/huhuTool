using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

namespace huhu
{
    public class Button1 : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public Button1()
        {
        }

        protected override void OnClick()
        {
            iterateAttribute();
            MessageBox.Show("赋空完成");
        }
        
        public static void iterateAttribute()
        {
            IMxDocument mxDocument = ArcMap.Application.Document as IMxDocument;
            IActiveView activeView = mxDocument.ActiveView;
            IMap map = activeView.FocusMap;
            //定义变量
            IFeatureLayer pLayer;
            IFeatureClass pClass;
            string pFieldName;
            //遍历图层
            for (int i = 0; i < map.LayerCount; i++)
            {
                pLayer = map.get_Layer(i) as IFeatureLayer;
                //遍历字段
                for (int j = 0; j < pLayer.FeatureClass.Fields.FieldCount; j++)
                {
                    pFieldName = pLayer.FeatureClass.Fields.get_Field(j).Name;
                    pClass = pLayer.FeatureClass;
                    int cIndex = pClass.FindField(pFieldName);
                    ITable pTable = pClass as ITable;
                    ICursor pCursor = pTable.Update(null, true);
                    IRow pRow;
                    pRow = pCursor.NextRow();
                    while (pRow != null)
                    {
                        object vo = pRow.get_Value(cIndex);
                        string type = vo.GetType().Name;
                        switch (type) 
                        {
                            case "String":
                                string Svalue = pRow.get_Value(cIndex).ToString();
                                if (Svalue == "")
                                {
                                    pRow.set_Value(cIndex, DBNull.Value);
                                    pCursor.UpdateRow(pRow);
                                }
                                pRow = pCursor.NextRow();
                                break;
                            default:
                                string value = pRow.get_Value(cIndex).ToString();
                                if (value == "0")
                                {
                                    pRow.set_Value(cIndex, DBNull.Value);
                                    pCursor.UpdateRow(pRow);
                                }
                                pRow = pCursor.NextRow();
                                break;
                        }
                    }
                }
            }
        }
    }
}
