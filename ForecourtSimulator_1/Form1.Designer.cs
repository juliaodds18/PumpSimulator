﻿namespace ForecourtSimulator_1
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonRemovePump = new System.Windows.Forms.Button();
            this.buttonAddPump = new System.Windows.Forms.Button();
            this.labelHeader = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(60)))), ((int)(((byte)(78)))));
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.buttonRemovePump);
            this.panel1.Controls.Add(this.buttonAddPump);
            this.panel1.Controls.Add(this.labelHeader);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1440, 41);
            this.panel1.TabIndex = 2;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ForecourtSimulator_1.Properties.Resources.LS_Fuel_App_icon;
            this.pictureBox1.Location = new System.Drawing.Point(3, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(44, 41);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // buttonRemovePump
            // 
            this.buttonRemovePump.AutoSize = true;
            this.buttonRemovePump.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonRemovePump.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRemovePump.Location = new System.Drawing.Point(1301, 5);
            this.buttonRemovePump.Name = "buttonRemovePump";
            this.buttonRemovePump.Size = new System.Drawing.Size(119, 32);
            this.buttonRemovePump.TabIndex = 0;
            this.buttonRemovePump.Text = "Remove Pump";
            this.buttonRemovePump.UseVisualStyleBackColor = true;
            this.buttonRemovePump.Click += new System.EventHandler(this.buttonRemovePump_Click);
            // 
            // buttonAddPump
            // 
            this.buttonAddPump.AutoSize = true;
            this.buttonAddPump.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonAddPump.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddPump.Location = new System.Drawing.Point(1176, 5);
            this.buttonAddPump.Name = "buttonAddPump";
            this.buttonAddPump.Size = new System.Drawing.Size(119, 32);
            this.buttonAddPump.TabIndex = 1;
            this.buttonAddPump.Text = "Add Pump";
            this.buttonAddPump.UseVisualStyleBackColor = true;
            this.buttonAddPump.Click += new System.EventHandler(this.buttonAddPump_Click);
            // 
            // labelHeader
            // 
            this.labelHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelHeader.AutoSize = true;
            this.labelHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(186)))), ((int)(((byte)(88)))));
            this.labelHeader.Location = new System.Drawing.Point(50, 3);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Size = new System.Drawing.Size(184, 29);
            this.labelHeader.TabIndex = 0;
            this.labelHeader.Text = "Pump Simulator";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ClientSize = new System.Drawing.Size(1475, 915);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Forecourt Pump Simulator";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonAddPump;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.Button buttonRemovePump;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

