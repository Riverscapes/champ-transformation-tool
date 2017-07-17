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

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RSGIS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CHAMP
{
    class ChampTransformErrorsClass
    {
        private string _fcName; // path to output feature class
        private bool _created; // whether it was created successfully
        private int _numRowErrors; // number of rows that couldn't be copied
        private List<string> _messages; // error messages

        public bool Created
        {
            get { return _created; }
        }

        public int NumRowErrors
        {
            get { return _numRowErrors; }
        }

        public ChampTransformErrorsClass(string name)
        {
            _fcName = name;
            _created = true;
            _messages = new List<string>();
        }

        public void AddCreationError(string msg)
        {
            _created = false;
            _messages.Add(msg);
        }

        public void AddRowError(string msg)
        {
            _numRowErrors++;
            _messages.Add(msg);
        }

        public string GetErrorMessage(int index)
        {
            if (index > -1 && index < _messages.Count)
                return _messages[index];
            else
                return "";
        }
    }

    class ChampTransformClass
    {
        private List<esriGeometryType> _validGeomTypes; // point, line, polygon

        private IMap _map; // map document
        private ISpatialReference _spatialRef; // spatial reference for output
        private IGeoTransformation _geotransform; // datum transformation

        private IFeatureWorkspace _dataFWS; // data workspace
        private IWorkspace2 _dataWS2; // data workspace
        private esriWorkspaceType _dataWSType; // type of workspace (file or local geodatabase)

        private IPoint[] _gpsBenchmarks; // GPS benchmarks, indices 1-3
        private IPoint[] _tsBenchmarks; // total station benchmarks, indices 1-3

        private Dictionary<string, double> _compassBearings; // compass bearings, "<from_index><to_index>"
        private Dictionary<string, double> _gpsBearings; // bearings between GPS benchmarks, "<from_index><to_index>"
        private Dictionary<string, double> _tsBearings; // bearings between total station benchmarks, "<from_index><to_index>"
        private bool _gpsBearingsDirty; // whether bearings need to be calculated or recalculated
        private bool _tsBearingsDirty; // whether bearings need to be calculated or recalculated

        private Dictionary<string, double> _errors; // errors, "<H,Z><GPS,Compass><hinge_index><bearing_index><error_bm_index>"

        private IFeatureClass _controlPts; // control point feature class
        private string _attField; // control point field name for benchmark IDs
        private string[] _attIds; // control point attribute values for benchmarks

        private List<IFeatureClassName> _inDataFcNames; // name objects for input feature classes
        private List<IFeatureClass> _outDataFc; // output feature classes
        private string _controlPtsOutputFn; // path to output control points feature class
        private Dictionary<string, string> _dataFns; // paths to output feature classes, keys are input paths

        private string _selectedTransform; // selected transformation,"<GPS,Compass><hinge_index><rotate_bm_index>"
        private List<string> _viewedList; // transformations viewed by user, "<GPS,Compass><hinge_index><rotate_bm_index>"
        private List<string> _graphicsLayers; // names of graphics layers added to the map

        private Dictionary<string, ChampTransformErrorsClass> _outFcErrors; // output errors, keys are output paths
        string _fullErrorMsg;

        /// <summary>
        /// Constructor. Anything not initizialized here automatically gets initialized with null.
        /// </summary>
        public ChampTransformClass()
        {
            _validGeomTypes = new List<esriGeometryType>();
            _validGeomTypes.Add(esriGeometryType.esriGeometryPoint);
            _validGeomTypes.Add(esriGeometryType.esriGeometryPolyline);
            _validGeomTypes.Add(esriGeometryType.esriGeometryPolygon);

            _gpsBenchmarks = new IPoint[4];
            _tsBenchmarks = new IPoint[4];

            _compassBearings = new Dictionary<string, double>();
            _gpsBearings = new Dictionary<string, double>();
            _tsBearings = new Dictionary<string, double>();
            _gpsBearingsDirty = true;
            _tsBearingsDirty = true;

            _errors = new Dictionary<string, double>();

            _attIds = new string[4];

            _inDataFcNames = new List<IFeatureClassName>();
            _outDataFc = new List<IFeatureClass>();
            _dataFns = new Dictionary<string, string>();

            _viewedList = new List<string>();
            _graphicsLayers = new List<string>();

            _outFcErrors = new Dictionary<string, ChampTransformErrorsClass>();
        }

        #region Private methods

        #region Bearing and error calculations

        /// <summary>
        /// Calculate bearings between GPS benchmarks.
        /// </summary>
        private void CalculateGPSBearings()
        {
            ILine2 line = new LineClass();
            if (_gpsBenchmarks[1] != null && _gpsBenchmarks[2] != null)
            {
                line.PutCoords(_gpsBenchmarks[1], _gpsBenchmarks[2]);
                _gpsBearings["12"] = TrigUtilities.TrigRadiansToGeoDegrees(line.Angle);
                _gpsBearings["21"] = TrigUtilities.ReverseBearing(_gpsBearings["12"]);
            }
            if (_gpsBenchmarks[1] != null && _gpsBenchmarks[3] != null)
            {
                line.PutCoords(_gpsBenchmarks[1], _gpsBenchmarks[3]);
                _gpsBearings["13"] = TrigUtilities.TrigRadiansToGeoDegrees(line.Angle);
                _gpsBearings["31"] = TrigUtilities.ReverseBearing(_gpsBearings["13"]);
            }
            if (_gpsBenchmarks[2] != null & _gpsBenchmarks[3] != null)
            {
                line.PutCoords(_gpsBenchmarks[2], _gpsBenchmarks[3]);
                _gpsBearings["23"] = TrigUtilities.TrigRadiansToGeoDegrees(line.Angle);
                _gpsBearings["32"] = TrigUtilities.ReverseBearing(_gpsBearings["23"]);
            }
            _gpsBearingsDirty = false;
        }

        /// <summary>
        /// Calculate bearings between total station benchmarks.
        /// </summary>
        private void CalculateTSBearings()
        {
            ILine2 line = new LineClass();
            if (_tsBenchmarks[1] != null && _tsBenchmarks[2] != null)
            {
                line.PutCoords(_tsBenchmarks[1], _tsBenchmarks[2]);
                _tsBearings["12"] = TrigUtilities.TrigRadiansToGeoDegrees(line.Angle);
                _tsBearings["21"] = TrigUtilities.ReverseBearing(_tsBearings["12"]);
            }
            if (_tsBenchmarks[1] != null && _tsBenchmarks[3] != null)
            {
                line.PutCoords(_tsBenchmarks[1], _tsBenchmarks[3]);
                _tsBearings["13"] = TrigUtilities.TrigRadiansToGeoDegrees(line.Angle);
                _tsBearings["31"] = TrigUtilities.ReverseBearing(_tsBearings["13"]);
            }
            if (_tsBenchmarks[2] != null & _tsBenchmarks[3] != null)
            {
                line.PutCoords(_tsBenchmarks[2], _tsBenchmarks[3]);
                _tsBearings["23"] = TrigUtilities.TrigRadiansToGeoDegrees(line.Angle);
                _tsBearings["32"] = TrigUtilities.ReverseBearing(_tsBearings["23"]);
            }
            _tsBearingsDirty = false;
        }

        /// <summary>
        /// Compute the errors for each benchmark given each possible combination of hinge and bearing.
        /// </summary>
        private void ComputeErrors()
        {
            // Make sure bearings are up to date
            if (_gpsBearingsDirty)
                CalculateGPSBearings();
            if (_tsBearingsDirty)
                CalculateTSBearings();

            IPoint gpsBM, tsBM, realPoint, modelPoint;
            double tsBearing, gpsBearing, compassBearing;
            string bearingId, errorId;

            // Loop through each index and use that GPS benchmark as a hinge
            for (int hingeIndex = 1; hingeIndex <= 3; hingeIndex++)
            {
                gpsBM = _gpsBenchmarks[hingeIndex];
                tsBM = _tsBenchmarks[hingeIndex];

                // Go to the next hinge if we don't have both GPS and total station coordinates for this index
                if (_gpsBenchmarks == null || tsBM == null)
                    continue;

                // Loop through each index and use it as the rotated to benchmark
                for (int bearingIndex = 1; bearingIndex <= 3; bearingIndex++)
                {
                    // Skip if it's the same as the hinge
                    if (bearingIndex == hingeIndex)
                        continue;

                    // Skip if we don't have a total station bearing between the hinge and this benchmark
                    bearingId = hingeIndex.ToString() + bearingIndex.ToString();
                    if (!_tsBearings.ContainsKey(bearingId))
                        continue;
                    tsBearing = _tsBearings[bearingId];

                    // Compute the GPS errors if we have a GPS bearing between the hinge and this benchmark
                    if (_gpsBearings.ContainsKey(bearingId))
                    {
                        gpsBearing = tsBearing - _gpsBearings[bearingId];

                        // Loop through each index and compute the error for it
                        for (int benchmarkIndex = 1; benchmarkIndex <= 3; benchmarkIndex++)
                        {
                            // Skip if it's the same as the hinge
                            if (benchmarkIndex == hingeIndex)
                                continue;

                            errorId = hingeIndex.ToString() + bearingIndex.ToString() + benchmarkIndex.ToString();
                            realPoint = _gpsBenchmarks[benchmarkIndex];
                            modelPoint = TransformPoint(_tsBenchmarks[benchmarkIndex], tsBM, gpsBM, gpsBearing);
                            _errors["HGPS" + errorId] = modelPoint.X - realPoint.X;
                            _errors["ZGPS" + errorId] = modelPoint.Y - realPoint.Y;
                        }
                    }

                    // Compute the compass errors if we have a compass bearing between the hinge and this benchmark
                    if (_compassBearings.ContainsKey(bearingId))
                    {
                        compassBearing = tsBearing - _compassBearings[bearingId];

                        // Loop through each index and compute the error for it
                        for (int benchmarkIndex = 1; benchmarkIndex <= 3; benchmarkIndex++)
                        {
                            // Skip if it's the same as the hinge
                            if (benchmarkIndex == hingeIndex)
                                continue;

                            errorId = hingeIndex.ToString() + bearingIndex.ToString() + benchmarkIndex.ToString();
                            realPoint = _gpsBenchmarks[benchmarkIndex];
                            modelPoint = TransformPoint(_tsBenchmarks[benchmarkIndex], tsBM, gpsBM, compassBearing);
                            _errors["HCompass" + errorId] = modelPoint.X - realPoint.X;
                            _errors["ZCompass" + errorId] = modelPoint.Y - realPoint.Y;
                        }
                    }
                }
            }
        }

        #endregion Bearing and error calculations

        #region Create output

        /// <summary>
        /// Creates a feature class with the benchmark points in it.
        /// </summary>
        private void CreateBenchmarkFeatureClass()
        {
            if (_spatialRef == null)
                throw new Exception("Spatial reference not set.");

            // Create the fields
            IFeatureClassDescription fcDesc = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDesc = (IObjectClassDescription)fcDesc;
            IFields fields = ocDesc.RequiredFields;
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;
            IField shape = fields.get_Field(fields.FindField(fcDesc.ShapeFieldName));
            IGeometryDefEdit shapeEdit = (IGeometryDefEdit)shape.GeometryDef;
            shapeEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            shapeEdit.HasZ_2 = true;
            shapeEdit.SpatialReference_2 = _spatialRef;
            IField projEast = EsriUtilities.MakeFieldDouble("Proj_Easting", 3);
            fieldsEdit.AddField(projEast);
            IField projNorth = EsriUtilities.MakeFieldDouble("Proj_Northing", 3);
            fieldsEdit.AddField(projNorth);
            IField unprojEast = EsriUtilities.MakeFieldDouble("Unproj_Easting", 3);
            fieldsEdit.AddField(unprojEast);
            IField unprojNorth = EsriUtilities.MakeFieldDouble("Unproj_Northing", 3);
            fieldsEdit.AddField(unprojNorth);
            IField lon = EsriUtilities.MakeFieldDouble("Longitude", 6);
            fieldsEdit.AddField(lon);
            IField lat = EsriUtilities.MakeFieldDouble("Latitude", 6);
            fieldsEdit.AddField(lat);

            // Create the feature class
            string backupName = "";
            IFeatureClass fc = null;
            if (_dataWSType == esriWorkspaceType.esriLocalDatabaseWorkspace)
            {
                IFeatureDataset featureDS;
                if (_dataWS2.get_NameExists(esriDatasetType.esriDTFeatureDataset, "Projected"))
                    featureDS = _dataFWS.OpenFeatureDataset("Projected");
                else
                    featureDS = _dataFWS.CreateFeatureDataset("Projected", _spatialRef);
                backupName = EsriUtilities.MakeUniqueName(_dataWS2, esriDatasetType.esriDTFeatureClass,
                    "Benchmarks");
                ManageVersions("Benchmarks", backupName);
                fc = featureDS.CreateFeatureClass("Benchmarks", fields, ocDesc.InstanceCLSID, null,
                    esriFeatureType.esriFTSimple, fcDesc.ShapeFieldName, "");
            }
            else
            {
                backupName = EsriUtilities.MakeUniqueName(_dataWS2, esriDatasetType.esriDTFeatureClass,
                    "Benchmarks");
                ManageVersions("Benchmarks", backupName);
                fc = _dataFWS.CreateFeatureClass("Benchmarks.shp", fields, ocDesc.InstanceCLSID, null,
                    esriFeatureType.esriFTSimple, fcDesc.ShapeFieldName, "");
            }

            // Fill the feature class
            IFeatureCursor rows = fc.Insert(true);
            IFeatureBuffer buffer = fc.CreateFeatureBuffer();
            IPoint gpsBM, tsBM, geoBM;
            for (int i = 1; i <= 3; i++)
            {
                gpsBM = _gpsBenchmarks[i];
                tsBM = _tsBenchmarks[i];
                geoBM = (IPoint)((IClone)gpsBM).Clone();
                if (_geotransform == null)
                    geoBM.Project(EsriUtilities.GetWGS84());
                else
                {
                    esriTransformDirection dir;
                    ISpatialReference fromSr;
                    ISpatialReference toSr;
                    _geotransform.GetSpatialReferences(out fromSr, out toSr);
                    if (EsriUtilities.IsWGS84(toSr))
                        dir = esriTransformDirection.esriTransformForward;
                    else
                        dir = esriTransformDirection.esriTransformReverse;
                    ((IGeometry2)geoBM).ProjectEx(_spatialRef, dir, _geotransform, false, 0, 0);
                }
                buffer.Shape = gpsBM;
                buffer.Value[2] = gpsBM.X;
                buffer.Value[3] = gpsBM.Y;
                buffer.Value[4] = tsBM.X;
                buffer.Value[5] = tsBM.Y;
                buffer.Value[6] = geoBM.X;
                buffer.Value[7] = geoBM.Y;
                rows.InsertFeature(buffer);
            }

            // Add the feature layer to the map
            if (_map != null)
            {
                IFeatureLayer fl = new FeatureLayerClass();
                fl.FeatureClass = fc;
                fl.Name = "Benchmarks";
                _map.AddLayer(fl);
            }
        }

        /// <summary>
        /// Create a new feature class based on an existing oen.
        /// </summary>
        /// <param name="inFc">feature class to copy</param>
        /// <param name="outName">name of new feature class</param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private IFeatureClass CreateFeatureClass(IFeatureClass inFc, string outName,
            ChampTransformErrorsClass errors)
        {
            try
            {
                // Create the feature class
                IFeatureClass outFc = null;
                IFields fields = (IFields)((IClone)inFc.Fields).Clone();
                if (_dataWSType == esriWorkspaceType.esriLocalDatabaseWorkspace)
                {
                    IFeatureDataset featureDS;
                    if (_dataWS2.get_NameExists(esriDatasetType.esriDTFeatureDataset, "Projected"))
                        featureDS = _dataFWS.OpenFeatureDataset("Projected");
                    else
                        featureDS = _dataFWS.CreateFeatureDataset("Projected", _spatialRef);
                    outFc = featureDS.CreateFeatureClass(outName, fields, inFc.CLSID, inFc.EXTCLSID,
                        inFc.FeatureType, inFc.ShapeFieldName, "");
                }
                else
                {
                    outFc = _dataFWS.CreateFeatureClass(outName, fields, inFc.CLSID, inFc.EXTCLSID,
                        inFc.FeatureType, inFc.ShapeFieldName, ".shp");
                }
                return outFc;
            }
            catch (Exception ex)
            {
                string msg = string.Format("Could not create {0}: {1}", outName, ex.Message);
                _fullErrorMsg += msg + "\n";
                errors.AddCreationError(msg);
                return null;
            }
        }

        /// <summary>
        /// Transform features and put them in another feature class.
        /// </summary>
        /// <param name="inFc">input feature class</param>
        /// <param name="outFc">output feature class</param>
        /// <param name="hingePoint">hinge point in untransformed coordinates</param>
        /// <param name="gpsPoint">projected (GPS) coordinates of the hinge point</param>
        /// <param name="bearing">angle to rotate by</param>
        /// <param name="errors"></param>
        private void CopyAndTransformFeatureClass(IFeatureClass inFc, IFeatureClass outFc, 
            IPoint hingePoint, IPoint gpsPoint, double bearing, ChampTransformErrorsClass errors)
        {
            IFeatureCursor inRows = inFc.Search(null, true);
            IFeatureCursor outRows = outFc.Insert(false);
            IFeatureBuffer buffer = outFc.CreateFeatureBuffer();
            IFields inFields = inFc.Fields;
            IFields outFields = buffer.Fields;
            IFeature inRow;
            int shpIndex = outRows.FindField(outFc.ShapeFieldName);
            inRow = inRows.NextFeature();
            int row = 0;
            while (inRow != null)
            {
                row++;
                int i;
                try
                {
                    for (i = 0; i < inFields.FieldCount; i++)
                    {
                        int outIndex = outFields.FindField(inFields.Field[i].Name);
                        if (i == shpIndex)
                            buffer.Shape = TransformGeometry(inRow.Shape, hingePoint, gpsPoint, bearing);
                        else if (outFields.Field[outIndex].Editable)
                            buffer.set_Value(outIndex, inRow.get_Value(i));
                    }
                    outRows.InsertFeature(buffer);
                    inRow = inRows.NextFeature();
                }
                catch (Exception ex)
                {
                    string msg = string.Format("Could not copy row {0} from {1} to {2}: {3}", 
                        row, inFc.AliasName, outFc.AliasName, ex.Message);
                    _fullErrorMsg += msg + "\n";
                    errors.AddRowError(msg);
                    inRow = inRows.NextFeature();
                }
            }
        }

        /// <summary>
        /// Transforms a feature class given hinge, benchmark, and bearing, and writes it to a new
        /// feature class.
        /// </summary>
        /// <param name="inFc">feature class to transform</param>
        /// <param name="outName">name of the new feature class to create</param>
        /// <param name="hingePoint">hinge point in untransformed coordinates</param>
        /// <param name="gpsPoint">projected (GPS) coordinates of the hinge point</param>
        /// <param name="bearing">angle to rotate by</param>
        /// <returns>new feature class</returns>
        private IFeatureClass TransformFeatureClass(IFeatureClass inFc, string outName,
            IPoint hingePoint, IPoint gpsPoint, double bearing, ChampTransformErrorsClass errors)
        {
            IFeatureClass outFc = CreateFeatureClass(inFc, outName, errors);
            if (outFc != null)
                CopyAndTransformFeatureClass(inFc, outFc, hingePoint, gpsPoint, bearing, errors);
            return outFc;
        }

        /// <summary>
        /// Insert a row into the report table.
        /// </summary>
        /// <param name="inserter">insert cursor to use</param>
        /// <param name="buffer">row buffer to fill</param>
        /// <param name="hingeIndex">index of the hinge point</param>
        /// <param name="bmIndex">index of the benchmark rotated to</param>
        /// <param name="otherIndex">index of the benchmark not rotated to</param>
        /// <param name="bearingType">"GPS" or "Compass" (case-sensitive)</param>
        private void InsertReportRow(ref ICursor inserter, ref IRowBuffer buffer, int hingeIndex, 
            int bmIndex, int otherIndex, string bearingType)
        {
            string bmId = hingeIndex.ToString() + bmIndex.ToString() + bmIndex.ToString();
            string otherId = hingeIndex.ToString() + bmIndex.ToString() + otherIndex.ToString();
            buffer.Value[1] = "BM " + hingeIndex.ToString();
            buffer.Value[2] = bearingType + " BM " + bmIndex.ToString();
            buffer.Value[3] = _errors["H" + bearingType + bmId];
            buffer.Value[4] = _errors["Z" + bearingType + bmId];
            buffer.Value[5] = _errors["H" + bearingType + otherId];
            buffer.Value[6] = _errors["Z" + bearingType + otherId];
            buffer.Value[7] = _viewedList.Contains(bearingType.ToLower() + hingeIndex.ToString() + bmIndex.ToString()) ? 1 : 0;
            buffer.Value[8] = bearingType + hingeIndex.ToString() + bmIndex.ToString() == _selectedTransform ? 1 : 0;
            inserter.InsertRow(buffer);
        }

        /// <summary>
        /// Creates the report table.
        /// </summary>
        /// <param name="hingeIndex">index of the hinge point</param>
        /// <param name="fromBenchmarkIndex">index of the benchmark that the bearing is from</param>
        /// <param name="toBenchmarkIndex">index of the benchmark that the bearing is to</param>
        /// <param name="isBearingGPS">true if the bearing is a GPS one, false if it's a compass one</param>
        /// <param name="viewedTrans">list of viewed transforms</param>
        public void CreateReportTable(int hingeIndex, int fromBenchmarkIndex, int toBenchmarkIndex,
            bool isBearingGPS, List<string> viewedTrans)
        {
            if (_gpsBearingsDirty || _tsBearingsDirty)
                ComputeErrors();

            _viewedList = viewedTrans;
            _selectedTransform = (isBearingGPS ? "GPS" : "Compass") + hingeIndex.ToString() + toBenchmarkIndex.ToString();

            // Create the empty table
            IObjectClassDescription desc = new ObjectClassDescriptionClass();
            IFields fields = desc.RequiredFields;
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;
            IField hinge = EsriUtilities.MakeFieldString("Hinge", 6);
            fieldsEdit.AddField(hinge);
            IField bearing = EsriUtilities.MakeFieldString("Bearing", 14);
            fieldsEdit.AddField(bearing);
            IField bearingH = EsriUtilities.MakeFieldDouble("Bearing_dH", 3);
            fieldsEdit.AddField(bearingH);
            IField bearingZ = EsriUtilities.MakeFieldDouble("Bearing_dZ", 3);
            fieldsEdit.AddField(bearingZ);
            IField otherH = EsriUtilities.MakeFieldDouble("Other_dH", 3);
            fieldsEdit.AddField(otherH);
            IField otherZ = EsriUtilities.MakeFieldDouble("Other_dZ", 3);
            fieldsEdit.AddField(otherZ);
            IField viewed = EsriUtilities.MakeFieldSmallInt("Viewed");
            fieldsEdit.AddField(viewed);
            IField chosen = EsriUtilities.MakeFieldSmallInt("Chosen");
            fieldsEdit.AddField(chosen);
            string tableName = EsriUtilities.MakeUniqueName(_dataWS2, esriDatasetType.esriDTTable, "Transformations");
            ManageTableVersions("Transformations", tableName);
            ITable table = _dataFWS.CreateTable("Transformations", fields, desc.InstanceCLSID, null, "");

            // Fill the table
            IRowBuffer buffer = table.CreateRowBuffer();
            ICursor inserter = table.Insert(true);
            InsertReportRow(ref inserter, ref buffer, 1, 2, 3, "GPS");
            InsertReportRow(ref inserter, ref buffer, 1, 3, 2, "GPS");
            if (_errors.ContainsKey("HCompass123"))
                InsertReportRow(ref inserter, ref buffer, 1, 2, 3, "Compass");
            if (_errors.ContainsKey("HCompass132"))
                InsertReportRow(ref inserter, ref buffer, 1, 3, 2, "Compass");
            InsertReportRow(ref inserter, ref buffer, 2, 1, 3, "GPS");
            InsertReportRow(ref inserter, ref buffer, 2, 3, 1, "GPS");
            if (_errors.ContainsKey("HCompass213"))
                InsertReportRow(ref inserter, ref buffer, 2, 1, 3, "Compass");
            if (_errors.ContainsKey("HCompass231"))
                InsertReportRow(ref inserter, ref buffer, 2, 3, 1, "Compass");
            InsertReportRow(ref inserter, ref buffer, 3, 1, 2, "GPS");
            InsertReportRow(ref inserter, ref buffer, 3, 2, 1, "GPS");
            if (_errors.ContainsKey("HCompass312"))
                InsertReportRow(ref inserter, ref buffer, 3, 1, 2, "Compass");
            if (_errors.ContainsKey("HCompass321"))
                InsertReportRow(ref inserter, ref buffer, 3, 2, 1, "Compass");

            // Add the metadata to the table
            IMetadata metadata = (IMetadata)((IDataset)table).FullName;
            if (((IMetadataEdit)metadata).CanEditMetadata)
            {
                string info = "<DIV STYLE=\"text-align:Left;\"><DIV><DIV>"; // stupid esri wants extra crap in here
                info += string.Format("<P STYLE=\"margin:0 0 0 0;\"><SPAN>Control Points Benchmark ID Field: {0}</SPAN></P>",
                    _attField);
                info += string.Format("<P STYLE=\"margin:0 0 0 0;\"><SPAN>Benchmark 1 ID: {0}</SPAN></P>", _attIds[1]);
                info += string.Format("<P STYLE=\"margin:0 0 0 0;\"><SPAN>Benchmark 2 ID: {0}</SPAN></P>", _attIds[2]);
                info += string.Format("<P STYLE=\"margin:0 0 0 0;\"><SPAN>Benchmark 3 ID: {0}</SPAN></P>", _attIds[3]);
                info += "<P />";
                IDataset ds = (IDataset)_controlPts;
                ChampTransformErrorsClass errors = _outFcErrors[_controlPtsOutputFn];
                info += string.Format("<P STYLE=\"margin:0 0 0 0;\"><SPAN>Unprojected Control Points: {0}\\{1}</SPAN></P>",
                    ds.Workspace.PathName, ds.Name);
                if (errors.Created)
                {
                    info += string.Format("<P STYLE=\"margin:0 0 0 0;\"><SPAN>Projected Control Points: {0}</SPAN></P>",
                        _controlPtsOutputFn);
                    for (int i = 0; i < errors.NumRowErrors; i++)
                        info += FormatMsg(errors.GetErrorMessage(i));
                }
                else
                {
                    info += string.Format(errors.GetErrorMessage(0));
                }
                Dictionary<string, string>.KeyCollection keys = _dataFns.Keys;
                foreach (string key in keys.OrderBy(key => key))
                {
                    errors = _outFcErrors[_dataFns[key]];
                    info += "<P />";
                    info += string.Format("<P STYLE=\"margin:0 0 0 0;\"><SPAN>Unprojected: {0}</SPAN></P>", key);
                    if (errors.Created)
                    {
                        info += string.Format("<P STYLE=\"margin:0 0 0 0;\"><SPAN>Projected: {0}</SPAN></P>", _dataFns[key]);
                        for (int i = 0; i < errors.NumRowErrors; i++)
                            info += FormatMsg(errors.GetErrorMessage(i));
                    }
                    else
                    {
                        info += string.Format(errors.GetErrorMessage(0));
                    }
                }
                info += "</DIV></DIV></DIV>";
                IPropertySet properties = metadata.Metadata;
                properties.SetProperty("dataIdInfo/idCitation/resTitle", "CHAMP Report");
                properties.SetProperty("dataIdInfo/idAbs", info);
                metadata.Metadata = properties;
            }
        }

        private string FormatMsg(string msg) 
        {
            return string.Format("<P STYLE=\"margin:0 0 0 0;\"><SPAN>{0}</SPAN></P>", msg);
        }

        #endregion Create output

        #region Geometry transforms

        /// <summary>
        /// Does a rough transformation of a geometry around a hinge point.
        /// </summary>
        /// <param name="shape">geometry to transform</param>
        /// <param name="hingePoint">hinge point in untransformed coordinates</param>
        /// <param name="gpsPoint">projected (GPS) coordinates of the hinge point</param>
        /// <param name="bearing">angle to rotate by</param>
        /// <returns>new transformed geometry</returns>
        private IGeometry TransformGeometry(IGeometry shape, IPoint hingePoint, IPoint gpsPoint, 
            double bearing)
        {
            if (shape is IPoint)
                return TransformPoint((IPoint)shape, hingePoint, gpsPoint, bearing);
            else if (shape is IPolyline)
                return TransformPolyline((IPolyline)shape, hingePoint, gpsPoint, bearing);
            else if (shape is IPolygon)
                return TransformPolygon((IPolygon)shape, hingePoint, gpsPoint, bearing);
            else
                return null;
        }

        /// <summary>
        /// Does a rough transformation of a point around a hinge point.
        /// </summary>
        /// <param name="oldPoint">point to transform</param>
        /// <param name="hingePoint">hinge point in untransformed coordinates</param>
        /// <param name="gpsPoint">projected (GPS) coordinates of the hinge point</param>
        /// <param name="bearing">angle to rotate by</param>
        /// <returns>new transformed point</returns>
        private IPoint TransformPoint(IPoint oldPoint, IPoint hingePoint, IPoint gpsPoint, 
            double bearing)
        {
            double radians = TrigUtilities.DegreesToRadians(bearing);
            IPoint newPoint = (IPoint)((IClone)oldPoint).Clone();
            newPoint.X = ((oldPoint.X - hingePoint.X) * Math.Cos(radians) - (oldPoint.Y - hingePoint.Y) *
                Math.Sin(radians)) + gpsPoint.X;
            newPoint.Y = ((oldPoint.Y - hingePoint.Y) * Math.Cos(radians) + (oldPoint.X - hingePoint.X) *
                Math.Sin(radians)) + gpsPoint.Y;
            newPoint.Z = oldPoint.Z + gpsPoint.Z - hingePoint.Z;
            return newPoint;
        }

        /// <summary>
        /// Does a rough transformation of a polyline around a hinge point.
        /// </summary>
        /// <param name="oldLine">polyline to transform</param>
        /// <param name="hingePoint">hinge point in untransformed coordinates</param>
        /// <param name="gpsPoint">projected (GPS) coordinates of the hinge point</param>
        /// <param name="bearing">angle to rotate by</param>
        /// <returns>new transformed polyline</returns>
        private IPolyline TransformPolyline(IPolyline oldLine, IPoint hingePoint, IPoint gpsPoint,
            double bearing)
        {
            IPointCollection oldPoints = (IPointCollection)oldLine;
            IPointCollection newPoints = (IPointCollection)((IClone)oldLine).Clone();
            for (int i = 0; i < oldPoints.PointCount; i++)
                newPoints.UpdatePoint(i, TransformPoint(oldPoints.Point[i], hingePoint, gpsPoint, bearing));
            return (IPolyline)newPoints;
        }

        /// <summary>
        /// Does a rough transformation of a polygon around a hinge point.
        /// </summary>
        /// <param name="oldPolygon">polygon to transform</param>
        /// <param name="hingePoint">hinge point in untransformed coordinates</param>
        /// <param name="gpsPoint">projected (GPS) coordinates of the hinge point</param>
        /// <param name="bearing">angle to rotate by</param>
        /// <returns>new transformed polygon</returns>
        private IPolygon TransformPolygon(IPolygon oldPolygon, IPoint hingePoint, IPoint gpsPoint,
            double bearing)
        {
            IPointCollection oldPoints = (IPointCollection)oldPolygon;
            IPointCollection newPoints = (IPointCollection)((IClone)oldPolygon).Clone();
            for (int i = 0; i < oldPoints.PointCount; i++)
                newPoints.UpdatePoint(i, TransformPoint(oldPoints.Point[i], hingePoint, gpsPoint, bearing));
            return (IPolygon)newPoints;
        }

        /// <summary>
        /// Transforms the geometries in a feature class based on a hinge point and bearing and puts
        /// the results in a graphics layer.
        /// </summary>
        /// <param name="fc">feature class to transform</param>
        /// <param name="hingePoint">hinge point in untransformed coordinates</param>
        /// <param name="gpsPoint">projected (GPS) coordinates of the hinge point</param>
        /// <param name="bearing">angle to rotate by</param>
        /// <param name="layerName">name of graphics layer</param>
        /// <param name="color">color for the graphics</param>
        /// <returns>extent of the new graphics layer</returns>
        private IEnvelope TransformFeatures2Graphics(IFeatureClass fc, IPoint hingePoint,
            IPoint gpsPoint, double bearing, string layerName, IColor color)
        {
            IEnvelope extent = new EnvelopeClass();
            IGraphicsContainer graphicsContainer = (IGraphicsContainer)GetGraphicsLayer(layerName);
            IElement element;
            IFeature row;
            IFeatureCursor rows = fc.Search(null, true);
            if (fc.ShapeType == esriGeometryType.esriGeometryPoint)
            {
                ISimpleMarkerSymbol symbol = new SimpleMarkerSymbolClass();
                symbol.Size = 5;
                symbol.Color = color;
                row = rows.NextFeature();
                while (row != null)
                {
                    element = new MarkerElementClass();
                    ((IMarkerElement)element).Symbol = symbol;
                    element.Geometry = TransformPoint((IPoint)row.Shape, hingePoint, gpsPoint, bearing);
                    extent.Union(element.Geometry.Envelope);
                    graphicsContainer.AddElement(element, 0);
                    row = rows.NextFeature();
                }
            }
            else if (fc.ShapeType == esriGeometryType.esriGeometryPolyline)
            {
                ISimpleLineSymbol symbol = new SimpleLineSymbolClass();
                symbol.Color = color;
                row = rows.NextFeature();
                while (row != null)
                {
                    element = new LineElementClass();
                    ((ILineElement)element).Symbol = symbol;
                    element.Geometry = TransformPolyline((IPolyline)row.Shape, hingePoint, gpsPoint, bearing);
                    extent.Union(element.Geometry.Envelope);
                    graphicsContainer.AddElement(element, 0);
                    row = rows.NextFeature();
                }
            }
            else if (fc.ShapeType == esriGeometryType.esriGeometryPolygon)
            {
                ISimpleFillSymbol symbol = new SimpleFillSymbolClass();
                symbol.Color = color;
                row = rows.NextFeature();
                while (row != null)
                {
                    element = new PolygonElementClass();
                    ((IFillShapeElement)element).Symbol = symbol;
                    element.Geometry = TransformPolygon((IPolygon)row.Shape, hingePoint, gpsPoint, bearing);
                    extent.Union(element.Geometry.Envelope);
                    graphicsContainer.AddElement(element, 0);
                    row = rows.NextFeature();
                }
            }
            return extent;
        }

        #endregion Geometry transforms

        #region Misc methods

        /// <summary>
        /// Checks if a geometry type is one of the supported ones.
        /// </summary>
        /// <param name="geomType">geometry type to check</param>
        /// <returns>true if supported, false if not</returns>
        private bool IsValidGeom(esriGeometryType geomType)
        {
            return _validGeomTypes.Contains(geomType);
        }

        /// <summary>
        /// Checks if an index is valid (1,2,3).
        /// </summary>
        /// <param name="i">index to check</param>
        /// <returns>true if valid, false if not</returns>
        private bool IsValidIndex(int i)
        {
            return i > 0 && i < 4;
        }

        /// <summary>
        /// Creates a basename for an output feature class by stripping off an _unprojected suffix
        /// and adding a _projected suffix for shapefiles.
        /// </summary>
        /// <param name="name">basename of the original feature class</param>
        /// <returns>new basename</returns>
        private string MakeOutputFCBaseName(string name)
        {
            if (_dataWSType == esriWorkspaceType.esriLocalDatabaseWorkspace)
            {
                if (name.EndsWith("_Unprojected"))
                    return name.Replace("_Unprojected", "");
                else
                    return name + "_Projected";
            }
            else
            {
                if (name.EndsWith("_Unprojected"))
                    return name.Replace("_Unprojected", "_Projected");
                else
                    return name + "_Projected";
            }
        }

        /// <summary>
        /// Takes a desired name and a backup name (the first available name like the desired name)
        /// </summary>
        /// <param name="desiredName">name to free up</param>
        /// <param name="backupName">replacement name for existing feature class</param>
        private void ManageVersions(string desiredName, string backupName)
        {
            if (desiredName.ToLower() != backupName.ToLower())
                ((IDataset)_dataFWS.OpenFeatureClass(desiredName)).Rename(backupName);
        }

        /// <summary>
        /// Takes a desired name and a backup name (the first available name like the desired name)
        /// </summary>
        /// <param name="desiredName">name to free up</param>
        /// <param name="backupName">replacement name for existing feature class</param>
        private void ManageTableVersions(string desiredName, string backupName)
        {
            if (desiredName.ToLower() != backupName.ToLower())
                ((IDataset)_dataFWS.OpenTable(desiredName)).Rename(backupName);
        }

        /// <summary>
        /// Get a graphics layer in the map. Creates one if it doesn't already exist.
        /// </summary>
        /// <param name="name">name of the graphics layer</param>
        /// <returns>the graphics layer</returns>
        private IGraphicsLayer GetGraphicsLayer(string name)
        {
            if (_map == null)
                throw new Exception("Map not set.");
            IGraphicsLayer graphicsLayer = EsriUtilities.GetGraphicsLayer(_map, name);
            if (graphicsLayer == null)
            {
                graphicsLayer = EsriUtilities.MakeGraphicsLayer(_map, name);
                _graphicsLayers.Add(name);
            }
            return graphicsLayer;
        }

        #endregion Misc methods

        #endregion Private methods

        #region Public methods

        #region Setting data

        /// <summary>
        /// Set the datum transform to WGS84.
        /// </summary>
        /// <param name="geotransform"></param>
        public void SetGeoTransform(IGeoTransformation geotransform)
        {
            _geotransform = geotransform;
        }

        /// <summary>
        /// Set the map that things will be happening in.
        /// </summary>
        /// <param name="map">map</param>
        public void SetMap(IMap map)
        {
            this._map = map;
        }

        /// <summary>
        /// Set the spatial reference that the new points will be in.
        /// </summary>
        /// <param name="spatialReference"></param>
        public void SetSpatialReference(ISpatialReference spatialReference)
        {
            _spatialRef = spatialReference;
        }

        /// <summary>
        /// Sets the workspace that the input data will come from and the output data will be saved in.
        /// </summary>
        /// <param name="workspace">feature workspace</param>
        public void SetWorkspace(IFeatureWorkspace workspace)
        {
            _dataWSType = ((IWorkspace)workspace).Type;
            if (_dataWSType != esriWorkspaceType.esriLocalDatabaseWorkspace &&
                _dataWSType != esriWorkspaceType.esriFileSystemWorkspace)
            {
                throw new Exception(((IWorkspace)_dataFWS).PathName + " is not a supported workspace.");
            }
            _dataFWS = workspace;
            _dataWS2 = (IWorkspace2)workspace;
        }

        /// <summary>
        /// Sets the point for a benchmark.
        /// </summary>
        /// <param name="index">benchmark index</param>
        /// <param name="benchmark">the point</param>
        public void SetBenchmarkPoint(int index, IPoint benchmark)
        {
            if (!IsValidIndex(index))
                throw new Exception("Invalid benchmark index.");
            _gpsBenchmarks[index] = benchmark;
        }

        /// <summary>
        /// <summary>
        /// Sets the projected GPS coordinates for a benchmark.
        /// </summary>
        /// <param name="index">benchmark index</param>
        /// <param name="x">easting</param>
        /// <param name="y">northing</param>
        /// <param name="z">elevation</param>
        public void SetBenchmarkProj(int index, double x, double y, double z)
        {
            if (!IsValidIndex(index))
                throw new Exception("Invalid benchmark index.");
            IPoint benchmark = new PointClass();
            benchmark.SpatialReference = _spatialRef;
            ((IZAware)benchmark).ZAware = true;
            benchmark.X = x;
            benchmark.Y = y;
            benchmark.Z = z;
            _gpsBenchmarks[index] = benchmark;
            _gpsBearingsDirty = true;
        }

        /// <summary>
        /// Sets the unprojected GPS coordinates for a benchmark.
        /// </summary>
        /// <param name="index">benchmark index</param>
        /// <param name="x">longitude, dd or dms</param>
        /// <param name="y">latitude, dd or dms</param>
        /// <param name="z">elevation</param>
        public void SetBenchmarkGeo(int index, string x, string y, double z)
        {
            if (!IsValidIndex(index))
                throw new Exception("Invalid benchmark index.");
            if (!GeoUtilities.CheckGeoCoordinate(x) || !GeoUtilities.CheckGeoCoordinate(y))
                throw new Exception("Invalid coordinates.");
            double xReal, yReal;
            if (!double.TryParse(x, out xReal))
                GeoUtilities.DmsToDeg(x, out xReal);
            if (!double.TryParse(y, out yReal))
                GeoUtilities.DmsToDeg(y, out yReal);
            ISpatialReferenceFactory3 srFactory = new SpatialReferenceEnvironmentClass();
            IGeographicCoordinateSystem geoCS = srFactory.CreateGeographicCoordinateSystem(
                (int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
            IGeoTransformation trans = null;
            if (_spatialRef is IProjectedCoordinateSystem)
                trans = (IGeoTransformation)srFactory.CreateGeoTransformation(
                    (int)esriSRGeoTransformationType.esriSRGeoTransformation_NAD1983_To_WGS1984_1);
            IPoint benchmark = new PointClass();
            benchmark.SpatialReference = geoCS;
            ((IZAware)benchmark).ZAware = true;
            benchmark.X = xReal;
            benchmark.Y = yReal;
            benchmark.Z = z;
            if (!EsriUtilities.IsWGS84(_spatialRef))
            {
                if (_geotransform != null)
                {
                    esriTransformDirection dir;
                    ISpatialReference fromSr;
                    ISpatialReference toSr;
                    _geotransform.GetSpatialReferences(out fromSr, out toSr);
                    if (EsriUtilities.IsWGS84(toSr))
                        dir = esriTransformDirection.esriTransformForward;
                    else
                        dir = esriTransformDirection.esriTransformReverse;
                    ((IGeometry2)benchmark).ProjectEx(_spatialRef, dir, _geotransform, false, 0, 0);
                }
                else
                    benchmark.Project(EsriUtilities.GetWGS84());
            }

            _gpsBenchmarks[index] = benchmark;
            _gpsBearingsDirty = true;
        }

        /// <summary>
        /// Sets a compass bearing.
        /// </summary>
        /// <param name="fromIndex">from benchmark index</param>
        /// <param name="toIndex">to benchmark index</param>
        /// <param name="bearing">bearing</param>
        public void SetCompassBearing(int fromIndex, int toIndex, double bearing)
        {
            if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex) || fromIndex == toIndex)
                throw new Exception("Invalid indices.");
            _compassBearings[fromIndex.ToString() + toIndex.ToString()] = bearing;
            _compassBearings[toIndex.ToString() + fromIndex.ToString()] = TrigUtilities.ReverseBearing(bearing);
        }

        /// <summary>
        /// Sets the control points feature class.
        /// </summary>
        /// <param name="dsName">name object for the control points feature class</param>
        public void SetControlPoints(IFeatureClassName fcName)
        {
            if (fcName != null)
            {
                IFeatureClass tempControlPts = (IFeatureClass)((IName)fcName).Open();
                if (tempControlPts.ShapeType != esriGeometryType.esriGeometryPoint)
                    throw new Exception(((IName)fcName).NameString + " is not a point feature class.");
                _controlPts = tempControlPts;
            }
            else
            {
                _controlPts = null;
            }
        }

        /// <summary>
        /// Set the field in the control points feature class that contains the benchmark id values. 
        /// </summary>
        /// <param name="name">name of field</param>
        public void SetAttField(string name)
        {
            if (_controlPts == null)
                throw new Exception("Control points feature class is not set.");
            if (_controlPts.FindField(name) == -1)
                throw new Exception("Field " + name + " does not exist in the control points feature class.");
            _attField = name;
        }

        /// <summary>
        /// Set a totoal station benchmark ID from the control points file.
        /// </summary>
        /// <param name="index">benchmark index</param>
        /// <param name="attribute">attribute value that denotes that benchmark</param>
        public void SetTSBenchmark(int index, string attribute)
        {
            if (_controlPts == null)
                throw new Exception("Control points feature class is not set.");
            if (_attField == null)
                throw new Exception("Control points benchmark ID attribute field is not set.");
            if (!IsValidIndex(index))
                throw new Exception("Invalid benchmark idnex.");
            IQueryFilter query = new QueryFilterClass();
            query.WhereClause = string.Format("\"{0}\" = '{1}'", _attField, attribute);
            IFeatureCursor rows = _controlPts.Search(query, true);
            int shpIndex = rows.FindField(_controlPts.ShapeFieldName);
            IFeature row = rows.NextFeature();
            if (row == null)
                throw new Exception("Invalid attribute.");
            _attIds[index] = attribute;
            _tsBenchmarks[index] = (IPoint)row.ShapeCopy;
            _tsBearingsDirty = true;
        }

        /// <summary>
        /// Add a feature class to be transformed.
        /// </summary>
        /// <param name="fcName">name object for the feature class</param>
        public void AddData(IFeatureClassName fcName)
        {
            if (IsValidGeom(fcName.ShapeType))
                _inDataFcNames.Add(fcName);
        }

        /// <summary>
        /// Clears all feature classes out of the list to be transformed.
        /// </summary>
        public void ClearData()
        {
            _inDataFcNames.Clear();
        }

        #endregion Setting data

        #region Getting data

        public string GetFullErrorMessage()
        {
            return _fullErrorMsg;
        }

        /// <summary>
        /// Get the output datum transformation.
        /// </summary>
        /// <returns>geotransformation</returns>
        public IGeoTransformation GetGeoTransform()
        {
            return _geotransform;
        }

        /// <summary>
        /// Get the output spatial reference.
        /// </summary>
        /// <returns>spatial reference</returns>
        public ISpatialReference GetSpatialReference()
        {
            return _spatialRef;
        }

        /// <summary>
        /// Get the path of the workspace that data are coming from and being saved to.
        /// </summary>
        /// <returns>path to the workspace</returns>
        public string GetWorkspacePath()
        {
            if (_dataFWS != null)
                return ((IWorkspace)_dataFWS).PathName;
            else
                return "";
        }

        public IPoint GetBenchmark(int index)
        {
            if (!IsValidIndex(index))
                throw new Exception("Invalid index.");
            return _gpsBenchmarks[index];
        }

        /// <summary>
        /// Gets a list of possible input feature classes. If the workspace is geodatabase and there
        /// is an Unprojected feature dataset, then returns the feature classes in the dataset. 
        /// Otherwise returns all feature classes at the top level of the workspace.
        /// </summary>
        /// <returns>list of name objects</returns>
        public List<IDatasetName> GetFeatureClassNames()
        {
            if (_dataFWS == null)
                throw new Exception("Workspace is not set.");
            if (_dataWSType == esriWorkspaceType.esriLocalDatabaseWorkspace &&
                _dataWS2.get_NameExists(esriDatasetType.esriDTFeatureDataset, "Unprojected" ))
            {
                return EsriUtilities.GetFeatureClassNames(esriGeometryType.esriGeometryAny,
                    (IWorkspace)_dataFWS, "Unprojected");
            }
            else
            {
                return EsriUtilities.GetFeatureClassNames(esriGeometryType.esriGeometryAny,
                    (IWorkspace)_dataFWS);
            }
        }

        /// <summary>
        /// Gets a list of string attribute fields in the control points feature class.
        /// </summary>
        /// <returns>sorted list of fields</returns>
        public List<string> GetAttFields()
        {
            if (_controlPts == null)
                throw new Exception("Control Points feature class is not set.");
            List<string> fieldNames = new List<string>();
            IFields fields = _controlPts.Fields;
            IField field;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                field = fields.Field[i];
                if (field.Type == esriFieldType.esriFieldTypeString)
                    fieldNames.Add(field.Name);
            }
            fieldNames.Sort();
            return fieldNames;
        }

        /// <summary>
        /// Gets a list of all of the values in the benchmark id field in the control points feature class.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAttValues()
        {
            if (_controlPts == null)
                throw new Exception("Control points feature class is not set.");
            if (_attField == null)
                throw new Exception("Control points benchmark ID attribute field is not set.");
            List<string> values = new List<string>();
            IFeatureCursor rows = _controlPts.Search(null, true);
            int fieldIndex = rows.FindField(_attField);
            IFeature row = rows.NextFeature();
            while (row != null)
            {
                values.Add(row.Value[fieldIndex].ToString());
                row = rows.NextFeature();
            }
            values.Sort();
            return values;
        }

        /// <summary>
        /// Gets the calculated error for an ID. ID values are this format (case sensitive):
        /// "H" or "Z" for horizontal or vertical error
        /// "GPS" or "Compass" for type of bearing
        /// number 1-3 for hinge index
        /// number 1-3 for the index of the bearing used (1->2, 1->3, 2->3)
        /// number 1-3 for the index of the benchmark the error is for
        /// so "HGPS123" would get the horizontal error for benchmark 3, using benchmark 1 as the
        /// hinge and the GPS bearing from benchmark 1 to benchmark 3
        /// </summary>
        /// <param name="id">ID of the error to get</param>
        /// <param name="error">variable that the error value will be stored in, will get -999 if no 
        /// valid value exists</param>
        /// <returns>true if there is an error for that ID, false otherwise</returns>
        public bool GetError(string id, out double error)
        {
            if (_gpsBearingsDirty || _tsBearingsDirty)
                ComputeErrors();
            if (_errors.ContainsKey(id))
            {
                error = _errors[id];
                return true;
            }
            else
            {
                error = -999;
                return false;
            }
        }

        /// <summary>
        /// Checks if a compass bearing for the ID exists. The ID strings are made up of two indices:
        /// the from index and the to index. For example, "23" would be the compass bearing from 
        /// benchmark 2 to benchmark 3.
        /// </summary>
        /// <param name="id">ID to check</param>
        /// <returns>true if there is a compass bearing with the ID, false otherwise</returns>
        public bool HasCompassBearing(string id)
        {
            return _compassBearings.ContainsKey(id);
        }

        #endregion Getting data

        #region Work

        /// <summary>
        /// Compute all possible transforms for feature class and add the results as graphics layers 
        /// using the color provided. 
        /// </summary>
        /// <param name="fcName">name object for the feature class to transform</param>
        /// <param name="color">color to draw the graphics with</param>
        /// <returns>extent of all of the new graphics layers</returns>
        public IEnvelope TransformFeatures2Graphics(IFeatureClassName fcName, IColor color)
        {
            if (_gpsBearingsDirty || _tsBearingsDirty)
                ComputeErrors();

            IPoint gpsBM, tsBM;
            double tsBearing, bearing;
            string bearingId, layerId;
            IFeatureClass fc = (IFeatureClass)((IName)fcName).Open();
            IEnvelope extent = new EnvelopeClass();

            // Loop through each index and use that GPS benchmark as a hinge
            for (int hingeIndex = 1; hingeIndex <= 3; hingeIndex++)
            {
                gpsBM = _gpsBenchmarks[hingeIndex];
                tsBM = _tsBenchmarks[hingeIndex];

                // Go to the next hinge if we don't have both GPS and total station coordinates for this index
                if (_gpsBenchmarks == null || tsBM == null)
                    continue;

                // Loop through each index and use it as the rotated to benchmark
                for (int bearingIndex = 1; bearingIndex <= 3; bearingIndex++)
                {
                    // Skip if it's the same as the hinge
                    if (bearingIndex == hingeIndex)
                        continue;

                    // Skip if we don't have a total station bearing between the hinge and this benchmark
                    bearingId = hingeIndex.ToString() + bearingIndex.ToString();
                    if (!_tsBearings.ContainsKey(bearingId))
                        continue;
                    tsBearing = _tsBearings[bearingId];

                    // Transform using the GPS bearing if one exists between the hinge and this benchmark
                    if (_gpsBearings.ContainsKey(bearingId))
                    {
                        layerId = "gps" + bearingId;
                        bearing = tsBearing - _gpsBearings[bearingId];
                        extent.Union(TransformFeatures2Graphics(fc, tsBM, gpsBM, bearing, layerId, color));
                        ((ILayer)GetGraphicsLayer(layerId)).Visible = false;
                    }

                    // Transform using the compass bearing if one exists between the hinge and this benchmark
                    if (_compassBearings.ContainsKey(bearingId))
                    {
                        layerId = "compass" + bearingId;
                        bearing = tsBearing - _compassBearings[bearingId];
                        extent.Union(TransformFeatures2Graphics(fc, tsBM, gpsBM, bearing, layerId, color));
                        ((ILayer)GetGraphicsLayer(layerId)).Visible = false;
                    }
                }
            }
            return extent;
        }

        /// <summary>
        /// Transform all input feature classes using the transform defined here.
        /// </summary>
        /// <param name="hingeIndex">index of the hinge point</param>
        /// <param name="toBenchmarkIndex">index of the benchmark that the bearing is to</param>
        /// <param name="isBearingGPS">true if we should use the GPS bearing, false for the compass bearing</param>
        public void SaveData(int hingeIndex, int toBenchmarkIndex, bool isBearingGPS)
        {
            if (_gpsBearingsDirty || _tsBearingsDirty)
                ComputeErrors();

            IPoint gpsBenchmark = _gpsBenchmarks[hingeIndex];
            IPoint tsBenchmark = _tsBenchmarks[hingeIndex];
            if (gpsBenchmark == null || tsBenchmark == null)
                throw new Exception("Invalid hinge index.");

            string bearingId = hingeIndex.ToString() + toBenchmarkIndex.ToString();
            if (!_tsBearings.ContainsKey(bearingId))
                throw new Exception("Invalid from and to benchmark combination.");
            double bearing;
            if (isBearingGPS)
            {
                if (!_gpsBearings.ContainsKey(bearingId))
                    throw new Exception("Invalid from and to benchmark combination for GPS bearings.");
                bearing = _tsBearings[bearingId] - _gpsBearings[bearingId];
            }
            else
            {
                if (!_compassBearings.ContainsKey(bearingId))
                    throw new Exception("Invalid from and to benchmark combination for compass bearings.");
                bearing = _tsBearings[bearingId] - _compassBearings[bearingId];
            }

            // Create the benchmark feature class
            CreateBenchmarkFeatureClass();

            IFeatureClass outFc;
            IFeatureLayer outFl;
            ChampTransformErrorsClass errors;
            string name, backupName;

            // Transform the control points
            name = MakeOutputFCBaseName(((IDataset)_controlPts).BrowseName);
            backupName = EsriUtilities.MakeUniqueName(_dataWS2, esriDatasetType.esriDTFeatureClass, name);
            ManageVersions(name, backupName);
            _controlPtsOutputFn = ((IWorkspace)_dataFWS).PathName + "\\" + name;
            errors = new ChampTransformErrorsClass(_controlPtsOutputFn);
            _outFcErrors[_controlPtsOutputFn] = errors;
            outFc = TransformFeatureClass(_controlPts, name, tsBenchmark, gpsBenchmark, bearing, errors);
            outFl = new FeatureLayerClass();
            outFl.FeatureClass = outFc;
            outFl.Name = name;
            _map.AddLayer(outFl);
            _outDataFc.Add(outFc);

            // Transform all other layers
            IFeatureClass inFc;
            IDataset inDs;
            string outPath;
            foreach (IFeatureClassName fcName in _inDataFcNames)
            {
                inFc = (IFeatureClass)((IName)fcName).Open();
                name = MakeOutputFCBaseName(((IDataset)inFc).BrowseName);
                backupName = EsriUtilities.MakeUniqueName(_dataWS2, esriDatasetType.esriDTFeatureClass, name);
                ManageVersions(name, backupName);
                inDs = (IDataset)inFc;
                outPath = ((IWorkspace)_dataFWS).PathName + "\\" + name;
                _dataFns[inDs.Workspace.PathName + "\\" + inDs.Name] = outPath;
                errors = new ChampTransformErrorsClass(outPath);
                _outFcErrors[outPath] = errors;
                outFc = TransformFeatureClass(inFc, name, tsBenchmark, gpsBenchmark, bearing, errors);
                outFl = new FeatureLayerClass();
                outFl.FeatureClass = outFc;
                outFl.Name = name;
                _map.AddLayer(outFl);
                _outDataFc.Add(outFc);

            }

            // Remove the graphics layers because now we have the output feature classes
            RemoveGraphicsLayers();

            // Update the QaQcPoints table if it exists
            IEnumDatasetName dsNames = ((IWorkspace)_dataFWS).get_DatasetNames(esriDatasetType.esriDTTable);
            IDatasetName dsName = dsNames.Next();
            while (dsName != null)
            {
                if (dsName.Name == "QaQcPoints")
                {
                    CalculatePointZStatistics((ITable)((IName)dsName).Open());
                    break;
                }
                dsName = dsNames.Next();
            }
        }

        /// <summary>
        /// Transform all input feature classes using a previous transform so not as much needs to
        /// be done.
        /// </summary>
        /// <param name="hingeIndex">index of the hinge point</param>
        /// <param name="toBenchmarkIndex">index of the benchmark that the bearing is to</param>
        public void SaveData(int hingeIndex, int toBenchmarkIndex)
        {
            CalculateGPSBearings();
            CalculateTSBearings();

            IPoint gpsBenchmark = _gpsBenchmarks[hingeIndex];
            IPoint tsBenchmark = _tsBenchmarks[hingeIndex];
            if (gpsBenchmark == null || tsBenchmark == null)
                throw new Exception("Invalid hinge index.");

            string bearingId = hingeIndex.ToString() + toBenchmarkIndex.ToString();
            if (!_tsBearings.ContainsKey(bearingId))
                throw new Exception("Invalid from and to benchmark combination.");
            double bearing;
            if (!_gpsBearings.ContainsKey(bearingId))
                throw new Exception("Invalid from and to benchmark combination for GPS bearings.");
            bearing = _tsBearings[bearingId] - _gpsBearings[bearingId];

            IFeatureClass outFc;
            IFeatureLayer outFl;
            ChampTransformErrorsClass errors;
            string name, backupName;

            // Transform the control points
            name = MakeOutputFCBaseName(((IDataset)_controlPts).BrowseName);
            backupName = EsriUtilities.MakeUniqueName(_dataWS2, esriDatasetType.esriDTFeatureClass, name);
            ManageVersions(name, backupName);
            _controlPtsOutputFn = ((IWorkspace)_dataFWS).PathName + "\\" + name;
            errors = new ChampTransformErrorsClass(_controlPtsOutputFn);
            _outFcErrors[_controlPtsOutputFn] = errors;
            outFc = TransformFeatureClass(_controlPts, name, tsBenchmark, gpsBenchmark, bearing, errors);
            outFl = new FeatureLayerClass();
            outFl.FeatureClass = outFc;
            outFl.Name = name;
            _map.AddLayer(outFl);
            _outDataFc.Add(outFc);

            // Transform all other layers
            IFeatureClass inFc;
            IDataset inDs;
            string outPath;
            foreach (IFeatureClassName fcName in _inDataFcNames)
            {
                inFc = (IFeatureClass)((IName)fcName).Open();
                name = MakeOutputFCBaseName(((IDataset)inFc).BrowseName);
                backupName = EsriUtilities.MakeUniqueName(_dataWS2, esriDatasetType.esriDTFeatureClass, name);
                ManageVersions(name, backupName);
                inDs = (IDataset)inFc;
                outPath = ((IWorkspace)_dataFWS).PathName + "\\" + name;
                _dataFns[inDs.Workspace.PathName + "\\" + inDs.Name] = outPath;
                errors = new ChampTransformErrorsClass(outPath);
                _outFcErrors[outPath] = errors;
                outFc = TransformFeatureClass(inFc, name, tsBenchmark, gpsBenchmark, bearing, errors);
                outFl = new FeatureLayerClass();
                outFl.FeatureClass = outFc;
                outFl.Name = name;
                _map.AddLayer(outFl);
                _outDataFc.Add(outFc);

            }
        }

        /// <summary>
        /// Remove all of the graphics layers that we've added up until this point.
        /// </summary>
        public void RemoveGraphicsLayers()
        {
            if (_map == null)
                throw new Exception("Map not set.");
            ICompositeGraphicsLayer mainGraphicsLayer = (ICompositeGraphicsLayer)_map.BasicGraphicsLayer;
            foreach (string name in _graphicsLayers)
                mainGraphicsLayer.DeleteLayer(name);
        }

        #endregion Work

        #region CHAMP-specific stuff

        /// <summary>
        /// Updates the table with z statistics for each of the classifications.
        /// </summary>
        /// <param name="table">table to update</param>
        public void CalculatePointZStatistics(ITable table)
        {
            int codeFieldIndex = table.FindField("Code");
            if (codeFieldIndex == -1)
                throw new Exception("Code field does not exist.");

            // Add the new fields
            string zMinName = EsriUtilities.MakeFieldName(table, "ZminProj");
            IField zMin = EsriUtilities.MakeFieldDouble(zMinName, 3);
            string zMaxName = EsriUtilities.MakeFieldName(table, "ZmaxProj");
            IField zMax = EsriUtilities.MakeFieldDouble(zMaxName, 3);
            string zRangeName = EsriUtilities.MakeFieldName(table, "ZrangeProj");
            IField zRange = EsriUtilities.MakeFieldDouble(zRangeName, 3);
            ISchemaLock schemalock = (ISchemaLock)table;
            schemalock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);
            table.AddField(zMin);
            table.AddField(zMax);
            table.AddField(zRange);
            schemalock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
            int minFieldIndex = table.FindField(zMinName);
            int maxFieldIndex = table.FindField(zMaxName);
            int rangeFieldIndex = table.FindField(zRangeName);

            // Find the min and max z values for each description
            Dictionary<string, double> zMins = new Dictionary<string, double>();
            Dictionary<string, double> zMaxs = new Dictionary<string, double>();
            ICursor tableRows = table.Search(null, true);
            IRow tableRow = tableRows.NextRow();
            while (tableRow != null)
            {
                zMins[tableRow.Value[codeFieldIndex].ToString()] = 9999999999;
                zMaxs[tableRow.Value[codeFieldIndex].ToString()] = -9999999999;
                tableRow = tableRows.NextRow();
            }
            IFeatureCursor fcRows;
            IFeature fcRow;
            int descriptionFieldIndex;
            string code;
            double z;
            foreach (IFeatureClass fc in _outDataFc)
            {
                descriptionFieldIndex = fc.FindField("DESCRIPTION");
                if (fc.ShapeType == esriGeometryType.esriGeometryPoint && descriptionFieldIndex > -1)
                {
                    fcRows = (IFeatureCursor)fc.Search(null, true);
                    fcRow = fcRows.NextFeature();
                    while (fcRow != null)
                    {
                        code = fcRow.Value[descriptionFieldIndex].ToString();
                        if (zMins.ContainsKey(code))
                        {
                            z = ((IPoint)fcRow.Shape).Z;
                            if (z < zMins[code])
                                zMins[code] = z;
                            if (z > zMaxs[code])
                                zMaxs[code] = z;
                        }
                        fcRow = fcRows.NextFeature();
                    }
                }
            }

            // Update the table
            tableRows = table.Update(null, false);
            tableRow = tableRows.NextRow();
            while (tableRow != null)
            {
                code = tableRow.Value[codeFieldIndex].ToString();
                if (code != "All" && code != "Bad")
                {
                    tableRow.Value[minFieldIndex] = zMins[code];
                    tableRow.Value[maxFieldIndex] = zMaxs[code];
                    tableRow.Value[rangeFieldIndex] = zMaxs[code] - zMins[code];
                    tableRows.UpdateRow(tableRow);
                }
                tableRow = tableRows.NextRow();
            }
        }

        #endregion CHAMP-specific stuff

        #endregion Public methods

    }
}
