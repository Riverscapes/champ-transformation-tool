//CHaMP Transformation Tool
//Copyright (C) 2012  Joseph Wheaton

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Collections.Generic;

namespace RSGIS.Utilites
{ 

    /// <summary>
    /// Class with static methods to do simple but common stuff with ArcObjects.
    /// </summary>
    class EsriUtilities
    {

        /// <summary>
        /// Checks if a geometry type matches another one. If mainType is esriGeometryAny then
        /// checkType will match if is is any of the other supported types: esriGeometryPoint,
        /// esriGeometryPolyline, esriGeometryPolygon.
        /// </summary>
        /// <param name="mainType">the type to check against</param>
        /// <param name="checkType">the type to check</param>
        /// <returns>true if they match, false otherwise</returns>
        public static bool CheckGeometryType(esriGeometryType mainType, esriGeometryType checkType)
        {
            if (mainType == esriGeometryType.esriGeometryAny &&
                (checkType == esriGeometryType.esriGeometryPoint ||
                checkType == esriGeometryType.esriGeometryPolyline ||
                checkType == esriGeometryType.esriGeometryPolygon))
                return true;
            else if (mainType == checkType)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Deletes the graphics layer with the given name.
        /// </summary>
        /// <param name="map">map to find the layer in</param>
        /// <param name="name">name of the layer to delete</param>
        public static void DeleteGraphicsLayer(IMap map, string name)
        {
            ICompositeLayer mainGraphicsLayer = (ICompositeLayer)map.BasicGraphicsLayer;
            for (int i = 0; i < mainGraphicsLayer.Count; i++)
            {
                if (mainGraphicsLayer.Layer[i].Name == name)
                    ((ICompositeGraphicsLayer)mainGraphicsLayer).DeleteLayer(name);
            }
        }

        /// <summary>
        /// Gets the geographic coordinate system of a spatial reference.
        /// </summary>
        /// <param name="sr">spatial reference</param>
        /// <returns>datum</returns>
        public static IGeographicCoordinateSystem GetDatum(ISpatialReference sr)
        {
            if (sr == null)
                return null;
            if (sr is IProjectedCoordinateSystem)
                return ((IProjectedCoordinateSystem)sr).GeographicCoordinateSystem;
            else
                return (IGeographicCoordinateSystem)sr;
        }

        /// <summary>
        /// Gets top-level feature class names in a workspace, ignoring everything in a feature
        /// dataset. Right now it only works with the following esriGeometryType values:
        /// esriGeometryPoint, esriGeometryPolyline, esriGeometryPolygon,
        /// esriGeometryAny (point, line, polygon).
        /// </summary>
        /// <param name="geomType">geometry type to get</param>
        /// <param name="ws">workspace</param>
        /// <returns>list of IDatasetName objects for the feature classes</returns>
        public static List<IDatasetName> GetFeatureClassNames(esriGeometryType geomType, IWorkspace ws)
        {
            List<IDatasetName> outDsNames = new List<IDatasetName>();
            IEnumDatasetName dsNames = ws.get_DatasetNames(esriDatasetType.esriDTFeatureClass);
            IDatasetName dsName = dsNames.Next();
            while (dsName != null)
            {
                if (CheckGeometryType(geomType, ((IFeatureClassName)dsName).ShapeType))
                    outDsNames.Add(dsName);
                dsName = dsNames.Next();
            }
            return outDsNames;
        }

        /// <summary>
        /// Gets feature class names in the given feature dataset. Right now it only works with the 
        /// following esriGeometryType values: esriGeometryPoint, esriGeometryPolyline,
        /// esriGeometryPolygon, esriGeometryAny (point, line, polygon).
        /// </summary>
        /// <param name="geomType">geometry type to get</param>
        /// <param name="fdsName">IFeatureDatasetName corresponding to the feature dataset to get
        /// feature classes in</param>
        /// <returns>list of IDatasetName objects for the feature classes</returns>
        public static List<IDatasetName> GetFeatureClassNames(esriGeometryType geomType,
            IFeatureDatasetName fdsName)
        {
            List<IDatasetName> outDsNames = new List<IDatasetName>();
            IEnumDatasetName dsNamesFc = fdsName.FeatureClassNames;
            IDatasetName dsNameFc = dsNamesFc.Next();
            while (dsNameFc != null)
            {
                if (CheckGeometryType(geomType, ((IFeatureClassName)dsNameFc).ShapeType))
                    outDsNames.Add(dsNameFc);
                dsNameFc = dsNamesFc.Next();
            }
            return outDsNames;
        }

        /// <summary>
        /// Gets feature class names in the feature dataset with the given name. Pass the empty string
        /// to get all feature classes in all feature datasets. Right now it only works with the
        /// following esriGeometryType values: esriGeometryPoint, esriGeometryPolyline,
        /// esriGeometryPolygon, esriGeometryAny (point, line, polygon).
        /// </summary>
        /// <param name="geomType">geometry type to get</param>
        /// <param name="ws">workspace</param>
        /// <param name="datasetName">name of feature dataset to get feature classes in</param>
        /// <returns>list of IDatasetName objects for the feature classes</returns>
        public static List<IDatasetName> GetFeatureClassNames(esriGeometryType geomType, IWorkspace ws,
            string datasetName)
        {
            List<IDatasetName> outDsNames = new List<IDatasetName>();
            if (datasetName != "")
            {
                IEnumDatasetName dsNames = ws.get_DatasetNames(esriDatasetType.esriDTFeatureDataset);
                IDatasetName dsName = dsNames.Next();
                while (dsName != null)
                {
                    if (dsName.Name == datasetName)
                    {
                        outDsNames.AddRange(GetFeatureClassNames(geomType, (IFeatureDatasetName)dsName));
                        break;
                    }
                }
            }
            else
            {
                outDsNames.AddRange(GetFeatureClassNames(geomType, ws));
                IEnumDatasetName dsNamesFds = ws.get_DatasetNames(esriDatasetType.esriDTFeatureDataset);
                IDatasetName dsNameFds = dsNamesFds.Next();
                while (dsNameFds != null)
                {
                    outDsNames.AddRange(GetFeatureClassNames(geomType, (IFeatureDatasetName)dsNameFds));
                    dsNameFds = dsNamesFds.Next();
                }
            }
            return outDsNames;
        }

        /// <summary>
        /// Gets the graphics layer with the given name.
        /// </summary>
        /// <param name="map">map to find the layer in</param>
        /// <param name="name">name of the layer to find</param>
        /// <returns>graphics layer</returns>
        public static IGraphicsLayer GetGraphicsLayer(IMap map, string name)
        {
            ICompositeLayer mainGraphicsLayer = (ICompositeLayer)map.BasicGraphicsLayer;
            for (int i = 0; i < mainGraphicsLayer.Count; i++)
            {
                if (mainGraphicsLayer.Layer[i].Name == name)
                    return (IGraphicsLayer)mainGraphicsLayer.Layer[i];
            }
            return null;
        }

        /// <summary>
        /// Gets a list of all geotransforms that use the provided spatial reference and unprojected
        /// WGS84
        /// </summary>
        /// <param name="sr">spatial reference to look for</param>
        /// <returns>List of applicable IGeoTransformation objects</returns>
        public static List<IGeoTransformation> GetTransformations(ISpatialReference sr)
        {
            GeoTransformationComparer transComparer = new GeoTransformationComparer();
            IClone wgs84 = (IClone)EsriUtilities.GetWGS84();
            IClone targetDatum = (IClone)EsriUtilities.GetDatum(sr);
            List<IGeoTransformation> transList = new List<IGeoTransformation>();
            ISpatialReferenceFactory2 srFactory = new SpatialReferenceEnvironmentClass();
            ISet transSet = srFactory.GetPredefinedGeographicTransformations();
            IGeoTransformation trans;
            ISpatialReference fromDatum;
            ISpatialReference toDatum;
            for (int i = 0; i < transSet.Count; i++)
            {
                trans = (IGeoTransformation)transSet.Next();
                trans.GetSpatialReferences(out fromDatum, out toDatum);
                if (wgs84.IsEqual((IClone)fromDatum) || wgs84.IsEqual((IClone)toDatum))
                {
                    if (targetDatum.IsEqual((IClone)fromDatum) || targetDatum.IsEqual((IClone)toDatum))
                    {
                        transList.Add(trans);
                    }
                }
            }
            transList.Sort(transComparer.Compare);
            return transList;
        }

        /// <summary>
        /// Gets the unprojected WGS84 geographic coordinate system
        /// </summary>
        /// <returns>WGS84 GCS</returns>
        public static IGeographicCoordinateSystem GetWGS84()
        {
            ISpatialReferenceFactory3 srFactory = new SpatialReferenceEnvironmentClass();
            return srFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
        }

        /// <summary>
        /// Checks if a spatial reference is unprojected WGS84.
        /// </summary>
        /// <param name="sr">spatial reference to check</param>
        /// <returns>true if the spatial reference is WGS84, false if not</returns>
        public static bool IsWGS84(ISpatialReference sr) 
        {
            IClone wgs84 = (IClone)EsriUtilities.GetWGS84();
            IClone targetDatum = (IClone)EsriUtilities.GetDatum(sr);
            return (targetDatum).IsEqual(wgs84);
        }

        /// <summary>
        /// Creates a new double field with the given name and precision. Does not check that either
        /// are valid values.
        /// </summary>
        /// <param name="name">name of field</param>
        /// <param name="precision">precision of field</param>
        /// <returns>new field</returns>
        public static IField MakeFieldDouble(string name, int precision)
        {
            IField field = new FieldClass();
            IFieldEdit fieldEdit = (IFieldEdit)field;
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            fieldEdit.Name_2 = name;
            //fieldEdit.Precision_2 = precision;
            return field;
        }

        /// <summary>
        /// Makes a unique field name in a table by adding numbers to the end of a name. Does not
        /// check to make sure it's valid.
        /// </summary>
        /// <param name="table">table the field will go in</param>
        /// <param name="baseName">basename</param>
        /// <returns>field name</returns>
        public static string MakeFieldName(ITable table, string baseName)
        {
            string name = baseName;
            int i = 1;
            while (table.FindField(name) > -1)
                name = baseName + (i++).ToString();
            return name;
        }

        /// <summary>
        /// Creates a new small integer field with the given name. Does not check that the name is
        /// valid.
        /// </summary>
        /// <param name="name">name of field</param>
        /// <returns>new field</returns>
        public static IField MakeFieldSmallInt(string name)
        {
            IField field = new FieldClass();
            IFieldEdit fieldEdit = (IFieldEdit)field;
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
            fieldEdit.Name_2 = name;
            return field;
        }

        /// <summary>
        /// Creates a new string field with the given name and length. Does not check that either are
        /// valid values.
        /// </summary>
        /// <param name="name">name of field</param>
        /// <param name="length">length of field</param>
        /// <returns>new field</returns>
        public static IField MakeFieldString(string name, int length)
        {
            IField field = new FieldClass();
            IFieldEdit fieldEdit = (IFieldEdit)field;
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldEdit.Name_2 = name;
            fieldEdit.Length_2 = length;
            return field;
        }

        /// <summary>
        /// Creates a new basic graphics layer in the map.
        /// </summary>
        /// <param name="map">map to create the layer in</param>
        /// <param name="name">name of the layer to create</param>
        /// <returns>new graphics layer</returns>
        public static IGraphicsLayer MakeGraphicsLayer(IMap map, string name)
        {
            return ((ICompositeGraphicsLayer)map.BasicGraphicsLayer).AddLayer(name, null);
        }

        /// <summary>
        /// Creates a unique name for an Esri object like a feature class or table by adding numbers
        /// on the end of the name.
        /// </summary>
        /// <param name="ws">workspace the object will go in</param>
        /// <param name="dataType">type of object</param>
        /// <param name="baseName">basename</param>
        /// <returns>new name</returns>
        public static string MakeUniqueName(IWorkspace2 ws, esriDatasetType dataType, string baseName)
        {
            string name = baseName;
            int n = 1;
            while (ws.get_NameExists(dataType, name))
                name = baseName + (n++).ToString();
            return name;
        }
    }

    /// <summary>
    /// Class to compare two DatasetName objects based on the filename of the dataset.
    /// </summary>
    class DatasetNameComparer : IComparer<IDatasetName>
    {
        /// <summary>
        /// Compares two DatasetName objects based on workspace name and dataset name.
        /// </summary>
        /// <param name="name1"></param>
        /// <param name="name2"></param>
        /// <returns>0 if equal, -1 if name1 sorts before name2, 1 if name2 sorts before
        /// name1 </returns>
        public int Compare(IDatasetName name1, IDatasetName name2)
        {
            if (name1 == null)
            {
                if (name2 == null)
                    return 0;
                else
                    return -1;
            }
            else if (name2 == null)
            {
                return 1;
            }
            else
            {
                int wsEqual = name1.WorkspaceName.PathName.CompareTo(name2.WorkspaceName.PathName);
                int nameEqual = name1.Name.CompareTo(name2.Name);
                if (wsEqual == 0)
                    return nameEqual;
                else
                    return wsEqual;
            }
        }

        /// <summary>
        /// Checks to see if two DatasetName objects are equal based on workspace name and dataset
        /// name.
        /// </summary>
        /// <param name="name1"></param>
        /// <param name="name2"></param>
        /// <returns>true if equal, false if not</returns>
        public bool Equal(IDatasetName name1, IDatasetName name2)
        {
            return Compare(name1, name2) == 0;
        }
    }

    /// <summary>
    /// Class to compare two GeoTransformation objects based on the name of the transform.
    /// </summary>
    class GeoTransformationComparer : IComparer<IGeoTransformation>
    {
        /// <summary>
        /// Compares two GeoTransformation objects based on transform name.
        /// </summary>
        /// <param name="trans1"></param>
        /// <param name="trans2"></param>
        /// <returns>0 if equal, -1 if trans1 sorts before trans2, 1 if trans2 sorts before
        /// trans1 </returns>
        public int Compare(IGeoTransformation trans1, IGeoTransformation trans2)
        {
            if (trans1 == null)
            {
                if (trans2 == null)
                    return 0;
                else
                    return -1;
            }
            else if (trans2 == null)
            {
                return 1;
            }
            else
            {
                return trans1.Name.CompareTo(trans2.Name);
            }
        }

        /// <summary>
        /// Checks to see if two GeoTransformation objects are equal based on transform name.
        /// </summary>
        /// <param name="trans1"></param>
        /// <param name="trans2"></param>
        /// <returns>true if equal, false if not</returns>
        public bool Equal(IGeoTransformation trans1, IGeoTransformation trans2)
        {
            return Compare(trans1, trans2) == 0;
        }
    }
}

