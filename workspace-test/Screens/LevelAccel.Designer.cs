namespace workspace_test.Screens
{
    partial class LevelAccel
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
            this.label1 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.kApplyButton = new System.Windows.Forms.Button();
            this.VBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.kBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.sumWeightBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.CsBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.IBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.RBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.SDSBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.applyButton = new System.Windows.Forms.Button();
            this.heightBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.weightBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.ErrorLabel = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.sumBox = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(40, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(582, 51);
            this.label1.TabIndex = 0;
            this.label1.Text = "Level Acceleration Calculator";
            // 
            // listView1
            // 
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(81, 482);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1316, 412);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.kApplyButton);
            this.groupBox1.Controls.Add(this.VBox);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.kBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.sumWeightBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.CsBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(81, 103);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(644, 289);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            // 
            // kApplyButton
            // 
            this.kApplyButton.AutoSize = true;
            this.kApplyButton.Location = new System.Drawing.Point(534, 156);
            this.kApplyButton.Margin = new System.Windows.Forms.Padding(0);
            this.kApplyButton.Name = "kApplyButton";
            this.kApplyButton.Size = new System.Drawing.Size(76, 42);
            this.kApplyButton.TabIndex = 10;
            this.kApplyButton.Text = "Apply";
            this.kApplyButton.UseVisualStyleBackColor = true;
            this.kApplyButton.Click += new System.EventHandler(this.kApplyButton_Click);
            // 
            // VBox
            // 
            this.VBox.Location = new System.Drawing.Point(357, 218);
            this.VBox.Multiline = true;
            this.VBox.Name = "VBox";
            this.VBox.ReadOnly = true;
            this.VBox.Size = new System.Drawing.Size(100, 31);
            this.VBox.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(27, 218);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(308, 31);
            this.label5.TabIndex = 6;
            this.label5.Text = "V (Seismic Base Shear):";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // kBox
            // 
            this.kBox.Location = new System.Drawing.Point(408, 162);
            this.kBox.Multiline = true;
            this.kBox.Name = "kBox";
            this.kBox.Size = new System.Drawing.Size(100, 31);
            this.kBox.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(27, 159);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(364, 31);
            this.label4.TabIndex = 4;
            this.label4.Text = "k (hx exponent based on Ta):";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // sumWeightBox
            // 
            this.sumWeightBox.Location = new System.Drawing.Point(218, 101);
            this.sumWeightBox.Multiline = true;
            this.sumWeightBox.Name = "sumWeightBox";
            this.sumWeightBox.ReadOnly = true;
            this.sumWeightBox.Size = new System.Drawing.Size(100, 31);
            this.sumWeightBox.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(27, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(174, 31);
            this.label3.TabIndex = 2;
            this.label3.Text = "Total Weight:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CsBox
            // 
            this.CsBox.Location = new System.Drawing.Point(488, 42);
            this.CsBox.Multiline = true;
            this.CsBox.Name = "CsBox";
            this.CsBox.ReadOnly = true;
            this.CsBox.Size = new System.Drawing.Size(100, 31);
            this.CsBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(27, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(443, 31);
            this.label2.TabIndex = 0;
            this.label2.Text = "Cs (Seismic Response Coefficient):";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.IBox);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.RBox);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.SDSBox);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Location = new System.Drawing.Point(774, 103);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(265, 299);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            // 
            // IBox
            // 
            this.IBox.Location = new System.Drawing.Point(111, 162);
            this.IBox.Name = "IBox";
            this.IBox.Size = new System.Drawing.Size(100, 31);
            this.IBox.TabIndex = 3;
            this.IBox.TextChanged += new System.EventHandler(this.IBox_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(75, 159);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(30, 31);
            this.label7.TabIndex = 4;
            this.label7.Text = "I:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // RBox
            // 
            this.RBox.Location = new System.Drawing.Point(111, 101);
            this.RBox.Name = "RBox";
            this.RBox.Size = new System.Drawing.Size(100, 31);
            this.RBox.TabIndex = 2;
            this.RBox.TextChanged += new System.EventHandler(this.RBox_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(63, 98);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 31);
            this.label8.TabIndex = 2;
            this.label8.Text = "R:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SDSBox
            // 
            this.SDSBox.Location = new System.Drawing.Point(111, 42);
            this.SDSBox.Name = "SDSBox";
            this.SDSBox.Size = new System.Drawing.Size(100, 31);
            this.SDSBox.TabIndex = 1;
            this.SDSBox.TextChanged += new System.EventHandler(this.SDSBox_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(27, 39);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(78, 31);
            this.label9.TabIndex = 0;
            this.label9.Text = "SDS:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.applyButton);
            this.groupBox3.Controls.Add(this.heightBox);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.weightBox);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(1098, 103);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(299, 299);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Floor 1";
            // 
            // applyButton
            // 
            this.applyButton.AutoSize = true;
            this.applyButton.Location = new System.Drawing.Point(165, 235);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(115, 42);
            this.applyButton.TabIndex = 9;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // heightBox
            // 
            this.heightBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.heightBox.Location = new System.Drawing.Point(165, 142);
            this.heightBox.Name = "heightBox";
            this.heightBox.Size = new System.Drawing.Size(100, 31);
            this.heightBox.TabIndex = 7;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(25, 139);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(134, 31);
            this.label10.TabIndex = 8;
            this.label10.Text = "Elevation:";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // weightBox
            // 
            this.weightBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.weightBox.Location = new System.Drawing.Point(165, 83);
            this.weightBox.Name = "weightBox";
            this.weightBox.Size = new System.Drawing.Size(100, 31);
            this.weightBox.TabIndex = 6;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(53, 80);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(106, 31);
            this.label11.TabIndex = 5;
            this.label11.Text = "Weight:";
            // 
            // ErrorLabel
            // 
            this.ErrorLabel.Location = new System.Drawing.Point(1093, 9);
            this.ErrorLabel.Name = "ErrorLabel";
            this.ErrorLabel.Size = new System.Drawing.Size(304, 91);
            this.ErrorLabel.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(75, 435);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(189, 31);
            this.label6.TabIndex = 10;
            this.label6.Text = "Sum (w * h^k):";
            // 
            // sumBox
            // 
            this.sumBox.Location = new System.Drawing.Point(270, 438);
            this.sumBox.Multiline = true;
            this.sumBox.Name = "sumBox";
            this.sumBox.ReadOnly = true;
            this.sumBox.Size = new System.Drawing.Size(100, 31);
            this.sumBox.TabIndex = 7;
            // 
            // LevelAccel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1481, 956);
            this.Controls.Add(this.sumBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ErrorLabel);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.label1);
            this.Name = "LevelAccel";
            this.Text = "LevelAccel";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox VBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox kBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox sumWeightBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox CsBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox IBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox RBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox SDSBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox heightBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox weightBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Label ErrorLabel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox sumBox;
        private System.Windows.Forms.Button kApplyButton;
    }
}