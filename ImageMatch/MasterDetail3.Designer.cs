namespace howto_image_hash
{
    partial class MasterDetail3
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnLoad = new System.Windows.Forms.Button();
            this.splitButton1 = new wyDay.Controls.SplitButton();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnFilter = new System.Windows.Forms.Button();
            this.btnReport = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.listZipPairs = new System.Windows.Forms.ListBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.listFilePairs = new System.Windows.Forms.ListBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnHideL = new System.Windows.Forms.Button();
            this.btnHideR = new System.Windows.Forms.Button();
            this.txtNote = new System.Windows.Forms.TextBox();
            this.btnDiff = new System.Windows.Forms.Button();
            this.btnShow = new System.Windows.Forms.Button();
            this.btnDelLeft = new System.Windows.Forms.Button();
            this.btnDelRight = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35.35353F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.32323F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.32323F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.listZipPairs, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.pictureBox2, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.label2, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.listFilePairs, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnDiff, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnShow, 2, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1063, 535);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 4);
            this.flowLayoutPanel1.Controls.Add(this.btnLoad);
            this.flowLayoutPanel1.Controls.Add(this.splitButton1);
            this.flowLayoutPanel1.Controls.Add(this.progressBar1);
            this.flowLayoutPanel1.Controls.Add(this.btnClear);
            this.flowLayoutPanel1.Controls.Add(this.btnFilter);
            this.flowLayoutPanel1.Controls.Add(this.btnReport);
            this.flowLayoutPanel1.Controls.Add(this.lblStatus);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1057, 29);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(3, 3);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(75, 23);
            this.btnLoad.TabIndex = 0;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // splitButton1
            // 
            this.splitButton1.AutoSize = true;
            this.splitButton1.Location = new System.Drawing.Point(84, 3);
            this.splitButton1.Name = "splitButton1";
            this.splitButton1.Size = new System.Drawing.Size(75, 23);
            this.splitButton1.TabIndex = 5;
            this.splitButton1.Text = "Load";
            this.splitButton1.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(165, 3);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(194, 23);
            this.progressBar1.TabIndex = 3;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(365, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 1;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnFilter
            // 
            this.btnFilter.Location = new System.Drawing.Point(446, 3);
            this.btnFilter.Name = "btnFilter";
            this.btnFilter.Size = new System.Drawing.Size(75, 23);
            this.btnFilter.TabIndex = 4;
            this.btnFilter.Text = "Filter";
            this.btnFilter.UseVisualStyleBackColor = true;
            this.btnFilter.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // btnReport
            // 
            this.btnReport.Location = new System.Drawing.Point(527, 3);
            this.btnReport.Name = "btnReport";
            this.btnReport.Size = new System.Drawing.Size(75, 23);
            this.btnReport.TabIndex = 2;
            this.btnReport.Text = "Report";
            this.btnReport.UseVisualStyleBackColor = true;
            this.btnReport.Click += new System.EventHandler(this.btnReport_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(608, 8);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 6;
            // 
            // listZipPairs
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.listZipPairs, 4);
            this.listZipPairs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listZipPairs.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listZipPairs.FormattingEnabled = true;
            this.listZipPairs.HorizontalScrollbar = true;
            this.listZipPairs.IntegralHeight = false;
            this.listZipPairs.ItemHeight = 20;
            this.listZipPairs.Location = new System.Drawing.Point(3, 38);
            this.listZipPairs.Name = "listZipPairs";
            this.listZipPairs.Size = new System.Drawing.Size(1057, 141);
            this.listZipPairs.TabIndex = 1;
            this.listZipPairs.SelectedIndexChanged += new System.EventHandler(this.listZipPairs_SelectedIndexChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(361, 185);
            this.pictureBox1.Name = "pictureBox1";
            this.tableLayoutPanel1.SetRowSpan(this.pictureBox1, 2);
            this.pictureBox1.Size = new System.Drawing.Size(321, 328);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox2.Location = new System.Drawing.Point(738, 185);
            this.pictureBox2.Name = "pictureBox2";
            this.tableLayoutPanel1.SetRowSpan(this.pictureBox2, 2);
            this.pictureBox2.Size = new System.Drawing.Size(322, 328);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 4;
            this.pictureBox2.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(738, 516);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 18);
            this.label2.TabIndex = 6;
            this.label2.Text = "label2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(361, 516);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 18);
            this.label1.TabIndex = 5;
            this.label1.Text = "label1";
            // 
            // listFilePairs
            // 
            this.listFilePairs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listFilePairs.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listFilePairs.FormattingEnabled = true;
            this.listFilePairs.HorizontalScrollbar = true;
            this.listFilePairs.IntegralHeight = false;
            this.listFilePairs.ItemHeight = 18;
            this.listFilePairs.Location = new System.Drawing.Point(3, 246);
            this.listFilePairs.Name = "listFilePairs";
            this.listFilePairs.ScrollAlwaysVisible = true;
            this.listFilePairs.Size = new System.Drawing.Size(352, 267);
            this.listFilePairs.TabIndex = 2;
            this.listFilePairs.SelectedIndexChanged += new System.EventHandler(this.listFilePairs_SelectedIndexChanged);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.btnHideL);
            this.flowLayoutPanel2.Controls.Add(this.btnHideR);
            this.flowLayoutPanel2.Controls.Add(this.btnDelLeft);
            this.flowLayoutPanel2.Controls.Add(this.btnDelRight);
            this.flowLayoutPanel2.Controls.Add(this.label3);
            this.flowLayoutPanel2.Controls.Add(this.txtNote);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 185);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(324, 55);
            this.flowLayoutPanel2.TabIndex = 8;
            // 
            // btnHideL
            // 
            this.btnHideL.Location = new System.Drawing.Point(3, 3);
            this.btnHideL.Name = "btnHideL";
            this.btnHideL.Size = new System.Drawing.Size(75, 23);
            this.btnHideL.TabIndex = 7;
            this.btnHideL.Text = "Hide (Left)";
            this.btnHideL.UseVisualStyleBackColor = true;
            this.btnHideL.Click += new System.EventHandler(this.btnHideL_Click);
            // 
            // btnHideR
            // 
            this.btnHideR.Location = new System.Drawing.Point(84, 3);
            this.btnHideR.Name = "btnHideR";
            this.btnHideR.Size = new System.Drawing.Size(75, 23);
            this.btnHideR.TabIndex = 8;
            this.btnHideR.Text = "Hide (Right)";
            this.btnHideR.UseVisualStyleBackColor = true;
            this.btnHideR.Click += new System.EventHandler(this.btnHideR_Click);
            // 
            // txtNote
            // 
            this.txtNote.Location = new System.Drawing.Point(42, 32);
            this.txtNote.Name = "txtNote";
            this.txtNote.Size = new System.Drawing.Size(200, 20);
            this.txtNote.TabIndex = 9;
            // 
            // btnDiff
            // 
            this.btnDiff.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnDiff.AutoSize = true;
            this.btnDiff.Location = new System.Drawing.Point(693, 217);
            this.btnDiff.Name = "btnDiff";
            this.btnDiff.Size = new System.Drawing.Size(33, 23);
            this.btnDiff.TabIndex = 9;
            this.btnDiff.Text = "Diff";
            this.btnDiff.UseVisualStyleBackColor = true;
            this.btnDiff.Click += new System.EventHandler(this.BtnDiff_Click);
            // 
            // btnShow
            // 
            this.btnShow.AutoSize = true;
            this.btnShow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnShow.Location = new System.Drawing.Point(688, 246);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(44, 23);
            this.btnShow.TabIndex = 10;
            this.btnShow.Text = "Show";
            this.btnShow.UseVisualStyleBackColor = true;
            this.btnShow.Click += new System.EventHandler(this.BtnShow_Click);
            // 
            // btnDelLeft
            // 
            this.btnDelLeft.Location = new System.Drawing.Point(165, 3);
            this.btnDelLeft.Name = "btnDelLeft";
            this.btnDelLeft.Size = new System.Drawing.Size(75, 23);
            this.btnDelLeft.TabIndex = 10;
            this.btnDelLeft.Text = "Del. Left";
            this.btnDelLeft.UseVisualStyleBackColor = true;
            this.btnDelLeft.Click += new System.EventHandler(this.BtnDelLeft_Click);
            // 
            // btnDelRight
            // 
            this.btnDelRight.Location = new System.Drawing.Point(246, 3);
            this.btnDelRight.Name = "btnDelRight";
            this.btnDelRight.Size = new System.Drawing.Size(75, 23);
            this.btnDelRight.TabIndex = 11;
            this.btnDelRight.Text = "Del. Right";
            this.btnDelRight.UseVisualStyleBackColor = true;
            this.btnDelRight.Click += new System.EventHandler(this.BtnDelRight_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Note:";
            // 
            // MasterDetail3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1063, 535);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MasterDetail3";
            this.Text = "MasterDetail3";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MasterDetail3_FormClosing);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnReport;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ListBox listZipPairs;
        private System.Windows.Forms.ListBox listFilePairs;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnHideL;
        private System.Windows.Forms.Button btnFilter;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button btnHideR;
        private System.Windows.Forms.TextBox txtNote;
        private System.Windows.Forms.Button btnDiff;
        private System.Windows.Forms.Button btnShow;
        private wyDay.Controls.SplitButton splitButton1;
        private System.Windows.Forms.Button btnDelLeft;
        private System.Windows.Forms.Button btnDelRight;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblStatus;
    }
}