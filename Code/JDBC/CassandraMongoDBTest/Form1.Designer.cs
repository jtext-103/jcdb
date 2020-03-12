namespace CassandraMongoDBTest
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.CassandraButton = new System.Windows.Forms.RadioButton();
            this.MongoDBButton = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxSignalNum = new System.Windows.Forms.TextBox();
            this.textBoxDataNum = new System.Windows.Forms.TextBox();
            this.textBoxThreadNum = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ThreadNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AppendNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Operation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.butStart = new System.Windows.Forms.Button();
            this.butRestore = new System.Windows.Forms.Button();
            this.labelTime = new System.Windows.Forms.Label();
            this.textBoxShowTime = new System.Windows.Forms.TextBox();
            this.butClearTab = new System.Windows.Forms.Button();
            this.butSaveData = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxAppendNum = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button_Query = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // CassandraButton
            // 
            this.CassandraButton.AutoSize = true;
            this.CassandraButton.Location = new System.Drawing.Point(45, 24);
            this.CassandraButton.Name = "CassandraButton";
            this.CassandraButton.Size = new System.Drawing.Size(77, 16);
            this.CassandraButton.TabIndex = 0;
            this.CassandraButton.TabStop = true;
            this.CassandraButton.Text = "Cassandra";
            this.CassandraButton.UseVisualStyleBackColor = true;
            // 
            // MongoDBButton
            // 
            this.MongoDBButton.AutoSize = true;
            this.MongoDBButton.Location = new System.Drawing.Point(45, 47);
            this.MongoDBButton.Name = "MongoDBButton";
            this.MongoDBButton.Size = new System.Drawing.Size(59, 16);
            this.MongoDBButton.TabIndex = 1;
            this.MongoDBButton.TabStop = true;
            this.MongoDBButton.Text = "MongoD";
            this.MongoDBButton.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(12, 117);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 16);
            this.label4.TabIndex = 20;
            this.label4.Text = "数据个数：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(12, 158);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 16);
            this.label3.TabIndex = 19;
            this.label3.Text = "通道个数：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(12, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 16);
            this.label2.TabIndex = 18;
            this.label2.Text = "线程数量：";
            // 
            // textBoxSignalNum
            // 
            this.textBoxSignalNum.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxSignalNum.Location = new System.Drawing.Point(111, 155);
            this.textBoxSignalNum.Name = "textBoxSignalNum";
            this.textBoxSignalNum.Size = new System.Drawing.Size(100, 26);
            this.textBoxSignalNum.TabIndex = 17;
            // 
            // textBoxDataNum
            // 
            this.textBoxDataNum.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxDataNum.Location = new System.Drawing.Point(111, 117);
            this.textBoxDataNum.Name = "textBoxDataNum";
            this.textBoxDataNum.Size = new System.Drawing.Size(100, 26);
            this.textBoxDataNum.TabIndex = 16;
            // 
            // textBoxThreadNum
            // 
            this.textBoxThreadNum.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxThreadNum.Location = new System.Drawing.Point(111, 79);
            this.textBoxThreadNum.Name = "textBoxThreadNum";
            this.textBoxThreadNum.Size = new System.Drawing.Size(100, 26);
            this.textBoxThreadNum.TabIndex = 15;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ThreadNum,
            this.DataNum,
            this.DataSize,
            this.AppendNum,
            this.Time,
            this.Operation});
            this.dataGridView1.Location = new System.Drawing.Point(292, 10);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(489, 481);
            this.dataGridView1.TabIndex = 21;
            // 
            // ThreadNum
            // 
            this.ThreadNum.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ThreadNum.HeaderText = "线程数";
            this.ThreadNum.Name = "ThreadNum";
            this.ThreadNum.Width = 66;
            // 
            // DataNum
            // 
            this.DataNum.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.DataNum.HeaderText = "数据个数";
            this.DataNum.Name = "DataNum";
            this.DataNum.Width = 78;
            // 
            // DataSize
            // 
            this.DataSize.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.DataSize.HeaderText = "通道个数";
            this.DataSize.Name = "DataSize";
            this.DataSize.Width = 78;
            // 
            // AppendNum
            // 
            this.AppendNum.HeaderText = "采集次数";
            this.AppendNum.Name = "AppendNum";
            // 
            // Time
            // 
            this.Time.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Time.HeaderText = "时间";
            this.Time.Name = "Time";
            this.Time.Width = 54;
            // 
            // Operation
            // 
            this.Operation.HeaderText = "操作";
            this.Operation.Name = "Operation";
            // 
            // butStart
            // 
            this.butStart.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.butStart.Location = new System.Drawing.Point(12, 238);
            this.butStart.Name = "butStart";
            this.butStart.Size = new System.Drawing.Size(77, 32);
            this.butStart.TabIndex = 22;
            this.butStart.Text = "开始";
            this.butStart.UseVisualStyleBackColor = true;
            this.butStart.Click += new System.EventHandler(this.butStart_Click);
            // 
            // butRestore
            // 
            this.butRestore.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.butRestore.Location = new System.Drawing.Point(125, 238);
            this.butRestore.Name = "butRestore";
            this.butRestore.Size = new System.Drawing.Size(77, 32);
            this.butRestore.TabIndex = 23;
            this.butRestore.Text = "重置";
            this.butRestore.UseVisualStyleBackColor = true;
            this.butRestore.Click += new System.EventHandler(this.butRestore_Click);
            // 
            // labelTime
            // 
            this.labelTime.AutoSize = true;
            this.labelTime.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelTime.Location = new System.Drawing.Point(24, 284);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(53, 16);
            this.labelTime.TabIndex = 25;
            this.labelTime.Text = "Time:";
            // 
            // textBoxShowTime
            // 
            this.textBoxShowTime.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxShowTime.Location = new System.Drawing.Point(83, 276);
            this.textBoxShowTime.Name = "textBoxShowTime";
            this.textBoxShowTime.Size = new System.Drawing.Size(128, 26);
            this.textBoxShowTime.TabIndex = 24;
            // 
            // butClearTab
            // 
            this.butClearTab.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.butClearTab.Location = new System.Drawing.Point(12, 362);
            this.butClearTab.Name = "butClearTab";
            this.butClearTab.Size = new System.Drawing.Size(90, 31);
            this.butClearTab.TabIndex = 27;
            this.butClearTab.Text = "清空表格";
            this.butClearTab.UseVisualStyleBackColor = true;
            this.butClearTab.Click += new System.EventHandler(this.butClearTab_Click);
            // 
            // butSaveData
            // 
            this.butSaveData.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.butSaveData.Location = new System.Drawing.Point(14, 325);
            this.butSaveData.Name = "butSaveData";
            this.butSaveData.Size = new System.Drawing.Size(90, 31);
            this.butSaveData.TabIndex = 26;
            this.butSaveData.Text = "保存数据";
            this.butSaveData.UseVisualStyleBackColor = true;
            this.butSaveData.Click += new System.EventHandler(this.butSaveData_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(19, 419);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(93, 16);
            this.label6.TabIndex = 29;
            this.label6.Text = "保存说明：";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(111, 419);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(137, 21);
            this.textBox2.TabIndex = 28;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(11, 197);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 16);
            this.label1.TabIndex = 31;
            this.label1.Text = "采集次数：";
            // 
            // textBoxAppendNum
            // 
            this.textBoxAppendNum.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxAppendNum.Location = new System.Drawing.Point(110, 194);
            this.textBoxAppendNum.Name = "textBoxAppendNum";
            this.textBoxAppendNum.Size = new System.Drawing.Size(100, 26);
            this.textBoxAppendNum.TabIndex = 30;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(211, 245);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 32;
            this.button1.Text = "初始化";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button_Query
            // 
            this.button_Query.Location = new System.Drawing.Point(211, 308);
            this.button_Query.Name = "button_Query";
            this.button_Query.Size = new System.Drawing.Size(75, 23);
            this.button_Query.TabIndex = 33;
            this.button_Query.Text = "Query";
            this.button_Query.UseVisualStyleBackColor = true;
            this.button_Query.Click += new System.EventHandler(this.button_Query_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(108, 350);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 34;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(211, 337);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 35;
            this.button3.Text = "Query2";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(211, 370);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 36;
            this.button4.Text = "QueryWeb";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(216, 178);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 37;
            this.button5.Text = "button5";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(785, 503);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button_Query);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxAppendNum);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.butClearTab);
            this.Controls.Add(this.butSaveData);
            this.Controls.Add(this.labelTime);
            this.Controls.Add(this.textBoxShowTime);
            this.Controls.Add(this.butRestore);
            this.Controls.Add(this.butStart);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxSignalNum);
            this.Controls.Add(this.textBoxDataNum);
            this.Controls.Add(this.textBoxThreadNum);
            this.Controls.Add(this.MongoDBButton);
            this.Controls.Add(this.CassandraButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton CassandraButton;
        private System.Windows.Forms.RadioButton MongoDBButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxSignalNum;
        private System.Windows.Forms.TextBox textBoxDataNum;
        private System.Windows.Forms.TextBox textBoxThreadNum;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button butStart;
        private System.Windows.Forms.Button butRestore;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.TextBox textBoxShowTime;
        private System.Windows.Forms.Button butClearTab;
        private System.Windows.Forms.Button butSaveData;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxAppendNum;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button_Query;
        private System.Windows.Forms.DataGridViewTextBoxColumn ThreadNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn DataNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn DataSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn AppendNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn Time;
        private System.Windows.Forms.DataGridViewTextBoxColumn Operation;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
    }
}

