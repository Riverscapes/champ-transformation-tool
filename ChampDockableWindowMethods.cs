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

using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using RSGIS.Utilites;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CHAMP
{

    /// <summary>
    /// Designer class of the dockable window add-in. It contains user interfaces that
    /// make up the dockable window.
    /// </summary>
    public partial class ChampDockableWindow : UserControl
    {

        /// <summary>
        /// Adds a datasetname object to the listview of feature classes.
        /// </summary>
        /// <param name="dsName">name object to add</param>
        private void AddItemToListViewData(IDatasetName dsName)
        {
            ListViewItem item = new ListViewItem(dsName.Name);
            item.UseItemStyleForSubItems = false;
            esriGeometryType geomType = ((IFeatureClassName)dsName).ShapeType;
            if (geomType == esriGeometryType.esriGeometryPoint)
                item.SubItems.Add("Point");
            else if (geomType == esriGeometryType.esriGeometryPolyline)
                item.SubItems.Add("Line");
            else if (geomType == esriGeometryType.esriGeometryPolygon)
                item.SubItems.Add("Polygon");
            item.SubItems.Add("remove");
            item.SubItems[2].ForeColor = Color.Blue;
            item.Tag = dsName;
            if (dsName.Name == "EdgeofWater_Points_Unprojected" || dsName.Name == "Topo_Points_Unprojected")
                item.Checked = true;
            listViewData.Items.Add(item);
        }

        /// <summary>
        /// Rechecks all of the lat/lon coordinates in case the coordinate system has changed between
        /// geographic and projected.
        /// </summary>
        private void CheckAllCoords()
        {
            _bm1LongOK = CheckCoords(textBoxLon1.Text);
            _bm1LatOK = CheckCoords(textBoxLat1.Text);
            _bm2LongOK = CheckCoords(textBoxLon2.Text);
            _bm2LatOK = CheckCoords(textBoxLat2.Text);
            _bm3LongOK = CheckCoords(textBoxLon3.Text);
            _bm3LatOK = CheckCoords(textBoxLat3.Text);
        }

        /// <summary>
        /// Rechecks all of the lat/lon coordinates in case the coordinate system has changed between
        /// geographic and projected.
        /// </summary>
        private bool CheckAllCoords(bool projected)
        {
            if (projected)
            {
                return GeoUtilities.CheckProjCoordinate(textBoxLon1.Text) && 
                    GeoUtilities.CheckProjCoordinate(textBoxLat1.Text) && 
                    GeoUtilities.CheckProjCoordinate(textBoxLon2.Text) && 
                    GeoUtilities.CheckProjCoordinate(textBoxLat2.Text) && 
                    GeoUtilities.CheckProjCoordinate(textBoxLon3.Text) && 
                    GeoUtilities.CheckProjCoordinate(textBoxLat3.Text);
            }
            else
            {
                return GeoUtilities.CheckGeoCoordinate(textBoxLon1.Text) &&
                    GeoUtilities.CheckGeoCoordinate(textBoxLat1.Text) &&
                    GeoUtilities.CheckGeoCoordinate(textBoxLon2.Text) &&
                    GeoUtilities.CheckGeoCoordinate(textBoxLat2.Text) &&
                    GeoUtilities.CheckGeoCoordinate(textBoxLon3.Text) &&
                    GeoUtilities.CheckGeoCoordinate(textBoxLat3.Text);
            }
        }

        /// <summary>
        /// Checks to see if a string is a valid bearing value.
        /// </summary>
        /// <param name="str">string to check</param>
        /// <returns>true if the string is null or can be converted to a valid bearing, false if
        /// not</returns>
        private bool CheckBearing(string str)
        {
            return string.IsNullOrEmpty(str) || GeoUtilities.CheckBearing(str);
        }

        /// <summary>
        /// Makes sure that different attributes are chosen for each benchmark.
        /// </summary>
        private void CheckBenchmarkIds()
        {
            int i1 = comboBoxBM1Att.SelectedIndex;
            int i2 = comboBoxBM2Att.SelectedIndex;
            int i3 = comboBoxBM3Att.SelectedIndex;
            _benchmarksOK = i1 > -1 && i2 > -1 && i3 > -1 && i1 != i2 && i1 != i3 && i2 != i3;
        }

        /// <summary>
        /// Checks to see if the string is a valid coordinate.
        /// </summary>
        /// <param name="str">string to check</param>
        /// <returns>true if the string can be converted to a valid coordinate value</returns>
        private bool CheckCoords(string str)
        {
            return GeoUtilities.CheckProjCoordinate(str) || GeoUtilities.CheckGeoCoordinate(str);
        }

        /// <summary>
        /// Checks to see if a string is a valid elevation value.
        /// </summary>
        /// <param name="str">string to check</param>
        /// <returns>true if the string is a number greater than 0</returns>
        private bool CheckElevation(string str)
        {
            double elevation;
            return double.TryParse(str, out elevation) && elevation > 0;
        }

        /// <summary>
        /// Deletes any existing CHaMP graphics layers.
        /// </summary>
        private void DeleteGraphicsLayers()
        {
            string bearingId;
            for (int hingeIndex = 1; hingeIndex <= 3; hingeIndex++)
            {
                for (int bearingIndex = 1; bearingIndex <= 3; bearingIndex++)
                {
                    if (bearingIndex == hingeIndex)
                        continue;
                    bearingId = hingeIndex.ToString() + bearingIndex.ToString();
                    EsriUtilities.DeleteGraphicsLayer(_map, "gps" + bearingId);
                    EsriUtilities.DeleteGraphicsLayer(_map, "compass" + bearingId);
                }
            }
        }

        /// <summary>
        /// Enables and disables the Select Inputs button based on values in the textboxes.
        /// </summary>
        private void EnableSelectInputs()
        {
            ISpatialReference sr = _transform.GetSpatialReference();
            buttonSelectInputs.Enabled = _bm1LongOK && _bm1LatOK && _bm1ElevOK && _bm2LongOK &&
                _bm2LatOK && _bm2ElevOK && _bm3LongOK && _bm3LatOK && _bm3ElevOK && _bearing12OK &&
                _bearing13OK && _bearing23OK && _workspaceOK && sr != null &&
                (EsriUtilities.IsWGS84(sr) || _transform.GetGeoTransform() != null);
        }

        /// <summary>
        /// Enables and disables the Proceed to Transformation button based on user inputs.
        /// </summary>
        private void EnableTransform()
        {
            buttonTransform.Enabled = _controlPtsOK && _benchmarksOK && _hingeOK && _bearingOK &&
                listViewData.CheckedItems.Count > 0;
        }

        /// <summary>
        /// Fills the control points combo box with the point feature classes from the data list.
        /// </summary>
        private void FillControlComboBox()
        {
            List<IDatasetName> nameList = new List<IDatasetName>();
            foreach (ListViewItem item in listViewData.Items)
            {
                if (((IFeatureClassName)item.Tag).ShapeType == esriGeometryType.esriGeometryPoint)
                    nameList.Add((IDatasetName)item.Tag);
            }
            nameList.Sort(_nameComparer);
            IDatasetName oldName = (IDatasetName)comboBoxControlPts.SelectedItem;
            comboBoxControlPts.DataSource = nameList;
            if (oldName != null)
            {
                foreach (System.Object item in comboBoxControlPts.Items)
                {
                    if (_nameComparer.Equal(oldName, (IDatasetName)item))
                    {
                        comboBoxControlPts.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Fills the datum transformations comobo box with the transforms applicable to the
        /// selected projection and WGS84.
        /// </summary>
        private void FillDatumTransform()
        {
            ISpatialReference sr = _transform.GetSpatialReference();
            if (sr != null)
            {
                IGeoTransformation trans = _transform.GetGeoTransform();
                if (trans != null)
                {
                    List<IGeoTransformation> transList = EsriUtilities.GetTransformations(sr);
                    comboBoxGeoTrans.DataSource = transList;
                    comboBoxGeoTrans.DisplayMember = "Name";
                    comboBoxGeoTrans.SelectedIndex = comboBoxGeoTrans.FindString(trans.Name);
                    comboBoxGeoTrans.Enabled = true;
                }
                else
                {
                    if (EsriUtilities.IsWGS84(sr))
                    {
                        comboBoxGeoTrans.DataSource = null;
                        comboBoxGeoTrans.Enabled = false;
                    }
                    else
                    {
                        List<IGeoTransformation> transList = EsriUtilities.GetTransformations(sr);
                        comboBoxGeoTrans.DataSource = transList;
                        comboBoxGeoTrans.DisplayMember = "Name";
                        comboBoxGeoTrans.Enabled = true;
                    }
                }
            }
            else
            {
                comboBoxGeoTrans.DataSource = null;
                comboBoxGeoTrans.Enabled = false;
            }
        }

        /// <summary>
        /// Looks for an attribute field called DESCRIPTION and returns the index of the first match.
        /// </summary>
        /// <returns>index of the first DESCRIPTION field or -1</returns>
        private int FindAttFieldIndex()
        {
            for (int i = 0; i < comboBoxAttField.Items.Count; i++)
            {
                if (comboBoxAttField.Items[i].ToString().ToUpper() == "DESCRIPTION")
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Looks for values with certain names plus the passed index in the attribute comboboxes
        /// and returns the index of the first match or -1.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private int FindBMAttIndex(string n)
        {
            for (int i = 0; i < comboBoxBM1Att.Items.Count; i++)
            {
                string att = comboBoxBM1Att.Items[i].ToString().ToLower();
                if (att == "bm" + n || att == "bm_" + n || att == "cp" + n || att == "cp_" + n)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Clears everything out of the form and the transform object.
        /// </summary>
        private void Init()
        {
            try
            {
                _map = ArcMap.Document.FocusMap;
                _transform = new ChampTransformClass();
                _bm1LongOK = _bm1LatOK = _bm1ElevOK = _bm2LongOK = _bm2LatOK = _bm2ElevOK = 
                    _bm3LongOK = _bm3LatOK = _bm3ElevOK = _workspaceOK = _controlPtsOK = 
                    _benchmarksOK = _hingeOK = _bearingOK = false;
                _bearing12OK = _bearing13OK = _bearing23OK = true;
                _viewedTransforms = new List<string>();

                comboBoxControlPts.DataSource = null;
                comboBoxControlPts.DisplayMember = "Name";
                comboBoxGeoTrans.DataSource = null;

                FormUtilities.ClearControls(panelGPS);
                FormUtilities.ClearControls(panelInput);
                FormUtilities.ClearControls(panelOutput);

                comboBoxCoordSys.Items.Add("Lat/Long (WGS84)");
                comboBoxCoordSys.Items.Add("UTM Zone 10N (NAD83)");
                comboBoxCoordSys.Items.Add("UTM Zone 11N (NAD83)");
                comboBoxCoordSys.Items.Add("UTM Zone 12N (NAD83)");
                comboBoxCoordSys.Items.Add("User Defined");
                labelSR.Text = "";

                radioButtonProj.Checked = true;

                groupBoxBMs.Enabled = false;
                groupBoxRotation.Enabled = false;

                buttonSelectInputs.Enabled = false;
                buttonTransform.Enabled = false;
                buttonSave.Enabled = false;
                buttonSR.Enabled = false;

                panelGPS.Visible = true;
                panelInput.Visible = false;
                panelOutput.Visible = false;
                panelSampleCsv.Visible = false;
                panelSampleBearings.Visible = false;

                SetOutputSpatialReferenceToMap();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Selects the item in a combo box that has the same workspace and name passed here. Items
        /// in the combo box are assumed to be IDatasetName.
        /// </summary>
        /// <param name="cb">combobox</param>
        /// <param name="workspace">workspace path to check against</param>
        /// <param name="name">datset name to check against</param>
        private void SelectComboBoxItem(ComboBox cb, string workspace, string name)
        {
            bool found = false;
            IDatasetName dsName;
            for (int i = 0; i < cb.Items.Count; i++)
            {
                dsName = (IDatasetName)cb.Items[i];
                if (dsName.WorkspaceName.PathName == workspace && dsName.Name == name)
                {
                    cb.SelectedIndex = i;
                    found = true;
                    break;
                }
            }
            if (!found)
                cb.SelectedIndex = -1;
        }

        /// <summary>
        /// Opens a dialog so the user can select an existing shapefile or file geodatabase
        /// feature class.
        /// </summary>
        /// <param name="title">title for dialog</param>
        /// <returns>dataset name object for the selected feature class or null </returns>
        private IDatasetName SelectFeatureClass(string title, esriGeometryType geom)
        {
            try
            {
                IGxObjectFilter shpFilter = new GxFilterShapefilesClass();
                IGxObjectFilter gdbFilter = new GxFilterFGDBFeatureClassesClass();
                IGxDialog dlg = new GxDialogClass();
                IGxObjectFilterCollection filters = (IGxObjectFilterCollection)dlg;
                filters.AddFilter(shpFilter, false);
                filters.AddFilter(gdbFilter, true);
                dlg.Title = title;
                IDatasetName dsName = null;
                IEnumGxObject objects = null;
                if (dlg.DoModalOpen(0, out objects))
                {
                    IGPUtilities2 util = new GPUtilitiesClass();
                    string name = objects.Next().FullName;
                    dsName = (IDatasetName)util.CreateFeatureClassName(name);
                    IFeatureWorkspace ws = (IFeatureWorkspace)((IName)dsName.WorkspaceName).Open();
                    IFeatureClass fc = ws.OpenFeatureClass(System.IO.Path.GetFileName(name));
                    esriGeometryType shp = fc.ShapeType;
                    if (!(geom == esriGeometryType.esriGeometryAny &&
                        (shp == esriGeometryType.esriGeometryPoint ||
                        shp == esriGeometryType.esriGeometryPolyline ||
                        shp == esriGeometryType.esriGeometryPolygon)) &&
                        shp != geom)
                    {
                        ShowError("Wrong geometry type.");
                        dsName = null;
                    }
                    dsName = (IDatasetName)((IDataset)fc).FullName;
                }
                return dsName;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Fills the error box denoted by the ID and enables the associated radio button if there
        /// really is an error.
        /// </summary>
        /// <param name="id">ID for the error</param>
        private void SetError(string id)
        {
            double error;
            bool exists = _transform.GetError(id, out error);
            string rbId = "radioButton" + id.Substring(1, id.Length - 2);
            ((RadioButton)panelOutput.Controls.Find(rbId, true)[0]).Enabled = exists;
            TextBox tb = (TextBox)groupBoxErrors.Controls.Find("textBox" + id, true)[0];
            tb.Text = exists ? string.Format("{0:0.00}", error) : "";
        }

        /// <summary>
        /// Gets the map's spatial reference and selects the corresponding option in the combo box
        /// if it exists.
        /// </summary>
        private void SetOutputSpatialReferenceToMap()
        {
            if (_map != null)
            {
                ISpatialReference sr = _map.SpatialReference;
                if (sr != null)
                {
                    if (sr.Name == "NAD_1983_UTM_Zone_10N")
                        comboBoxCoordSys.SelectedIndex = comboBoxCoordSys.FindString("UTM Zone 10");
                    else if (sr.Name == "NAD_1983_UTM_Zone_11N")
                        comboBoxCoordSys.SelectedIndex = comboBoxCoordSys.FindString("UTM Zone 11");
                    else if (sr.Name == "NAD_1983_UTM_Zone_12N")
                        comboBoxCoordSys.SelectedIndex = comboBoxCoordSys.FindString("UTM Zone 12");
                    else if (sr.Name == "GCS_WGS_1984")
                        comboBoxCoordSys.SelectedIndex = comboBoxCoordSys.FindString("Lat/Long (WGS84)");
                    else
                        comboBoxCoordSys.SelectedIndex = comboBoxCoordSys.FindString("User Defined");
                }
            }
        }

        /// <summary>
        /// Show an error message.
        /// </summary>
        /// <param name="msg">message to show</param>
        /// <returns>false</returns>
        private bool ShowError(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        /// <summary>
        /// Asks the user a yes or no question.
        /// </summary>
        /// <param name="msg">question to ask</param>
        /// <returns>true if the user answers yes, false if they answer no</returns>
        private bool ShowYesNoQuestion(string msg)
        {
            return MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.Yes;
        }

    }
}