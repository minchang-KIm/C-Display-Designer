namespace TInspSolutionPlatForm
{
    partial class DisplayDesigner
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvForm = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.auto_column = new System.Windows.Forms.TextBox();
            this.auto_row = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.DPtextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.default_btn = new System.Windows.Forms.Button();
            this.save_btn = new System.Windows.Forms.Button();
            this.num_btn = new System.Windows.Forms.Button();
            this.modify_btn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.dp_col_label = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.dp_row_label = new System.Windows.Forms.Label();
            this.dp_num_label = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSetForm = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.save = new System.Windows.Forms.Button();
            this.delete = new System.Windows.Forms.Button();
            this.InitNumBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvForm)).BeginInit();
            this.panel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvForm
            // 
            this.dgvForm.AllowUserToAddRows = false;
            this.dgvForm.AllowUserToDeleteRows = false;
            this.dgvForm.AllowUserToResizeColumns = false;
            this.dgvForm.AllowUserToResizeRows = false;
            this.dgvForm.BackgroundColor = System.Drawing.Color.White;
            this.dgvForm.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvForm.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvForm.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvForm.ColumnHeadersVisible = false;
            this.dgvForm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.dgvForm.GridColor = System.Drawing.Color.Black;
            this.dgvForm.Location = new System.Drawing.Point(12, 173);
            this.dgvForm.Margin = new System.Windows.Forms.Padding(5, 5, 5, 1);
            this.dgvForm.Name = "dgvForm";
            this.dgvForm.ReadOnly = true;
            this.dgvForm.RowHeadersVisible = false;
            this.dgvForm.RowHeadersWidth = 15;
            this.dgvForm.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgvForm.RowTemplate.Height = 15;
            this.dgvForm.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvForm.Size = new System.Drawing.Size(47, 41);
            this.dgvForm.TabIndex = 0;
            this.dgvForm.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvForm_CellClick);
            this.dgvForm.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvForm_CellDoubleClick);
            this.dgvForm.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvForm_CellDoubleClick);
            this.dgvForm.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvForm_CellEndEdit);
            this.dgvForm.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvForm_CellMouseDown);
            this.dgvForm.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvForm_CellMouseUp);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.save_btn);
            this.panel1.Controls.Add(this.modify_btn);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.listBox1);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.save);
            this.panel1.Controls.Add(this.delete);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(694, 153);
            this.panel1.TabIndex = 20;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.auto_column);
            this.groupBox3.Controls.Add(this.auto_row);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.DPtextbox);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.default_btn);
            this.groupBox3.Location = new System.Drawing.Point(203, 7);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(233, 140);
            this.groupBox3.TabIndex = 22;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "자동생성";
            // 
            // auto_column
            // 
            this.auto_column.Location = new System.Drawing.Point(76, 88);
            this.auto_column.Name = "auto_column";
            this.auto_column.Size = new System.Drawing.Size(100, 21);
            this.auto_column.TabIndex = 24;
            this.auto_column.Text = "0";
            // 
            // auto_row
            // 
            this.auto_row.Location = new System.Drawing.Point(76, 60);
            this.auto_row.Name = "auto_row";
            this.auto_row.Size = new System.Drawing.Size(100, 21);
            this.auto_row.TabIndex = 23;
            this.auto_row.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 12);
            this.label3.TabIndex = 22;
            this.label3.Text = "Column";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 12);
            this.label2.TabIndex = 21;
            this.label2.Text = "Row";
            // 
            // DPtextbox
            // 
            this.DPtextbox.Location = new System.Drawing.Point(105, 30);
            this.DPtextbox.Name = "DPtextbox";
            this.DPtextbox.Size = new System.Drawing.Size(71, 21);
            this.DPtextbox.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 12);
            this.label1.TabIndex = 15;
            this.label1.Text = "디스플레이 개수";
            // 
            // default_btn
            // 
            this.default_btn.Location = new System.Drawing.Point(160, 114);
            this.default_btn.Name = "default_btn";
            this.default_btn.Size = new System.Drawing.Size(67, 23);
            this.default_btn.TabIndex = 20;
            this.default_btn.Text = "자동설정";
            this.default_btn.UseVisualStyleBackColor = true;
            this.default_btn.Click += new System.EventHandler(this.default_btn_Click);
            // 
            // save_btn
            // 
            this.save_btn.Location = new System.Drawing.Point(112, 96);
            this.save_btn.Name = "save_btn";
            this.save_btn.Size = new System.Drawing.Size(75, 23);
            this.save_btn.TabIndex = 19;
            this.save_btn.Text = "저장";
            this.save_btn.UseVisualStyleBackColor = true;
            this.save_btn.Click += new System.EventHandler(this.save_btn_Click);
            // 
            // num_btn
            // 
            this.num_btn.Location = new System.Drawing.Point(134, 104);
            this.num_btn.Name = "num_btn";
            this.num_btn.Size = new System.Drawing.Size(75, 23);
            this.num_btn.TabIndex = 21;
            this.num_btn.Text = "넘버링";
            this.num_btn.UseVisualStyleBackColor = true;
            this.num_btn.Click += new System.EventHandler(this.num_btn_Click);
            // 
            // modify_btn
            // 
            this.modify_btn.Location = new System.Drawing.Point(112, 67);
            this.modify_btn.Name = "modify_btn";
            this.modify_btn.Size = new System.Drawing.Size(75, 23);
            this.modify_btn.TabIndex = 16;
            this.modify_btn.Text = "수정";
            this.modify_btn.UseVisualStyleBackColor = true;
            this.modify_btn.Click += new System.EventHandler(this.modify_btn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.InitNumBtn);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.textBox4);
            this.groupBox1.Controls.Add(this.num_btn);
            this.groupBox1.Controls.Add(this.dp_col_label);
            this.groupBox1.Controls.Add(this.textBox5);
            this.groupBox1.Controls.Add(this.dp_row_label);
            this.groupBox1.Controls.Add(this.dp_num_label);
            this.groupBox1.Location = new System.Drawing.Point(442, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(224, 141);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "정보";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(158, 50);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(51, 21);
            this.textBox1.TabIndex = 5;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(158, 77);
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(51, 21);
            this.textBox4.TabIndex = 6;
            // 
            // dp_col_label
            // 
            this.dp_col_label.AutoSize = true;
            this.dp_col_label.Location = new System.Drawing.Point(15, 80);
            this.dp_col_label.Name = "dp_col_label";
            this.dp_col_label.Size = new System.Drawing.Size(137, 12);
            this.dp_col_label.TabIndex = 14;
            this.dp_col_label.Text = "오른쪽 아래 꼭짓점 좌표";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(158, 24);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(51, 21);
            this.textBox5.TabIndex = 8;
            // 
            // dp_row_label
            // 
            this.dp_row_label.AutoSize = true;
            this.dp_row_label.Location = new System.Drawing.Point(15, 53);
            this.dp_row_label.Name = "dp_row_label";
            this.dp_row_label.Size = new System.Drawing.Size(113, 12);
            this.dp_row_label.TabIndex = 13;
            this.dp_row_label.Text = "왼쪽 위 꼭짓점 좌표";
            // 
            // dp_num_label
            // 
            this.dp_num_label.AutoSize = true;
            this.dp_num_label.Location = new System.Drawing.Point(15, 28);
            this.dp_num_label.Name = "dp_num_label";
            this.dp_num_label.Size = new System.Drawing.Size(93, 12);
            this.dp_num_label.TabIndex = 12;
            this.dp_num_label.Text = "디스플레이 번호";
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 12;
            this.listBox1.Location = new System.Drawing.Point(10, 71);
            this.listBox1.Name = "listBox1";
            this.listBox1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.listBox1.Size = new System.Drawing.Size(96, 76);
            this.listBox1.TabIndex = 7;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSetForm);
            this.groupBox2.Controls.Add(this.textBox3);
            this.groupBox2.Location = new System.Drawing.Point(10, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(96, 60);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "크기";
            // 
            // btnSetForm
            // 
            this.btnSetForm.Location = new System.Drawing.Point(44, 26);
            this.btnSetForm.Name = "btnSetForm";
            this.btnSetForm.Size = new System.Drawing.Size(52, 23);
            this.btnSetForm.TabIndex = 3;
            this.btnSetForm.Text = "초기화";
            this.btnSetForm.UseVisualStyleBackColor = true;
            this.btnSetForm.Click += new System.EventHandler(this.btnSetForm_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(6, 26);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(32, 21);
            this.textBox3.TabIndex = 4;
            this.textBox3.Text = "20";
            // 
            // save
            // 
            this.save.Location = new System.Drawing.Point(112, 7);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(75, 23);
            this.save.TabIndex = 9;
            this.save.Text = "추가";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.save_Click);
            // 
            // delete
            // 
            this.delete.Location = new System.Drawing.Point(112, 37);
            this.delete.Name = "delete";
            this.delete.Size = new System.Drawing.Size(75, 23);
            this.delete.TabIndex = 10;
            this.delete.Text = "삭제";
            this.delete.UseVisualStyleBackColor = true;
            this.delete.Click += new System.EventHandler(this.delete_Click);
            // 
            // InitNumBtn
            // 
            this.InitNumBtn.Location = new System.Drawing.Point(17, 104);
            this.InitNumBtn.Name = "InitNumBtn";
            this.InitNumBtn.Size = new System.Drawing.Size(99, 23);
            this.InitNumBtn.TabIndex = 22;
            this.InitNumBtn.Text = "넘버링 초기화";
            this.InitNumBtn.UseVisualStyleBackColor = true;
            this.InitNumBtn.Click += new System.EventHandler(this.InitNumBtn_Click);
            // 
            // DisplayDesigner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(732, 364);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.dgvForm);
            this.Name = "DisplayDesigner";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dgvForm)).EndInit();
            this.panel1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvForm;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button save_btn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label dp_col_label;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Label dp_row_label;
        private System.Windows.Forms.Label dp_num_label;
        private System.Windows.Forms.Button modify_btn;
        private System.Windows.Forms.Button delete;
        private System.Windows.Forms.Button save;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnSetForm;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button default_btn;
        private System.Windows.Forms.Button num_btn;
        private System.Windows.Forms.TextBox DPtextbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox auto_column;
        private System.Windows.Forms.TextBox auto_row;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button InitNumBtn;
    }
}

