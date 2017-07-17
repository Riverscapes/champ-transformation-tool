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
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RSGIS.Utilites;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CHAMP
{
    /// <summary>
    /// Designer class of the dockable window add-in. It contains user interfaces that
    /// make up the dockable window.
    /// </summary>
    public partial class ChampDockableWindow : UserControl
    {

        #region Esri generated code

        public ChampDockableWindow(object hook)
        {
            InitializeComponent();
            this.Hook = hook;
        }

        /// <summary>
        /// Host object of the dockable window
        /// </summary>
        private object Hook
        {
            get;
            set;
        }

        /// <summary>
        /// Implementation class of the dockable window add-in. It is responsible for 
        /// creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : ESRI.ArcGIS.Desktop.AddIns.DockableWindow
        {
            private ChampDockableWindow m_windowUI;

            public AddinImpl()
            {
            }

            protected override IntPtr OnCreateChild()
            {
                m_windowUI = new ChampDockableWindow(this.Hook);
                return m_windowUI.Handle;
            }

            protected override void Dispose(bool disposing)
            {
                if (m_windowUI != null)
                    m_windowUI.Dispose(disposing);

                base.Dispose(disposing);
            }

        }

        #endregion Esri generated code

        private IMap _map; // map document
        private ChampTransformClass _transform; // object that does all the real work
        private DatasetNameComparer _nameComparer; // used to compare DatasetName objects
        private GeoTransformationComparer _transformComparer; // used to compare GeoTransformation objects
        private List<string> _viewedTransforms; // transformations viewed by user, "<GPS,Compass><hinge_index><rotate_bm_index>"
        private IColor _graphicsColor; // color to draw the graphics with
        
        // variables to keep track of whether or not some user settings are valid
        private bool _bm1LongOK, _bm1LatOK, _bm1ElevOK, _bm2LongOK, _bm2LatOK, _bm2ElevOK, _bm3LongOK, _bm3LatOK, _bm3ElevOK;
        private bool _bearing12OK, _bearing13OK, _bearing23OK;
        private bool _workspaceOK;
        private bool _controlPtsOK, _benchmarksOK, _hingeOK, _bearingOK;

        /// <summary>
        /// Initializes things that only need to be set once, no matter what.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChampDockableWindow_Load(object sender, EventArgs e)
        {
            _nameComparer = new DatasetNameComparer();
            _transformComparer = new GeoTransformationComparer();
            _graphicsColor = new RgbColorClass();
            _graphicsColor.RGB = (16711680);
            panelInput.Location = panelGPS.Location;
            panelOutput.Location = panelGPS.Location;
            panelSampleCsv.Location = new System.Drawing.Point(55, panelSampleCsv.Location.Y);
            panelSampleBearings.Location = new System.Drawing.Point(55, panelSampleBearings.Location.Y);
            Init();
        }

        /// <summary>
        /// If the window is closing, reinitialize everything for the next time. For some reason this
        /// only fires when the window closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChampDockableWindow_VisibleChanged(object sender, EventArgs e)
        {
            Init();
        }

        #region panelGPS

        /// <summary>
        /// Sets the output projection for the transform to the one chosen here.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxCoordSys_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ISpatialReference sr = null;
                ISpatialReferenceFactory3 srFactory = new SpatialReferenceEnvironmentClass();
                if (comboBoxCoordSys.SelectedIndex == 4)
                {
                    if (_map != null && _map.SpatialReference != null)
                    {
                        sr = _map.SpatialReference;
                        labelSR.Text = sr.Name;
                    }
                    else
                        labelSR.Text = "";
                    buttonSR.Enabled = true;
                    _transform.SetGeoTransform(null);
                }
                else
                {
                    if (comboBoxCoordSys.SelectedIndex == 0)
                    {
                        sr = srFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
                        _transform.SetGeoTransform(null);
                    }
                    else
                    {
                        if (comboBoxCoordSys.SelectedIndex == 1)
                            sr = srFactory.CreateProjectedCoordinateSystem((int)esriSRProjCSType.esriSRProjCS_NAD1983UTM_10N);
                        else if (comboBoxCoordSys.SelectedIndex == 2)
                            sr = srFactory.CreateProjectedCoordinateSystem((int)esriSRProjCSType.esriSRProjCS_NAD1983UTM_11N);
                        else if (comboBoxCoordSys.SelectedIndex == 3)
                            sr = srFactory.CreateProjectedCoordinateSystem((int)esriSRProjCSType.esriSRProjCS_NAD1983UTM_12N);
                        IGeoTransformation trans = null;
                        trans = (IGeoTransformation)srFactory.CreateGeoTransformation(
                                (int)esriSRGeoTransformationType.esriSRGeoTransformation_NAD1983_To_WGS1984_1);
                        _transform.SetGeoTransform(trans);
                    }
                    labelSR.Text = "";
                    buttonSR.Enabled = false;
                }
                _transform.SetSpatialReference(sr);
                if ((_map.SpatialReference == null) || !((IClone)_map.SpatialReference).IsEqual((IClone)sr))
                {
                    _map.SpatialReference = sr;
                    ArcMap.Document.ActiveView.Refresh();
                }
                FillDatumTransform();
                EnableSelectInputs();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Allows the user to choose a custom output projection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSR_Click(object sender, EventArgs e)
        {
            ISpatialReferenceDialog2 dlg = new SpatialReferenceDialogClass();
            ISpatialReference sr = dlg.DoModalCreate(false, false, false, 0);
            labelSR.Text = sr.Name;
            _transform.SetSpatialReference(sr);
            if (sr != null)
            {
                _map.SpatialReference = sr;
                ArcMap.Document.ActiveView.Refresh();
            }
            FillDatumTransform();
            EnableSelectInputs();
        }

        /// <summary>
        /// Sets the output datum transformation to the one chosen here.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxGeoTrans_SelectedIndexChanged(object sender, EventArgs e)
        {
            _transform.SetGeoTransform((IGeoTransformation)comboBoxGeoTrans.SelectedItem);
        }

        /// <summary>
        /// Checks to see if the text is a valid coordinate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxLon1_TextChanged(object sender, EventArgs e)
        {
            _bm1LongOK = CheckCoords(((TextBox)sender).Text);
            EnableSelectInputs();
        }

        /// <summary>
        /// Checks to see if the text is a valid coordinate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxLat1_TextChanged(object sender, EventArgs e)
        {
            _bm1LatOK = CheckCoords(((TextBox)sender).Text);
            EnableSelectInputs();
        }

        /// <summary>
        /// Checks to see if the text is a valid coordinate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxLon2_TextChanged(object sender, EventArgs e)
        {
            _bm2LongOK = CheckCoords(((TextBox)sender).Text);
            EnableSelectInputs();
        }

        /// <summary>
        /// Checks to see if the text is a valid coordinate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxLat2_TextChanged(object sender, EventArgs e)
        {
            _bm2LatOK = CheckCoords(((TextBox)sender).Text);
            EnableSelectInputs();
        }

        /// <summary>
        /// Checks to see if the text is a valid coordinate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxLon3_TextChanged(object sender, EventArgs e)
        {
            _bm3LongOK = CheckCoords(((TextBox)sender).Text);
            EnableSelectInputs();
        }

        /// <summary>
        /// Checks to see if the text is a valid coordinate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxLat3_TextChanged(object sender, EventArgs e)
        {
            _bm3LatOK = CheckCoords(((TextBox)sender).Text);
            EnableSelectInputs();
        }

        /// <summary>
        /// Checks to see if the text is a valid elevation value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxElev1_TextChanged(object sender, EventArgs e)
        {
            _bm1ElevOK = CheckElevation(((TextBox)sender).Text);
            EnableSelectInputs();
        }

        /// <summary>
        /// Checks to see if the text is a valid elevation value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxElev2_TextChanged(object sender, EventArgs e)
        {
            _bm2ElevOK = CheckElevation(((TextBox)sender).Text);
            EnableSelectInputs();
        }

        /// <summary>
        /// Checks to see if the text is a valid elevation value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxElev3_TextChanged(object sender, EventArgs e)
        {
            _bm3ElevOK = CheckElevation(((TextBox)sender).Text);
            EnableSelectInputs();
        }

        /// <summary>
        /// Rechecks the benchmark coordinates assuming either projected or geographic coordinates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButtonCoordType_CheckedChanged(object sender, EventArgs e)
        {
            CheckAllCoords();
            EnableSelectInputs();
        }

        /// <summary>
        /// Shows the sample benchmarks CSV.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabelSampleCsv_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            panelSampleCsv.Visible = true;
        }

        /// <summary>
        /// Hides the sample benchmarks CSV.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCsvOk_Click(object sender, EventArgs e)
        {
            panelSampleCsv.Visible = false;
        }

        /// <summary>
        /// Shows the sample bearings CSV.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabelSampleBearings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            panelSampleBearings.Visible = true;
        }

        /// <summary>
        /// Hides the sample bearings CSV.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonBearingsCsvOK_Click(object sender, EventArgs e)
        {
            panelSampleBearings.Visible = false;
        }

        /// <summary>
        /// Loads coordinate information from a file. The file should look like this:
        /// bm,northing,easting,elevation
        /// with the first row being a header row, and bm being 1,2,or 3 for each row.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLoad_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Title = "Select Benchmark File";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                    {
                        string index;
                        string[] data;
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            data = line.Split(',');
                            index = data[0].Trim();
                            if (index == "1")
                            {
                                textBoxLon1.Text = data[2].Trim();
                                textBoxLat1.Text = data[1].Trim();
                                textBoxElev1.Text = data[3].Trim();
                            }
                            else if (index == "2")
                            {
                                textBoxLon2.Text = data[2].Trim();
                                textBoxLat2.Text = data[1].Trim();
                                textBoxElev2.Text = data[3].Trim();
                            }
                            else if (index == "3")
                            {
                                textBoxLon3.Text = data[2].Trim();
                                textBoxLat3.Text = data[1].Trim();
                                textBoxElev3.Text = data[3].Trim();
                            }
                        }
                    }
                    CheckAllCoords();
                    EnableSelectInputs();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Loads bearings from a file. The file should look like this:
        /// bearing_id,bearing
        /// with the first row being a header row, and bearing_id being 1_2, 1_3, or 2_3 for each row.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLoadBearings_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Title = "Select Bearings File";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                    {
                        string index;
                        string[] data;
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            data = line.Split(',');
                            index = data[0].Trim();
                            if ((index == "1_2") && GeoUtilities.CheckBearing(data[1]))
                                textBoxBearing12.Text = data[1].Trim();
                            else if ((index == "1_3") && GeoUtilities.CheckBearing(data[1]))
                                textBoxBearing13.Text = data[1].Trim();
                            else if ((index == "2_3") && GeoUtilities.CheckBearing(data[1]))
                                textBoxBearing23.Text = data[1].Trim();
                        }
                    }
                    EnableSelectInputs();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Checks to see if the text is a valid bearing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxBearing12_TextChanged(object sender, EventArgs e)
        {
            _bearing12OK = CheckBearing(((TextBox)sender).Text);
            EnableSelectInputs();
        }

        /// <summary>
        /// Checks to see if the text is a valid bearing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxBearing13_TextChanged(object sender, EventArgs e)
        {
            _bearing13OK = CheckBearing(((TextBox)sender).Text);
            EnableSelectInputs();
        }

        /// <summary>
        /// Checks to see if the text is a valid bearing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxBearing23_TextChanged(object sender, EventArgs e)
        {
            _bearing23OK = CheckBearing(((TextBox)sender).Text);
            EnableSelectInputs();
        }

        /// <summary>
        /// Opens a dialog so the user can select a workspace.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOutWS_Click(object sender, EventArgs e)
        {
            try
            {
                IGxObjectFilter wsFilter = new GxFilterWorkspacesClass();
                IGxDialog dlg = new GxDialogClass();
                IGxObjectFilterCollection filters = (IGxObjectFilterCollection)dlg;
                filters.AddFilter(wsFilter, false);
                dlg.Title = "Select File Geodatabase or Shapefile Folder";
                dlg.ButtonCaption = "Select";
                IEnumGxObject objects = null;
                if (dlg.DoModalOpen(0, out objects))
                {
                    IGxObject obj = objects.Next();
                    IWorkspaceFactory2 workspaceFactory;
                    if (obj.Category == "File Geodatabase")
                    {
                        workspaceFactory = new FileGDBWorkspaceFactoryClass();
                    }
                    else if (obj.Category == "Folder")
                    {
                        workspaceFactory = new ShapefileWorkspaceFactoryClass();
                    }
                    else
                    {
                        ShowError("Not a file geodatabase or shapefile folder.");
                        return;
                    }
                    IFeatureWorkspace workspace = (IFeatureWorkspace)workspaceFactory.OpenFromFile(obj.FullName, 0);
                    _transform.SetWorkspace(workspace);
                    textBoxOutWS.Text = obj.BaseName;
                    _workspaceOK = true;

                    List<IDatasetName> allNames = _transform.GetFeatureClassNames();
                    listViewData.Items.Clear();
                    for (int i = 0; i < allNames.Count; i++)
                        AddItemToListViewData(allNames[i]);
                    FillControlComboBox();
                    SelectComboBoxItem(comboBoxControlPts, _transform.GetWorkspacePath(),
                        "Control_Points_Unprojected");
                }
                EnableSelectInputs();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Sets GPS benchmarks and compass bearings on the transform object and goes to the next
        /// screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSelectInputs_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioButtonProj.Checked && comboBoxCoordSys.SelectedIndex == 0)
                {
                    ShowError("If benchmarks are in projected coordinates, then the output" +
                        "coordinate system must also be projected.");
                    return;
                }
                else if (radioButtonProj.Checked)
                {
                    if (CheckAllCoords(false) && !ShowYesNoQuestion("These benchmark coordinates " +
                        "look unprojected. Are they really projected?"))
                    {
                        return;
                    }
                    _transform.SetBenchmarkProj(1, double.Parse(textBoxLon1.Text), 
                        double.Parse(textBoxLat1.Text), double.Parse(textBoxElev1.Text));
                    _transform.SetBenchmarkProj(2, double.Parse(textBoxLon2.Text),
                        double.Parse(textBoxLat2.Text), double.Parse(textBoxElev2.Text));
                    _transform.SetBenchmarkProj(3, double.Parse(textBoxLon3.Text),
                        double.Parse(textBoxLat3.Text), double.Parse(textBoxElev3.Text));
                }
                else
                {
                    if (!CheckAllCoords(false))
                    {
                        ShowError("These benchmark coordinates are not geographic.");
                        return;
                    }
                    _transform.SetBenchmarkGeo(1, textBoxLon1.Text, textBoxLat1.Text,
                        double.Parse(textBoxElev1.Text));
                    _transform.SetBenchmarkGeo(2, textBoxLon2.Text, textBoxLat2.Text,
                        double.Parse(textBoxElev2.Text));
                    _transform.SetBenchmarkGeo(3, textBoxLon3.Text, textBoxLat3.Text,
                        double.Parse(textBoxElev3.Text));
                }
                if (!string.IsNullOrEmpty(textBoxBearing12.Text))
                    _transform.SetCompassBearing(1, 2, double.Parse(textBoxBearing12.Text));
                if (!string.IsNullOrEmpty(textBoxBearing13.Text))
                    _transform.SetCompassBearing(1, 3, double.Parse(textBoxBearing13.Text));
                if (!string.IsNullOrEmpty(textBoxBearing23.Text))
                    _transform.SetCompassBearing(2, 3, double.Parse(textBoxBearing23.Text));
                _transform.SetMap(ArcMap.Document.FocusMap);
                IEnvelope extent = _transform.GetBenchmark(1).Envelope;
                extent.Expand(1, 1, false);
                ArcMap.Document.ActiveView.Extent = extent;
                ArcMap.Document.FocusMap.MapScale = 24000;
                DeleteGraphicsLayers();
                ArcMap.Document.ActiveView.Refresh();
                _viewedTransforms.Clear();
                panelGPS.Visible = false;
                panelInput.Visible = true;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        #endregion panelGPS


        #region panelInput

        /// <summary>
        /// Adds the control points file to the transform object, fills the attribute field
        /// combobox, and changes the color of the file in the data list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxControlPts_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                comboBoxAttField.Text = "";
                comboBoxAttField.Items.Clear();
                if (comboBoxControlPts.SelectedIndex == -1)
                {
                    _transform.SetControlPoints(null);
                    _controlPtsOK = false;
                    groupBoxBMs.Enabled = false;
                    foreach (ListViewItem item in listViewData.Items)
                        item.SubItems[2].ForeColor = Color.Blue;
                }
                else
                {
                    IDatasetName controlPtsName = (IDatasetName)comboBoxControlPts.SelectedItem;
                    _transform.SetControlPoints((IFeatureClassName)controlPtsName);
                    foreach (string fieldName in _transform.GetAttFields())
                        comboBoxAttField.Items.Add(fieldName);
                    comboBoxAttField.SelectedIndex = FindAttFieldIndex();
                    _controlPtsOK = true;
                    groupBoxBMs.Enabled = true;
                    foreach (ListViewItem item in listViewData.Items)
                    {
                        if (_nameComparer.Equal(controlPtsName, (IDatasetName)item.Tag))
                            item.SubItems[2].ForeColor = Color.Gray;
                        else
                            item.SubItems[2].ForeColor = Color.Blue;
                    }
                }
                EnableTransform();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Allows the user to select a new feature class and adds it to the data list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                IDatasetName dsName = SelectFeatureClass("Add Data", esriGeometryType.esriGeometryAny);
                if (dsName != null)
                {
                    bool found = false;
                    foreach (ListViewItem item in listViewData.Items)
                    {
                        if (_nameComparer.Equal((IDatasetName)item.Tag, dsName))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        AddItemToListViewData(dsName);
                        FillControlComboBox();
                    }
                    EnableTransform();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Remove a feature class if the user clicked on the remove link.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewData_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                ListViewHitTestInfo info = listViewData.HitTest(e.X, e.Y);
                ListViewItem item = info.Item;
                if (info.SubItem == item.SubItems[2] &&
                    !_nameComparer.Equal((IDatasetName)comboBoxControlPts.SelectedItem, 
                        (IDatasetName)item.Tag))
                {
                    listViewData.Items.Remove(item);
                    FillControlComboBox();
                }
                else
                    listViewData.Select();
                EnableTransform();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }

        }

        /// <summary>
        /// Checks to see if the transform button can be enabled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewData_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            EnableTransform();
        }

        /// <summary>
        /// Fills the attribute value comboboxes with the available attributes from the selected 
        /// field and looks for defaults.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxAttField_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxAttField.SelectedIndex > -1)
                {
                    _transform.SetAttField(comboBoxAttField.SelectedItem.ToString());
                    comboBoxBM1Att.Text = "";
                    comboBoxBM2Att.Text = "";
                    comboBoxBM3Att.Text = "";
                    comboBoxBM1Att.Items.Clear();
                    comboBoxBM2Att.Items.Clear();
                    comboBoxBM3Att.Items.Clear();
                    foreach (string value in _transform.GetAttValues())
                    {
                        comboBoxBM1Att.Items.Add(value);
                        comboBoxBM2Att.Items.Add(value);
                        comboBoxBM3Att.Items.Add(value);
                    }
                    comboBoxBM1Att.SelectedIndex = FindBMAttIndex("1");
                    comboBoxBM2Att.SelectedIndex = FindBMAttIndex("2");
                    comboBoxBM3Att.SelectedIndex = FindBMAttIndex("3");
                    CheckBenchmarkIds();
                    groupBoxBMs.Enabled = true;
                }
                else
                {
                    FormUtilities.ClearControls(groupBoxBMs);
                    FormUtilities.ClearControls(groupBoxRotation);
                    groupBoxBMs.Enabled = false;
                    groupBoxRotation.Enabled = false;
                }
                EnableTransform();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Makes sure that different attributes are chosen for each benchmark.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxBMAtt_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckBenchmarkIds();
            EnableTransform();
        }

        /// <summary>
        /// Enables and disables the bearing radio buttons based on which hinge point is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButtonBM_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                radioButtonBearingGPS1.Enabled = !radioButtonBM1.Checked;
                radioButtonBearingGPS2.Enabled = !radioButtonBM2.Checked;
                radioButtonBearingGPS3.Enabled = !radioButtonBM3.Checked;
                if (radioButtonBM1.Checked)
                {
                    radioButtonBearingCompass1.Enabled = false;
                    radioButtonBearingCompass2.Enabled = _transform.HasCompassBearing("12");
                    radioButtonBearingCompass3.Enabled = _transform.HasCompassBearing("13");
                }
                if (radioButtonBM2.Checked)
                {
                    radioButtonBearingCompass1.Enabled = _transform.HasCompassBearing("21");
                    radioButtonBearingCompass2.Enabled = false;
                    radioButtonBearingCompass3.Enabled = _transform.HasCompassBearing("23");
                }
                if (radioButtonBM3.Checked)
                {
                    radioButtonBearingCompass1.Enabled = _transform.HasCompassBearing("31");
                    radioButtonBearingCompass2.Enabled = _transform.HasCompassBearing("32");
                    radioButtonBearingCompass3.Enabled = false;
                }
                groupBoxRotation.Enabled = true;
                _hingeOK = true;
                RadioButton rb = FormUtilities.GetCheckedRadioButton(groupBoxRotation);
                _bearingOK = rb != null && rb.Enabled;
                EnableTransform();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Checks that the selecting bearing is available given the selected hinge point.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButtonBearing_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Enabled)
                _bearingOK = true;
            EnableTransform();
        }

        /// <summary>
        /// Takes the user back to the GPS benchmarks panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonBackGPS_Click(object sender, EventArgs e)
        {
            panelInput.Visible = false;
            panelGPS.Visible = true;
        }

        /// <summary>
        /// Adds the feataure classes to be transformed, sets the benchmark attribute IDs,
        /// transforms the data into graphics, and fills the error boxes on the next panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTransform_Click(object sender, EventArgs e)
        {
            try
            {
                IDatasetName controlName = (IDatasetName)comboBoxControlPts.SelectedItem;
                _transform.ClearData();
                foreach (ListViewItem item in listViewData.Items)
                {
                    if (!_nameComparer.Equal((IDatasetName)item.Tag, controlName))
                        _transform.AddData((IFeatureClassName)item.Tag);
                }
                _transform.SetTSBenchmark(1, comboBoxBM1Att.SelectedItem.ToString());
                _transform.SetTSBenchmark(2, comboBoxBM2Att.SelectedItem.ToString());
                _transform.SetTSBenchmark(3, comboBoxBM3Att.SelectedItem.ToString());

                string rbName = FormUtilities.GetCheckedRadioButton(groupBoxBMs).Name;
                int defaultHinge = int.Parse(rbName.Substring(rbName.Length - 1));
                rbName = FormUtilities.GetCheckedRadioButton(groupBoxRotation).Name;
                int defaultBearing = int.Parse(rbName.Substring(rbName.Length - 1));
                rbName = string.Format("radioButton{0}{1}{2}", rbName.Contains("GPS") ? "GPS" : "Compass",
                    defaultHinge, defaultBearing);

                IEnvelope extent = new EnvelopeClass();
                foreach (ListViewItem item in listViewData.CheckedItems)
                    extent.Union(_transform.TransformFeatures2Graphics((IFeatureClassName)item.Tag,
                                                                       _graphicsColor));
                ArcMap.Document.ActiveView.Extent = extent;
                ArcMap.Document.FocusMap.MapScale = 10000;
                ArcMap.Document.ActiveView.Refresh();

                ((RadioButton)panelOutput.Controls.Find(rbName, true)[0]).Checked = false; // to force things to update
                ((RadioButton)panelOutput.Controls.Find(rbName, true)[0]).Checked = true;

                string id;
                for (int hingeIndex = 1; hingeIndex <= 3; hingeIndex++)
                {
                    for (int bearingIndex = 1; bearingIndex <= 3; bearingIndex++)
                    {
                        if (bearingIndex != hingeIndex)
                        {
                            for (int benchmarkIndex = 1; benchmarkIndex <= 3; benchmarkIndex++)
                            {
                                if (benchmarkIndex != hingeIndex)
                                {
                                    id = hingeIndex.ToString() + bearingIndex.ToString() +
                                        benchmarkIndex.ToString();
                                    SetError("HGPS" + id);
                                    SetError("ZGPS" + id);
                                    SetError("HCompass" + id);
                                    SetError("ZCompass" + id);
                                }
                            }
                        }
                    }
                }

                panelInput.Visible = false;
                panelOutput.Visible = true;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        #endregion panelInput

        #region panelOutput

        /// <summary>
        /// Makes the annotation layer corresponding to the checked radio button visible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButtonTransformation_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton rb = (RadioButton)sender;
                string name = rb.Name.Substring(11).ToLower();
                ICompositeLayer mainGraphicsLayer = (ICompositeLayer)_map.BasicGraphicsLayer;
                for (int i = 0; i < mainGraphicsLayer.Count; i++)
                {
                    if (mainGraphicsLayer.Layer[i].Name == name)
                    {
                        ((ILayer)mainGraphicsLayer.Layer[i]).Visible = rb.Checked;
                        if (!_viewedTransforms.Contains(name))
                            _viewedTransforms.Add(name);
                        break;
                    }
                }
                ArcMap.Document.ActiveView.Refresh();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Enables the save button based on whether or not the "I've checked stuff" checkboxes are
        /// checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxCheckError_CheckedChanged(object sender, EventArgs e)
        {
            buttonSave.Enabled = checkBoxVisual.Checked && checkBoxErrors.Checked;
        }

        /// <summary>
        /// Takes the user back to the Inputs panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonBackInputs_Click(object sender, EventArgs e)
        {
            panelOutput.Visible = false;
            panelInput.Visible = true;
        }

        // Close the dockable window.
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            ChampExtension.GetExtension().GetDockableWindow().Show(false);
        }

        /// <summary>
        /// Saves the transformed data corresponding to the checked radio button to a permanent file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                string rbName = FormUtilities.GetCheckedRadioButton(panelOutput).Name;
                int hingeIndex = int.Parse(rbName.Substring(rbName.Length - 2, 1));
                int bearingIndex = int.Parse(rbName.Substring(rbName.Length - 1));
                if (rbName.Contains("GPS"))
                {
                    _transform.SaveData(hingeIndex, bearingIndex, true);
                    _transform.CreateReportTable(hingeIndex, hingeIndex, bearingIndex, true,
                        _viewedTransforms);
                }
                else if (rbName.Contains("Compass"))
                {
                    _transform.SaveData(hingeIndex, bearingIndex, false);
                    _transform.CreateReportTable(hingeIndex, hingeIndex, bearingIndex, false,
                        _viewedTransforms);
                }
                if (_transform.GetFullErrorMessage() != null)
                    ShowError(_transform.GetFullErrorMessage());
                ChampExtension.GetExtension().GetDockableWindow().Show(false);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        #endregion panelOutput

 
    }

}
