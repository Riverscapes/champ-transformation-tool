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

using System.Windows.Forms;

namespace RSGIS.Utilites
{

    /// <summary>
    /// Class with static methods to do simple but common stuff with form controls.
    /// </summary>
    class FormUtilities
    {

        /// <summary>
        /// Recursively clears all child controls in a parent. This means clearing text from
        /// textboxes, removing subitems from comboboxes and listviews, and unchecking radiobuttons
        /// and checkbuttons.
        /// </summary>
        /// <param name="parent">control to clear</param>
        public static void ClearControls(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is TextBox)
                    child.Text = "";
                else if (child is RadioButton)
                    ((RadioButton)child).Checked = false;
                else if (child is ComboBox)
                    ((ComboBox)child).Items.Clear();
                else if (child is CheckBox)
                    ((CheckBox)child).Checked = false;
                else if (child is ListView)
                    ((ListView)child).Items.Clear();
                else
                    ClearControls(child);
            }
        }

        /// <summary>
        /// Returns the checked radio button in the parent control.
        /// </summary>
        /// <param name="parent">parent control to look in</param>
        /// <returns>checked radio button or null if there isn't one</returns>
        public static RadioButton GetCheckedRadioButton(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is RadioButton && ((RadioButton)ctrl).Checked)
                    return (RadioButton)ctrl;
            }
            return null;
        }

    }
}
