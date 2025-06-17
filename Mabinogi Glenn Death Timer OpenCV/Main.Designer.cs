namespace Mabi_CV
{
    partial class Main
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
            pb_debugging = new PictureBox();
            btn_debugging = new Button();
            ((System.ComponentModel.ISupportInitialize)pb_debugging).BeginInit();
            SuspendLayout();
            // 
            // pb_debugging
            // 
            pb_debugging.Location = new Point(113, 60);
            pb_debugging.Name = "pb_debugging";
            pb_debugging.Size = new Size(675, 361);
            pb_debugging.SizeMode = PictureBoxSizeMode.Zoom;
            pb_debugging.TabIndex = 0;
            pb_debugging.TabStop = false;
            // 
            // btn_debugging
            // 
            btn_debugging.Location = new Point(313, 15);
            btn_debugging.Name = "btn_debugging";
            btn_debugging.Size = new Size(75, 23);
            btn_debugging.TabIndex = 1;
            btn_debugging.Text = "button1";
            btn_debugging.UseVisualStyleBackColor = true;
            btn_debugging.Click += btn_debugging_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btn_debugging);
            Controls.Add(pb_debugging);
            Name = "Main";
            Text = "Main";
            ((System.ComponentModel.ISupportInitialize)pb_debugging).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pb_debugging;
        private Button btn_debugging;
    }
}