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
            txtbx_topleft = new TextBox();
            pnl_Crop = new Panel();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            textBox3 = new TextBox();
            textBox2 = new TextBox();
            textBox1 = new TextBox();
            label1 = new Label();
            richtx_debugging = new RichTextBox();
            ((System.ComponentModel.ISupportInitialize)pb_debugging).BeginInit();
            pnl_Crop.SuspendLayout();
            SuspendLayout();
            // 
            // pb_debugging
            // 
            pb_debugging.Location = new Point(12, 54);
            pb_debugging.Name = "pb_debugging";
            pb_debugging.Size = new Size(675, 361);
            pb_debugging.SizeMode = PictureBoxSizeMode.Zoom;
            pb_debugging.TabIndex = 0;
            pb_debugging.TabStop = false;
            // 
            // btn_debugging
            // 
            btn_debugging.Location = new Point(12, 12);
            btn_debugging.Name = "btn_debugging";
            btn_debugging.Size = new Size(75, 23);
            btn_debugging.TabIndex = 1;
            btn_debugging.Text = "button1";
            btn_debugging.UseVisualStyleBackColor = true;
            btn_debugging.Click += btn_debugging_Click;
            // 
            // txtbx_topleft
            // 
            txtbx_topleft.Location = new Point(40, 53);
            txtbx_topleft.Name = "txtbx_topleft";
            txtbx_topleft.Size = new Size(100, 23);
            txtbx_topleft.TabIndex = 2;
            txtbx_topleft.Text = "2247";
            // 
            // pnl_Crop
            // 
            pnl_Crop.Controls.Add(label5);
            pnl_Crop.Controls.Add(label4);
            pnl_Crop.Controls.Add(label3);
            pnl_Crop.Controls.Add(label2);
            pnl_Crop.Controls.Add(textBox3);
            pnl_Crop.Controls.Add(textBox2);
            pnl_Crop.Controls.Add(textBox1);
            pnl_Crop.Controls.Add(label1);
            pnl_Crop.Controls.Add(txtbx_topleft);
            pnl_Crop.Location = new Point(704, 54);
            pnl_Crop.Name = "pnl_Crop";
            pnl_Crop.Size = new Size(279, 139);
            pnl_Crop.TabIndex = 3;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(146, 35);
            label5.Name = "label5";
            label5.Size = new Size(78, 15);
            label5.TabIndex = 9;
            label5.Text = "Bottom Right";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(21, 85);
            label4.Name = "label4";
            label4.Size = new Size(13, 15);
            label4.TabIndex = 8;
            label4.Text = "y";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(21, 56);
            label3.Name = "label3";
            label3.Size = new Size(13, 15);
            label3.TabIndex = 4;
            label3.Text = "x";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(40, 35);
            label2.Name = "label2";
            label2.Size = new Size(49, 15);
            label2.TabIndex = 7;
            label2.Text = "Top Left";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(146, 82);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(100, 23);
            textBox3.TabIndex = 6;
            textBox3.Text = "318";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(40, 82);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(100, 23);
            textBox2.TabIndex = 5;
            textBox2.Text = "160";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(146, 53);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 4;
            textBox1.Text = "2560";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 9);
            label1.Name = "label1";
            label1.Size = new Size(33, 15);
            label1.TabIndex = 3;
            label1.Text = "Crop";
            // 
            // richtx_debugging
            // 
            richtx_debugging.Location = new Point(704, 210);
            richtx_debugging.Name = "richtx_debugging";
            richtx_debugging.Size = new Size(405, 205);
            richtx_debugging.TabIndex = 5;
            richtx_debugging.Text = "";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1451, 649);
            Controls.Add(richtx_debugging);
            Controls.Add(pnl_Crop);
            Controls.Add(btn_debugging);
            Controls.Add(pb_debugging);
            Name = "Main";
            Text = "Main";
            ((System.ComponentModel.ISupportInitialize)pb_debugging).EndInit();
            pnl_Crop.ResumeLayout(false);
            pnl_Crop.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pb_debugging;
        private Button btn_debugging;
        private TextBox txtbx_topleft;
        private Panel pnl_Crop;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label2;
        private TextBox textBox3;
        private TextBox textBox2;
        private TextBox textBox1;
        private Label label1;
        private RichTextBox richtx_debugging;
    }
}