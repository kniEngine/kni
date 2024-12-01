using System.Windows.Forms;

namespace Content.Pipeline.Editor.Forms
{
    partial class PackageDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackageDialog));
            tbName = new TextBox();
            okButton = new Button();
            cancelButton = new Button();
            label1 = new Label();
            label2 = new Label();
            tbVersion = new TextBox();
            llSearchNuget = new LinkLabel();
            SuspendLayout();
            // 
            // tbName
            // 
            tbName.Location = new System.Drawing.Point(12, 32);
            tbName.Name = "tbName";
            tbName.Size = new System.Drawing.Size(359, 27);
            tbName.TabIndex = 0;
            // 
            // okButton
            // 
            okButton.DialogResult = DialogResult.OK;
            okButton.Location = new System.Drawing.Point(296, 88);
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(100, 35);
            okButton.TabIndex = 2;
            okButton.Text = "&OK";
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new System.Drawing.Point(402, 88);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(100, 35);
            cancelButton.TabIndex = 3;
            cancelButton.Text = "&Cancel";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(63, 20);
            label1.TabIndex = 3;
            label1.Text = "Package";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(377, 9);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(57, 20);
            label2.TabIndex = 4;
            label2.Text = "Version";
            // 
            // tbVersion
            // 
            tbVersion.Location = new System.Drawing.Point(377, 32);
            tbVersion.Name = "tbVersion";
            tbVersion.Size = new System.Drawing.Size(125, 27);
            tbVersion.TabIndex = 1;
            // 
            // llSearchNuget
            // 
            llSearchNuget.AutoSize = true;
            llSearchNuget.Location = new System.Drawing.Point(12, 62);
            llSearchNuget.Name = "llSearchNuget";
            llSearchNuget.Size = new System.Drawing.Size(140, 20);
            llSearchNuget.TabIndex = 5;
            llSearchNuget.TabStop = true;
            llSearchNuget.Text = "search on nuget.org";
            llSearchNuget.LinkClicked += llSearchNuget_LinkClicked;
            // 
            // PackageDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(514, 135);
            Controls.Add(llSearchNuget);
            Controls.Add(tbVersion);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(tbName);
            Controls.Add(okButton);
            Controls.Add(cancelButton);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "PackageDialog";
            Text = "Package";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button okButton;
        private Button cancelButton;
        private Label label1;
        private Label label2;
        public TextBox tbName;
        public TextBox tbVersion;
        private LinkLabel llSearchNuget;
    }
}