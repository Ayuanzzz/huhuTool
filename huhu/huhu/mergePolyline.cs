using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;


namespace huhu
{
    public class mergePolyline : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        IMap map = null;
        IActiveView pActiveView = null;
        //private List<IPolyline> DisconnPolylineList = new List<IPolyline>();
        public mergePolyline()
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            map = mxDoc.FocusMap;
            pActiveView = mxDoc.ActivatedView;
        }
        protected override void OnClick()
        {
            //
            //  TODO: Sample code showing how to access button host
            //
            //ArcMap.Application.CurrentTool = null;

            //计算程序耗时
            DateTime beforDT = System.DateTime.Now;

            List<string> distinctString = getDistinctNAMEValue();
            MergePloyline(distinctString);

            DateTime afterDT = System.DateTime.Now;
            TimeSpan ts = afterDT.Subtract(beforDT);
            MessageBox.Show("线要素合并结束，运行程序共耗时约：" + ts.Minutes + "分钟");
        }

        public List<string> getDistinctNAMEValue()
        {
            IFeatureLayer featureLayer = map.get_Layer(0) as IFeatureLayer;
            IFeatureClass featureClass = featureLayer.FeatureClass;
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = "";
            IFeatureCursor pFeatCursor = featureClass.Search(queryFilter, false);
            IFeature pFeature = pFeatCursor.NextFeature();
            ArrayList fieldArray = new ArrayList();
            List<string> distinctString = new List<string>();
            while (pFeature != null)
            {
                if (featureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                {
                    //获取要素字段
                    IFields fields = pFeatCursor.Fields;
                    int fieldIndex = fields.FindField("Name");
                    if (pFeature.get_Value(fieldIndex) != null)
                    {
                        string field_NAME = (string)pFeature.get_Value(fieldIndex);
                        fieldArray.Add(field_NAME);
                    }
                }
                pFeature = pFeatCursor.NextFeature();
            }
            distinctString = removeSameString(fieldArray);
            return distinctString;
        }

        public void MergePloyline(List<string> DistinctNameValue)
        {
            IFeatureLayer featureLayer = map.get_Layer(0) as IFeatureLayer;
            IFeatureClass featureClass = featureLayer.FeatureClass;

            //IDataset dataset = featureClass as IDataset;
            //IWorkspaceEdit workspaceEdit = dataset.Workspace as IWorkspaceEdit;
            //Type.Missing指的是空类型，因为有些方法你传null给它会出错的，必须要用Type.Missing.
            object Missing = Type.Missing;
            //workspaceEdit.StartEditing(true);
            //workspaceEdit.StartEditOperation();
            //string field_NAME = "";

            for (int i = 0; i < DistinctNameValue.Count; i++)
            {
                IQueryFilter queryFilter = new QueryFilterClass();
                queryFilter.WhereClause = "";
                IFeatureCursor pFeatCursor = featureClass.Search(queryFilter, false);
                IFeature pFeature = pFeatCursor.NextFeature();

                IFeature pFeatureFirst = pFeature;
                //List<IPolyline> toMergePolylineList = new List<IPolyline>();

                IGeometryCollection Geometrybag = new GeometryBagClass();
                ITopologicalOperator2 pTopOperatorFirst = null;
                IGeometry geometrySecond = null;
                IGeometry pGeometryFirst = null;
                bool bSwitch = true;
                while (pFeature != null)
                {
                    map.SelectFeature(featureLayer, pFeature);
                    if (featureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                    {
                        //IPolyline polyline = geometry as IPolyline;
                        IFields fields = pFeatCursor.Fields;
                        int fieldIndex = fields.FindField("NAME");
                        string field_NAME = (string)pFeature.get_Value(fieldIndex);

                        if (field_NAME == DistinctNameValue[i])
                        {
                            if (bSwitch)
                            {
                                //将当前name字段相同的feature中的第一个feature传给pFeatureFirst
                                pFeatureFirst = pFeature;
                                pGeometryFirst = pFeature.Shape;
                                pTopOperatorFirst = (ITopologicalOperator2)pGeometryFirst;
                                pTopOperatorFirst.IsKnownSimple_2 = false;
                                pTopOperatorFirst.Simplify();
                                pGeometryFirst.SnapToSpatialReference();
                                bSwitch = false;
                                //break;
                            }
                            else
                            {
                                //geometrySecond = pFeature.ShapeCopy;
                                geometrySecond = pFeature.Shape;
                                Geometrybag.AddGeometry(geometrySecond, ref Missing, ref Missing);
                                //toMergePolylineList.Add(polyline);
                            }
                        }
                        //DisconnPolylineList.Add(polyline);
                    }
                    pFeature = pFeatCursor.NextFeature();
                }
                IEnumGeometry tEnumGeometry = (IEnumGeometry)Geometrybag;
                //IGeometry mergeGeomery = null;
                pTopOperatorFirst.ConstructUnion(tEnumGeometry);

                pTopOperatorFirst.IsKnownSimple_2 = false;
                pTopOperatorFirst.Simplify();
                pFeatureFirst.Shape = pGeometryFirst;
                //pFeatureFirst.Store();
                IFeatureLayer featureLayer2 = map.get_Layer(1) as IFeatureLayer;
                IFeatureClass featureClass2 = featureLayer2.FeatureClass;
                AddPolyline(featureClass2, pGeometryFirst);
            }
            //workspaceEdit.StopEditOperation();
            //workspaceEdit.StopEditing(true);
        }
        private void AddPolyline(IFeatureClass pFeatureClass, IGeometry polyline)
        {
            IFeatureBuffer featureBuffer = pFeatureClass.CreateFeatureBuffer();
            IFeatureCursor featureCursor;
            featureCursor = pFeatureClass.Insert(true);
            featureBuffer.Shape = polyline;
            featureCursor.InsertFeature(featureBuffer);
            featureCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
        }
        public List<string> removeSameString(ArrayList stringArray)
        {
            //List用于存储从数组里取出来的不相同的元素
            List<string> distinctString = new List<string>();
            foreach (string eachString in stringArray)
            {
                if (!distinctString.Contains(eachString))
                    distinctString.Add(eachString);
            }
            return distinctString;
        }

        //protected override void OnUpdate()
        //{
        //    Enabled = ArcMap.Application != null;
        //}
    }
}
