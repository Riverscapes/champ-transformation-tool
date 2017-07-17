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

namespace CHAMP
{
    public class ChampExtension : ESRI.ArcGIS.Desktop.AddIns.Extension
    {

        private static ChampExtension s_extension;
        private ESRI.ArcGIS.Framework.IDockableWindow m_dockWindow;
        private ESRI.ArcGIS.Framework.IDockableWindow m_dockWindowInfo;
        private ESRI.ArcGIS.Framework.IDockableWindow m_dockWindowApply;

        public ChampExtension()
        {
            s_extension = this;
        }

        protected override void OnStartup()
        {
            WireDocumentEvents();
        }

        private void WireDocumentEvents()
        {
            ArcMap.Events.CloseDocument += delegate()
            {
                if (m_dockWindow != null)
                    m_dockWindow.Show(false);
                if (m_dockWindowInfo != null)
                    m_dockWindowInfo.Show(false);
                if (m_dockWindowApply != null)
                    m_dockWindowApply.Show(false);
            };
        }

        internal static ChampExtension GetExtension()
        {
            if (s_extension == null)
            {
                ESRI.ArcGIS.esriSystem.UID extID = new ESRI.ArcGIS.esriSystem.UIDClass();
                extID.Value = ThisAddIn.IDs.ChampExtension;
                ArcMap.Application.FindExtensionByCLSID(extID);
            }
            return s_extension;
        }

        internal ESRI.ArcGIS.Framework.IDockableWindow GetDockableWindow()
        {
            if (m_dockWindow == null)
            {
                ESRI.ArcGIS.esriSystem.UID dockUID = new ESRI.ArcGIS.esriSystem.UIDClass();
                dockUID.Value = ThisAddIn.IDs.ChampDockableWindow;
                m_dockWindow = ArcMap.DockableWindowManager.GetDockableWindow(dockUID);
            }
            return m_dockWindow;
        }

        internal ESRI.ArcGIS.Framework.IDockableWindow GetInfoDockableWindow()
        {
            if (m_dockWindowInfo == null)
            {
                ESRI.ArcGIS.esriSystem.UID dockUID = new ESRI.ArcGIS.esriSystem.UIDClass();
                dockUID.Value = ThisAddIn.IDs.ChampInfoDockableWindow;
                m_dockWindowInfo = ArcMap.DockableWindowManager.GetDockableWindow(dockUID);
            }
            return m_dockWindowInfo;
        }

        internal ESRI.ArcGIS.Framework.IDockableWindow GetApplyDockableWindow()
        {
            if (m_dockWindowApply == null)
            {
                ESRI.ArcGIS.esriSystem.UID dockUID = new ESRI.ArcGIS.esriSystem.UIDClass();
                dockUID.Value = ThisAddIn.IDs.ChampApplyDockableWindow;
                m_dockWindowApply = ArcMap.DockableWindowManager.GetDockableWindow(dockUID);
            }
            return m_dockWindowApply;
        }

    }
}
