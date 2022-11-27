using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;

namespace huhu
{
    public class union : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        //定义变量
        private IApplication pApplication;
        private static IMap pMap;
        IFeatureLayer pLayer;
        IFeatureClass pClass;
        IFeatureCursor pFeatCur;
        ICursor pCursor;
        public union()
        {
        }
        protected override void OnClick()
        {
            pApplication = ArcMap.Application;
            pMap = (pApplication.Document as IMxDocument).FocusMap;
            pLayer = pMap.get_Layer(0) as IFeatureLayer;
            selectByAttribute();
        }
        private void selectByAttribute()
        {
            //使用FeatureLayer对象的IFeatureSelection接口来执行查询操作。这里有一个接口转换操作。
            IFeatureSelection featureSelection = pLayer as IFeatureSelection;
            //新建IQueryFilter接口的对象来进行where语句的定义
            IQueryFilter queryFilter = new QueryFilterClass();
            //设置where语句内容
            string tmpStr;
            tmpStr = "\"" + "Name" + "\"" + " = " + "\'" + "哈尔滨路" + "\'";
            queryFilter.WhereClause = tmpStr;
            //首先使用IMap接口的ClearSelection()方法清空地图选择集
            pMap.ClearSelection();
            //根据定义的where语句使用IFeatureSelection接口的SelectFeatures方法选择要素
            featureSelection.SelectFeatures(queryFilter, esriSelectionResultEnum.esriSelectionResultNew, false);
            ISelectionSet pSelSet = featureSelection.SelectionSet;
            pSelSet.Search(null, false, out pCursor);
            pFeatCur = pCursor as IFeatureCursor;
        }
    }
}
