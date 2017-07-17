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
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RSGIS.Utilites;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CHAMP
{
    /// <summary>
    /// Designer class of the dockable window add-in. It contains user interfaces that
    /// make up the dockable window.
    /// </summary>
    public partial class ChampApplyDockableWindow : UserControl
    {

        #region Esri generated code

        public ChampApplyDockableWindow(object hook)
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
            private ChampApplyDockableWindow m_windowUI;

            public AddinImpl()
            {
            }

            protected override IntPtr OnCreateChild()
            {
                m_windowUI = new ChampApplyDockableWindow(this.Hook);
                return m_windowUI.Handle;
            }

            protected override void Dispose(bool disposing)
            {
                if (m_windowUI != null)
                    m_windowUI.Dispose(disposing);

                base.Dispose(disposing);
            }

        }

        #endregion

        private ChampTransformClass _transform; // object that does all the real work
        private IFeatureDatasetName2 _inputDatasetName; // dataset with files to be transformed
        private IName _benchmarkName; // previously transformed benchmarks
        private IName _transformTableName; // previous transform table
        private DatasetNameComparer _nameComparer; // used to compare DatasetName objects


        private void EnableSave()
        {
            saveButton.Enabled = (benchmark1ComboBox.SelectedIndex != -1) &&
                (benchmark2ComboBox.SelectedIndex != -1) && (benchmark3ComboBox.SelectedIndex != -1)
                && (_benchmarkName != null) && (_transformTableName != null) &&
                (_transform.GetWorkspacePath() != "");

        }

        /// <summary>
        /// Clears everything.
        /// </summary>
        private void Init()
        {
            _transform = new ChampTransformClass();
            _transform.SetMap(ArcMap.Document.FocusMap);
            _benchmarkName = null;
            _transformTableName = null;
            inputDatasetTextBox.Text = "";
            controlPointsComboBox.DataSource = null;
            controlPointsComboBox.Text = "";
            attributeFieldComboBox.Items.Clear();
            attributeFieldComboBox.Text = "";
            benchmark1ComboBox.Items.Clear();
            benchmark1ComboBox.Text = "";
            benchmark2ComboBox.Items.Clear();
            benchmark2ComboBox.Text = "";
            benchmark3ComboBox.Items.Clear();
            benchmark3ComboBox.Text = "";
            benchmarkTextBox.Text = "";
            transformTextBox.Text = "";
            outputWorkspaceTextBox.Text = "";
        }

        /// <summary>
        /// Tries to find an attribute value that matches the given benchmark number.
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="n"></param>
        private void SetBenchmarkAttributeIndex(ComboBox cb, string n)
        {
            cb.SelectedIndex = -1;
            for (int i = 0; i < cb.Items.Count; i++)
            {
                string att = cb.Items[i].ToString().ToLower();
                if (att == "bm" + n || att == "bm_" + n || att == "cp" + n || att == "cp_" + n)
                {
                    cb.SelectedIndex = i;
                    break;
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
        /// Initializes things that only need to be set once, no matter what.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChampApplyDockableWindow_Load(object sender, EventArgs e)
        {
            _nameComparer = new DatasetNameComparer();
            Init();
        }

        /// <summary>
        /// Opens a dialog so the user can select a dataset containing the new data to be transformed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inputDatasetButton_Click(object sender, EventArgs e)
        {
            try
            {
                IGxObjectFilter datasetFilter = new GxFilterFeatureDatasetsClass();
                IGxDialog dlg = new GxDialogClass();
                IGxObjectFilterCollection filters = (IGxObjectFilterCollection)dlg;
                filters.AddFilter(datasetFilter, true);
                dlg.Title = "Select the feature dataset containing the data to be transformed";
                dlg.ButtonCaption = "Select";
                IEnumGxObject objects = null;
                if (dlg.DoModalOpen(0, out objects))
                {
                    IGxObject obj = objects.Next();
                    _inputDatasetName= (IFeatureDatasetName2)obj.InternalObjectName;
                    inputDatasetTextBox.Text = obj.Parent.Name + "/" + obj.Name;
                    List<IDatasetName> fcNames =
                        EsriUtilities.GetFeatureClassNames(esriGeometryType.esriGeometryPoint, 
                        _inputDatasetName);
                    fcNames.Sort(_nameComparer);
                    controlPointsComboBox.DataSource = fcNames;
                    controlPointsComboBox.DisplayMember = "Name";
                    EnableSave();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Fills the list of attribute fields when the control points are selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void controlPointsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                attributeFieldComboBox.Text = "";
                attributeFieldComboBox.Items.Clear();
                if (controlPointsComboBox.SelectedIndex != -1)
                {
                    _transform.SetControlPoints((IFeatureClassName)controlPointsComboBox.SelectedItem);
                    foreach (string fieldName in _transform.GetAttFields())
                        attributeFieldComboBox.Items.Add(fieldName);
                    for (int i = 0; i < attributeFieldComboBox.Items.Count; i++)
                    {
                        if (attributeFieldComboBox.Items[i].ToString().ToUpper() == "DESCRIPTION")
                        {
                            attributeFieldComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    _transform.SetControlPoints(null);
                }
                EnableSave();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Fills the attribute value combo boxes when the attribute field is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void attributeFieldComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                List<ComboBox> benchmarkComboBoxes =
                    new List<ComboBox> {benchmark1ComboBox, benchmark2ComboBox, benchmark3ComboBox};
                foreach (ComboBox cb in benchmarkComboBoxes)
                {
                    cb.Text = "";
                    cb.Items.Clear();
                }
                if (attributeFieldComboBox.SelectedIndex != -1)
                {
                    _transform.SetAttField(attributeFieldComboBox.SelectedItem.ToString());
                    foreach (string value in _transform.GetAttValues())
                    {
                        foreach (ComboBox cb in benchmarkComboBoxes)
                            cb.Items.Add(value);
                    }
                    for (int i = 1; i <= benchmarkComboBoxes.Count; i++)
                        SetBenchmarkAttributeIndex(benchmarkComboBoxes[i - 1], i.ToString());
                }
                else
                {
                    _transform.SetAttField(null);
                }
                EnableSave();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Opens a dialog so the user can select a feature class containing previously transformed
        /// benchmarks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void benchmarkButton_Click(object sender, EventArgs e)
        {
            try
            {
                IGxObjectFilter featureClassFilter = new GxFilterFeatureClassesClass();
                IGxDialog dlg = new GxDialogClass();
                IGxObjectFilterCollection filters = (IGxObjectFilterCollection)dlg;
                filters.AddFilter(featureClassFilter, true);
                dlg.Title = "Select the feature class containing previously transformed benchmarks";
                dlg.ButtonCaption = "Select";
                IEnumGxObject objects = null;
                if (dlg.DoModalOpen(0, out objects))
                {
                    IGxObject obj = objects.Next();
                    _benchmarkName = obj.InternalObjectName;
                    benchmarkTextBox.Text = obj.Name;
                    EnableSave();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Opens a dialog so the user can select a table containing transformation information
        /// from a previous time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void transformButton_Click(object sender, EventArgs e)
        {
            try
            {
                IGxObjectFilter tableFilter = new GxFilterTablesClass();
                IGxDialog dlg = new GxDialogClass();
                IGxObjectFilterCollection filters = (IGxObjectFilterCollection)dlg;
                filters.AddFilter(tableFilter, true);
                dlg.Title = "Select the table containing previous transform information";
                dlg.ButtonCaption = "Select";
                IEnumGxObject objects = null;
                if (dlg.DoModalOpen(0, out objects))
                {
                    IGxObject obj = objects.Next();
                    _transformTableName = obj.InternalObjectName;
                    transformTextBox.Text = obj.Name;
                    EnableSave();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Opens a dialog so the user can select an output workspace.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void outputWorkspaceButton_Click(object sender, EventArgs e)
        {
            try
            {
                IGxObjectFilter geodatabaseFilter = new GxFilterFileGeodatabasesClass();
                IGxDialog dlg = new GxDialogClass();
                IGxObjectFilterCollection filters = (IGxObjectFilterCollection)dlg;
                filters.AddFilter(geodatabaseFilter, false);
                dlg.Title = "Select the file geodatabase for the output";
                dlg.ButtonCaption = "Select";
                IEnumGxObject objects = null;
                if (dlg.DoModalOpen(0, out objects))
                {
                    IGxObject obj = objects.Next();
                    IWorkspaceFactory2 workspaceFactory = new FileGDBWorkspaceFactoryClass();
                    IFeatureWorkspace workspace = (IFeatureWorkspace)workspaceFactory.OpenFromFile(obj.FullName, 0);
                    _transform.SetWorkspace(workspace);
                    outputWorkspaceTextBox.Text = obj.Name;
                    EnableSave();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Cancels the dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            Init();
            ChampExtension.GetExtension().GetApplyDockableWindow().Show(false);
        }

        /// <summary>
        /// Transforms and saves the data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Get spatial reference
                IFeatureClass benchmarks = (IFeatureClass)_benchmarkName.Open();
                _transform.SetSpatialReference(((IGeoDataset)benchmarks).SpatialReference);

                // Set benchmarks
                 for (int i = 1; i < 4; i++)
                {
                     IFeature feature = benchmarks.GetFeature(i);
                     _transform.SetBenchmarkPoint(i, (IPoint)feature.ShapeCopy);
                }

                // Set total station benchmarks
                _transform.SetTSBenchmark(1, benchmark1ComboBox.Text);
                _transform.SetTSBenchmark(2, benchmark2ComboBox.Text);
                _transform.SetTSBenchmark(3, benchmark3ComboBox.Text);

                // Add data to be transformed
                foreach (IDatasetName dsName in EsriUtilities.GetFeatureClassNames(esriGeometryType.esriGeometryAny, _inputDatasetName))
                    _transform.AddData((IFeatureClassName)dsName);

                // Transform data
                IQueryFilter query = new QueryFilterClass();
                query.WhereClause = "\"Chosen\" = 1";
                ITable table = (ITable)_transformTableName.Open();
                ICursor cursor = table.Search(query, true);
                IRow row = cursor.NextRow();
                int hinge = Convert.ToInt32(((string)row.get_Value(table.FindField("Hinge"))).Split(' ')[1]);
                int toBM = Convert.ToInt32(((string)row.get_Value(table.FindField("Bearing"))).Split(' ')[2]);
                _transform.SaveData(hinge, toBM);

                // Close dialog
                Init();
                ChampExtension.GetExtension().GetApplyDockableWindow().Show(false);
                ArcMap.Document.ActiveView.Refresh();

            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

 

 
    }
}
