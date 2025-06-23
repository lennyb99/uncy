namespace uncy.gui
{
    partial class Form1
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
            sidePanel = new Panel();
            mainPanel = new Panel();
            SuspendLayout();
            // 
            // sidePanel
            // 
            sidePanel.Dock = DockStyle.Right;
            sidePanel.Location = new Point(988, 0);
            sidePanel.Name = "sidePanel";
            sidePanel.Size = new Size(181, 888);
            sidePanel.TabIndex = 0;
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.Black;
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(988, 888);
            mainPanel.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DimGray;
            ClientSize = new Size(1169, 888);
            Controls.Add(mainPanel);
            Controls.Add(sidePanel);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Panel sidePanel;
        private Panel mainPanel;
    }
}