namespace CHAMP
{
    partial class ChampApplyDockableWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.inputDatasetTextBox = new System.Windows.Forms.TextBox();
            this.inputDatasetButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.outputWorkspaceButton = new System.Windows.Forms.Button();
            this.outputWorkspaceTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.controlPointsComboBox = new System.Windows.Forms.ComboBox();
            this.attributeFieldComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.benchmark3ComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.benchmark2ComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.benchmark1ComboBox = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.inputGroupBox1 = new System.Windows.Forms.GroupBox();
            this.transformDataGroupBox = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.benchmarkTextBox = new System.Windows.Forms.TextBox();
            this.benchmarkButton = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.transformTextBox = new System.Windows.Forms.TextBox();
            this.transformButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.cancelButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.inputGroupBox1.SuspendLayout();
            this.transformDataGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input Dataset";
            // 
            // inputDatasetTextBox
            // 
            this.inputDatasetTextBox.Enabled = false;
            this.inputDatasetTextBox.Location = new System.Drawing.Point(109, 24);
            this.inputDatasetTextBox.Name = "inputDatasetTextBox";
            this.inputDatasetTextBox.Size = new System.Drawing.Size(203, 20);
            this.inputDatasetTextBox.TabIndex = 1;
            // 
            // inputDatasetButton
            // 
            this.inputDatasetButton.Image = global::CHAMP.Properties.Resources.openfile_24;
            this.inputDatasetButton.Location = new System.Drawing.Point(318, 18);
            this.inputDatasetButton.Name = "inputDatasetButton";
            this.inputDatasetButton.Size = new System.Drawing.Size(30, 30);
            this.inputDatasetButton.TabIndex = 2;
            this.inputDatasetButton.UseVisualStyleBackColor = true;
            this.inputDatasetButton.Click += new System.EventHandler(this.inputDatasetButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 347);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Output Workspace";
            // 
            // outputWorkspaceButton
            // 
            this.outputWorkspaceButton.Image = global::CHAMP.Properties.Resources.openfile_24;
            this.outputWorkspaceButton.Location = new System.Drawing.Point(321, 338);
            this.outputWorkspaceButton.Name = "outputWorkspaceButton";
            this.outputWorkspaceButton.Size = new System.Drawing.Size(30, 30);
            this.outputWorkspaceButton.TabIndex = 5;
            this.outputWorkspaceButton.UseVisualStyleBackColor = true;
            this.outputWorkspaceButton.Click += new System.EventHandler(this.outputWorkspaceButton_Click);
            // 
            // outputWorkspaceTextBox
            // 
            this.outputWorkspaceTextBox.Enabled = false;
            this.outputWorkspaceTextBox.Location = new System.Drawing.Point(112, 344);
            this.outputWorkspaceTextBox.Name = "outputWorkspaceTextBox";
            this.outputWorkspaceTextBox.Size = new System.Drawing.Size(203, 20);
            this.outputWorkspaceTextBox.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Control Points";
            // 
            // controlPointsComboBox
            // 
            this.controlPointsComboBox.FormattingEnabled = true;
            this.controlPointsComboBox.Location = new System.Drawing.Point(109, 60);
            this.controlPointsComboBox.Name = "controlPointsComboBox";
            this.controlPointsComboBox.Size = new System.Drawing.Size(203, 21);
            this.controlPointsComboBox.TabIndex = 7;
            this.controlPointsComboBox.SelectedIndexChanged += new System.EventHandler(this.controlPointsComboBox_SelectedIndexChanged);
            // 
            // attributeFieldComboBox
            // 
            this.attributeFieldComboBox.FormattingEnabled = true;
            this.attributeFieldComboBox.Location = new System.Drawing.Point(109, 96);
            this.attributeFieldComboBox.Name = "attributeFieldComboBox";
            this.attributeFieldComboBox.Size = new System.Drawing.Size(203, 21);
            this.attributeFieldComboBox.Sorted = true;
            this.attributeFieldComboBox.TabIndex = 10;
            this.attributeFieldComboBox.SelectedIndexChanged += new System.EventHandler(this.attributeFieldComboBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Attribute Field";
            // 
            // benchmark3ComboBox
            // 
            this.benchmark3ComboBox.FormattingEnabled = true;
            this.benchmark3ComboBox.Location = new System.Drawing.Point(109, 186);
            this.benchmark3ComboBox.Name = "benchmark3ComboBox";
            this.benchmark3ComboBox.Size = new System.Drawing.Size(203, 21);
            this.benchmark3ComboBox.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 189);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Benchmark 3";
            // 
            // benchmark2ComboBox
            // 
            this.benchmark2ComboBox.FormattingEnabled = true;
            this.benchmark2ComboBox.Location = new System.Drawing.Point(109, 159);
            this.benchmark2ComboBox.Name = "benchmark2ComboBox";
            this.benchmark2ComboBox.Size = new System.Drawing.Size(203, 21);
            this.benchmark2ComboBox.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 162);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Benchmark 2";
            // 
            // benchmark1ComboBox
            // 
            this.benchmark1ComboBox.FormattingEnabled = true;
            this.benchmark1ComboBox.Location = new System.Drawing.Point(109, 132);
            this.benchmark1ComboBox.Name = "benchmark1ComboBox";
            this.benchmark1ComboBox.Size = new System.Drawing.Size(203, 21);
            this.benchmark1ComboBox.TabIndex = 16;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 135);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(70, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Benchmark 1";
            // 
            // inputGroupBox1
            // 
            this.inputGroupBox1.Controls.Add(this.label1);
            this.inputGroupBox1.Controls.Add(this.benchmark1ComboBox);
            this.inputGroupBox1.Controls.Add(this.inputDatasetTextBox);
            this.inputGroupBox1.Controls.Add(this.label7);
            this.inputGroupBox1.Controls.Add(this.inputDatasetButton);
            this.inputGroupBox1.Controls.Add(this.benchmark2ComboBox);
            this.inputGroupBox1.Controls.Add(this.label3);
            this.inputGroupBox1.Controls.Add(this.label6);
            this.inputGroupBox1.Controls.Add(this.controlPointsComboBox);
            this.inputGroupBox1.Controls.Add(this.benchmark3ComboBox);
            this.inputGroupBox1.Controls.Add(this.label5);
            this.inputGroupBox1.Controls.Add(this.label4);
            this.inputGroupBox1.Controls.Add(this.attributeFieldComboBox);
            this.inputGroupBox1.Location = new System.Drawing.Point(3, 3);
            this.inputGroupBox1.Name = "inputGroupBox1";
            this.inputGroupBox1.Size = new System.Drawing.Size(359, 220);
            this.inputGroupBox1.TabIndex = 17;
            this.inputGroupBox1.TabStop = false;
            this.inputGroupBox1.Text = "New Input Data";
            // 
            // transformDataGroupBox
            // 
            this.transformDataGroupBox.Controls.Add(this.label9);
            this.transformDataGroupBox.Controls.Add(this.transformTextBox);
            this.transformDataGroupBox.Controls.Add(this.transformButton);
            this.transformDataGroupBox.Controls.Add(this.label8);
            this.transformDataGroupBox.Controls.Add(this.benchmarkTextBox);
            this.transformDataGroupBox.Controls.Add(this.benchmarkButton);
            this.transformDataGroupBox.Location = new System.Drawing.Point(3, 229);
            this.transformDataGroupBox.Name = "transformDataGroupBox";
            this.transformDataGroupBox.Size = new System.Drawing.Size(359, 98);
            this.transformDataGroupBox.TabIndex = 18;
            this.transformDataGroupBox.TabStop = false;
            this.transformDataGroupBox.Text = "Transform Information";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 27);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(93, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "Benchmark Points";
            // 
            // benchmarkTextBox
            // 
            this.benchmarkTextBox.Enabled = false;
            this.benchmarkTextBox.Location = new System.Drawing.Point(109, 24);
            this.benchmarkTextBox.Name = "benchmarkTextBox";
            this.benchmarkTextBox.Size = new System.Drawing.Size(203, 20);
            this.benchmarkTextBox.TabIndex = 18;
            // 
            // benchmarkButton
            // 
            this.benchmarkButton.Image = global::CHAMP.Properties.Resources.openfile_24;
            this.benchmarkButton.Location = new System.Drawing.Point(318, 18);
            this.benchmarkButton.Name = "benchmarkButton";
            this.benchmarkButton.Size = new System.Drawing.Size(30, 30);
            this.benchmarkButton.TabIndex = 19;
            this.benchmarkButton.UseVisualStyleBackColor = true;
            this.benchmarkButton.Click += new System.EventHandler(this.benchmarkButton_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 63);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(84, 13);
            this.label9.TabIndex = 20;
            this.label9.Text = "Transform Table";
            // 
            // transformTextBox
            // 
            this.transformTextBox.Enabled = false;
            this.transformTextBox.Location = new System.Drawing.Point(109, 60);
            this.transformTextBox.Name = "transformTextBox";
            this.transformTextBox.Size = new System.Drawing.Size(203, 20);
            this.transformTextBox.TabIndex = 21;
            // 
            // transformButton
            // 
            this.transformButton.Image = global::CHAMP.Properties.Resources.openfile_24;
            this.transformButton.Location = new System.Drawing.Point(318, 54);
            this.transformButton.Name = "transformButton";
            this.transformButton.Size = new System.Drawing.Size(30, 30);
            this.transformButton.TabIndex = 22;
            this.transformButton.UseVisualStyleBackColor = true;
            this.transformButton.Click += new System.EventHandler(this.transformButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(140, 383);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(63, 40);
            this.cancelButton.TabIndex = 19;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Enabled = false;
            this.saveButton.Image = global::CHAMP.Properties.Resources.SaveProject;
            this.saveButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.saveButton.Location = new System.Drawing.Point(209, 383);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(142, 40);
            this.saveButton.TabIndex = 20;
            this.saveButton.Text = "Save to File and Exit";
            this.saveButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // ChampApplyDockableWindow
            // 
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.transformDataGroupBox);
            this.Controls.Add(this.inputGroupBox1);
            this.Controls.Add(this.outputWorkspaceButton);
            this.Controls.Add(this.outputWorkspaceTextBox);
            this.Controls.Add(this.label2);
            this.Name = "ChampApplyDockableWindow";
            this.Size = new System.Drawing.Size(364, 438);
            this.Load += new System.EventHandler(this.ChampApplyDockableWindow_Load);
            this.inputGroupBox1.ResumeLayout(false);
            this.inputGroupBox1.PerformLayout();
            this.transformDataGroupBox.ResumeLayout(false);
            this.transformDataGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox inputDatasetTextBox;
        private System.Windows.Forms.Button inputDatasetButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button outputWorkspaceButton;
        private System.Windows.Forms.TextBox outputWorkspaceTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox controlPointsComboBox;
        private System.Windows.Forms.ComboBox attributeFieldComboBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox benchmark3ComboBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox benchmark2ComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox benchmark1ComboBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox inputGroupBox1;
        private System.Windows.Forms.GroupBox transformDataGroupBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox transformTextBox;
        private System.Windows.Forms.Button transformButton;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox benchmarkTextBox;
        private System.Windows.Forms.Button benchmarkButton;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button saveButton;

    }
}
