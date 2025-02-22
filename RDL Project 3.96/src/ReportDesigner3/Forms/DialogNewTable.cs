using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;
using System.Xml;
using fyiReporting.RDL;


namespace fyiReporting.RdlDesign
{
	/// <summary>
	/// Summary description for DialogDataSourceRef.
	/// </summary>
	class DialogNewTable : System.Windows.Forms.Form
	{
		private DesignXmlDraw _Draw;
		private System.Windows.Forms.Button bOK;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.ComboBox cbDataSets;
		private System.Windows.Forms.ListBox lbFields;
		private System.Windows.Forms.CheckedListBox lbTableColumns;
		private System.Windows.Forms.Button bUp;
		private System.Windows.Forms.Button bDown;
		private System.Windows.Forms.Button bRight;
		private System.Windows.Forms.Button bAllRight;
		private System.Windows.Forms.Button bLeft;
        private System.Windows.Forms.Button bAllLeft;
		private System.Windows.Forms.ComboBox cbGroupColumn;
		private System.Windows.Forms.CheckBox chkGrandTotals;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton rbHorz;
		private System.Windows.Forms.RadioButton rbVert;
		private System.Windows.Forms.RadioButton rbVertComp;
        private Label label3;
        private Label label2;
        private Label label1;
        private Label label4;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		internal DialogNewTable(DesignXmlDraw dxDraw, XmlNode container)
		{
			_Draw = dxDraw;
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			InitValues(container);
		}

		private void InitValues(XmlNode container)
		{
			this.bOK.Enabled = false;
			rbHorz.Checked = true;
			//
			// Obtain the existing DataSets info
			//
			object[] datasets = _Draw.DataSetNames;
			if (datasets == null)
				return;		// not much to do if no DataSets

			if (_Draw.IsDataRegion(container))
			{
				string s = _Draw.GetDataSetNameValue(container);
				if (s == null)
					return;
				this.cbDataSets.Items.Add(s);
				this.cbDataSets.Enabled = false;
			}
			else
				this.cbDataSets.Items.AddRange(datasets);
			cbDataSets.SelectedIndex = 0;
		}

		internal string TableXml
		{
			get
			{
				return rbHorz.Checked? TableXmlHorz: TableXmlVert;
			}
		}

		private string TableXmlHorz
		{
			get 
			{
				StringBuilder table = new StringBuilder("<Table>");
				table.AppendFormat("<DataSetName>{0}</DataSetName>", this.cbDataSets.Text);
				table.Append("<NoRows>Query returned no rows!</NoRows><Style>"+
					"<BorderStyle><Default>Solid</Default></BorderStyle></Style>");

				StringBuilder tablecolumns = new StringBuilder("<TableColumns>");

				StringBuilder headercolumns = 
					new StringBuilder("<Header><TableRows><TableRow><Height>12 pt</Height><TableCells>");

				StringBuilder detailcolumns = 
					new StringBuilder("<Details><TableRows><TableRow><Height>12 pt</Height><TableCells>");

				StringBuilder tablegroups= null;
				StringBuilder footergroup=null;
				string gname = this.cbGroupColumn.Text;
				if (gname != null && gname.Trim() != "")
				{
					gname = gname.Trim();
					tablegroups = 
						new StringBuilder("<TableGroups><TableGroup><Grouping><GroupExpressions><GroupExpression>");
					tablegroups.AppendFormat("=Fields!{0}.Value</GroupExpression></GroupExpressions></Grouping>", gname);
					tablegroups.Append("<Header><TableRows><TableRow><Height>12 pt</Height><TableCells>");
					footergroup = 
						new StringBuilder("<Footer><TableRows><TableRow><Height>12 pt</Height><TableCells>");	
				}
				else
					gname = null;

				StringBuilder footercolumns = null;
				if (this.chkGrandTotals.Checked)
					footercolumns = 
						new StringBuilder("<Footer><TableRows><TableRow><Height>12 pt</Height><TableCells>");

				bool bHaveFooter=false;		// indicates one or more columns have been checked for subtotaling
				foreach (string colname in this.lbTableColumns.Items)
				{
					tablecolumns.Append("<TableColumn><Width>1in</Width></TableColumn>");
					headercolumns.AppendFormat("<TableCell><ReportItems><Textbox><Value>{0}</Value>"+
                      "<Style><TextAlign>Center</TextAlign><BorderStyle><Default>Solid</Default></BorderStyle>"+
                      "<FontWeight>Bold</FontWeight></Style>"+
						"</Textbox></ReportItems></TableCell>", colname);
					string dcol;
					string gcol;
					if (gname == colname)
					{
						dcol = "";
						gcol = string.Format("=Fields!{0}.Value", colname);
					}
					else
					{
						gcol = "";
						dcol = string.Format("=Fields!{0}.Value", colname);
					}
					int iChecked = this.lbTableColumns.CheckedItems.IndexOf(colname);
					string fcol="";
					if (iChecked >= 0)
					{
						bHaveFooter = true;
						fcol = string.Format("=Sum(Fields!{0}.Value)", colname);
					}
					if (tablegroups != null)
					{
						tablegroups.AppendFormat("<TableCell><ReportItems><Textbox>"+
							"<Value>{0}</Value><CanGrow>true</CanGrow>"+
							"<Style><BorderStyle><Default>Solid</Default></BorderStyle>"+
							"</Style></Textbox></ReportItems></TableCell>", gcol);
						footergroup.AppendFormat("<TableCell><ReportItems><Textbox>"+
							"<Value>{0}</Value><CanGrow>true</CanGrow>"+
							"<Style><BorderStyle><Default>Solid</Default></BorderStyle>"+
							"</Style></Textbox></ReportItems></TableCell>", fcol);
					}
					detailcolumns.AppendFormat("<TableCell><ReportItems><Textbox>"+
                      "<Value>{0}</Value><CanGrow>true</CanGrow>"+
                      "<Style><BorderStyle><Default>Solid</Default></BorderStyle>"+
                      "</Style></Textbox></ReportItems></TableCell>", dcol);
					if (footercolumns != null)
						footercolumns.AppendFormat("<TableCell><ReportItems><Textbox>"+
							"<Value>{0}</Value><CanGrow>true</CanGrow>"+
							"<Style><BorderStyle><Default>Solid</Default></BorderStyle>"+
							"</Style></Textbox></ReportItems></TableCell>", fcol);
				}
				tablecolumns.Append("</TableColumns>");
				table.Append(tablecolumns.ToString());
				headercolumns.Append("</TableCells></TableRow></TableRows>"+
					"<RepeatOnNewPage>true</RepeatOnNewPage></Header>");
				table.Append(headercolumns.ToString());
				detailcolumns.Append("</TableCells></TableRow></TableRows>"+
					"</Details>");
				table.Append(detailcolumns.ToString());
				if (footercolumns != null)
				{
					footercolumns.Append("</TableCells></TableRow></TableRows>"+
						"</Footer>");
					table.Append(footercolumns.ToString());
				}
				if (tablegroups != null)
				{
					tablegroups.Append("</TableCells></TableRow></TableRows>"+
						"</Header>");
					if (bHaveFooter)
					{
						footergroup.Append("</TableCells></TableRow></TableRows>"+
							"</Footer>");
						tablegroups.Append(footergroup.ToString());
					}
					tablegroups.Append("</TableGroup></TableGroups>");
					table.Append(tablegroups);
				}
				table.Append("</Table>");

				return table.ToString();
			}
		}

		private string TableXmlVert
		{
			get 
			{
				StringBuilder table = new StringBuilder("<Table>");
				table.AppendFormat("<DataSetName>{0}</DataSetName>", this.cbDataSets.Text);
				table.Append("<NoRows>Query returned no rows!</NoRows><Style>"+
					"<BorderStyle><Default>Solid</Default></BorderStyle></Style>");

				table.Append("<TableColumns><TableColumn><Width>5in</Width></TableColumn></TableColumns>");

				table.Append("<Details><TableRows>"+ Environment.NewLine);

				foreach (string colname in this.lbTableColumns.Items)
				{
					string dcol = string.Format("Fields!{0}.Value", colname);

					if (this.rbVertComp.Checked)
					{
						string val = String.Format("<Value>=\"&lt;span style='color:Crimson;'&gt;{0}:&amp;nbsp;&amp;nbsp;&lt;/span&gt;\" &amp; {1}</Value>", colname, dcol);
						table.AppendFormat(
							"<TableRow><Height>12 pt</Height>"+
							"<Visibility><Hidden>=Iif({1} = Nothing, true, false)</Hidden></Visibility>"+
							"<TableCells><TableCell><ReportItems><Textbox>"+
							"{0}"+
							"<CanGrow>true</CanGrow>"+
							"<Style><BorderStyle><Default>None</Default></BorderStyle>"+
							"<Format>html</Format>"+
							"</Style></Textbox></ReportItems></TableCell>"+
							"</TableCells></TableRow>"+
							Environment.NewLine, val, dcol);
					}
					else
					{
						table.AppendFormat(
							"<TableRow><Height>12 pt</Height><TableCells>"+
							"<TableCell><ReportItems><Textbox>"+
							"<Value>{0}</Value>"+
							"<Style><BorderStyle><Default>None</Default></BorderStyle>"+
							"<FontWeight>Bold</FontWeight>"+
							"<Color>Crimson</Color>"+
							"</Style></Textbox></ReportItems></TableCell>"+
							"</TableCells></TableRow>", colname);

						table.AppendFormat(
							"<TableRow><Height>12 pt</Height><TableCells>"+
							"<TableCell><ReportItems><Textbox>"+
							"<Value>={0}</Value><CanGrow>true</CanGrow>"+
							"<Style><BorderStyle><Default>None</Default></BorderStyle>"+
							"</Style></Textbox></ReportItems></TableCell>"+
							"</TableCells></TableRow>", dcol);
					}
				}
				table.Append("</TableRows></Details></Table>");

				return table.ToString();
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.bOK = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.cbDataSets = new System.Windows.Forms.ComboBox();
            this.lbFields = new System.Windows.Forms.ListBox();
            this.lbTableColumns = new System.Windows.Forms.CheckedListBox();
            this.bUp = new System.Windows.Forms.Button();
            this.bDown = new System.Windows.Forms.Button();
            this.bRight = new System.Windows.Forms.Button();
            this.bAllRight = new System.Windows.Forms.Button();
            this.bLeft = new System.Windows.Forms.Button();
            this.bAllLeft = new System.Windows.Forms.Button();
            this.cbGroupColumn = new System.Windows.Forms.ComboBox();
            this.chkGrandTotals = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbVertComp = new System.Windows.Forms.RadioButton();
            this.rbVert = new System.Windows.Forms.RadioButton();
            this.rbHorz = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // bOK
            // 
            this.bOK.Location = new System.Drawing.Point(326, 336);
            this.bOK.Name = "bOK";
            this.bOK.Size = new System.Drawing.Size(90, 25);
            this.bOK.TabIndex = 12;
            this.bOK.Text = "确定(&O)";
            this.bOK.Click += new System.EventHandler(this.bOK_Click);
            // 
            // bCancel
            // 
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Location = new System.Drawing.Point(442, 336);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(90, 25);
            this.bCancel.TabIndex = 13;
            this.bCancel.Text = "取消(&C)";
            // 
            // cbDataSets
            // 
            this.cbDataSets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDataSets.Location = new System.Drawing.Point(68, 17);
            this.cbDataSets.Name = "cbDataSets";
            this.cbDataSets.Size = new System.Drawing.Size(460, 20);
            this.cbDataSets.TabIndex = 0;
            this.cbDataSets.SelectedIndexChanged += new System.EventHandler(this.cbDataSets_SelectedIndexChanged);
            // 
            // lbFields
            // 
            this.lbFields.ItemHeight = 12;
            this.lbFields.Location = new System.Drawing.Point(19, 121);
            this.lbFields.Name = "lbFields";
            this.lbFields.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbFields.Size = new System.Drawing.Size(183, 124);
            this.lbFields.TabIndex = 2;
            // 
            // lbTableColumns
            // 
            this.lbTableColumns.Location = new System.Drawing.Point(278, 121);
            this.lbTableColumns.Name = "lbTableColumns";
            this.lbTableColumns.Size = new System.Drawing.Size(183, 132);
            this.lbTableColumns.TabIndex = 7;
            // 
            // bUp
            // 
            this.bUp.Location = new System.Drawing.Point(470, 153);
            this.bUp.Name = "bUp";
            this.bUp.Size = new System.Drawing.Size(58, 26);
            this.bUp.TabIndex = 8;
            this.bUp.Text = "Up";
            this.bUp.Click += new System.EventHandler(this.bUp_Click);
            // 
            // bDown
            // 
            this.bDown.Location = new System.Drawing.Point(470, 188);
            this.bDown.Name = "bDown";
            this.bDown.Size = new System.Drawing.Size(58, 26);
            this.bDown.TabIndex = 9;
            this.bDown.Text = "Down";
            this.bDown.Click += new System.EventHandler(this.bDown_Click);
            // 
            // bRight
            // 
            this.bRight.Location = new System.Drawing.Point(221, 129);
            this.bRight.Name = "bRight";
            this.bRight.Size = new System.Drawing.Size(38, 26);
            this.bRight.TabIndex = 3;
            this.bRight.Text = ">";
            this.bRight.Click += new System.EventHandler(this.bRight_Click);
            // 
            // bAllRight
            // 
            this.bAllRight.Location = new System.Drawing.Point(221, 164);
            this.bAllRight.Name = "bAllRight";
            this.bAllRight.Size = new System.Drawing.Size(38, 26);
            this.bAllRight.TabIndex = 4;
            this.bAllRight.Text = ">>";
            this.bAllRight.Click += new System.EventHandler(this.bAllRight_Click);
            // 
            // bLeft
            // 
            this.bLeft.Location = new System.Drawing.Point(221, 198);
            this.bLeft.Name = "bLeft";
            this.bLeft.Size = new System.Drawing.Size(38, 26);
            this.bLeft.TabIndex = 5;
            this.bLeft.Text = "<";
            this.bLeft.Click += new System.EventHandler(this.bLeft_Click);
            // 
            // bAllLeft
            // 
            this.bAllLeft.Location = new System.Drawing.Point(221, 233);
            this.bAllLeft.Name = "bAllLeft";
            this.bAllLeft.Size = new System.Drawing.Size(38, 25);
            this.bAllLeft.TabIndex = 6;
            this.bAllLeft.Text = "<<";
            this.bAllLeft.Click += new System.EventHandler(this.bAllLeft_Click);
            // 
            // cbGroupColumn
            // 
            this.cbGroupColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGroupColumn.Location = new System.Drawing.Point(19, 302);
            this.cbGroupColumn.Name = "cbGroupColumn";
            this.cbGroupColumn.Size = new System.Drawing.Size(202, 20);
            this.cbGroupColumn.TabIndex = 10;
            this.cbGroupColumn.Enter += new System.EventHandler(this.cbGroupColumn_Enter);
            // 
            // chkGrandTotals
            // 
            this.chkGrandTotals.Location = new System.Drawing.Point(278, 302);
            this.chkGrandTotals.Name = "chkGrandTotals";
            this.chkGrandTotals.Size = new System.Drawing.Size(106, 20);
            this.chkGrandTotals.TabIndex = 11;
            this.chkGrandTotals.Text = "计算全部汇总";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbVertComp);
            this.groupBox1.Controls.Add(this.rbVert);
            this.groupBox1.Controls.Add(this.rbHorz);
            this.groupBox1.Location = new System.Drawing.Point(19, 43);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(509, 43);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "字段排列";
            // 
            // rbVertComp
            // 
            this.rbVertComp.Location = new System.Drawing.Point(355, 17);
            this.rbVertComp.Name = "rbVertComp";
            this.rbVertComp.Size = new System.Drawing.Size(135, 17);
            this.rbVertComp.TabIndex = 2;
            this.rbVertComp.Text = "Down (compress)";
            // 
            // rbVert
            // 
            this.rbVert.Location = new System.Drawing.Point(192, 17);
            this.rbVert.Name = "rbVert";
            this.rbVert.Size = new System.Drawing.Size(144, 17);
            this.rbVert.TabIndex = 1;
            this.rbVert.Text = "Down (row per field)";
            // 
            // rbHorz
            // 
            this.rbHorz.Location = new System.Drawing.Point(10, 17);
            this.rbHorz.Name = "rbHorz";
            this.rbHorz.Size = new System.Drawing.Size(192, 17);
            this.rbHorz.TabIndex = 0;
            this.rbHorz.Text = "Across (standard columns)";
            this.rbHorz.CheckedChanged += new System.EventHandler(this.rbHorz_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(278, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "表列：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "数据字段：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "数据集：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 284);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 1;
            this.label4.Text = "选择分组列：";
            // 
            // DialogNewTable
            // 
            this.AcceptButton = this.bOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.bCancel;
            this.ClientSize = new System.Drawing.Size(550, 372);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.chkGrandTotals);
            this.Controls.Add(this.cbGroupColumn);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.bAllLeft);
            this.Controls.Add(this.bLeft);
            this.Controls.Add(this.bAllRight);
            this.Controls.Add(this.bRight);
            this.Controls.Add(this.bDown);
            this.Controls.Add(this.bUp);
            this.Controls.Add(this.lbTableColumns);
            this.Controls.Add(this.lbFields);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbDataSets);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.bOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DialogNewTable";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "插入表格";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
		public void Apply()
		{
			//
		}

		private void bOK_Click(object sender, System.EventArgs e)
		{
			// apply the result
			Apply();
			DialogResult = DialogResult.OK;
		}

		private void cbDataSets_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			this.lbTableColumns.Items.Clear();
			bOK.Enabled = false;
			this.lbFields.Items.Clear();
			string [] fields = _Draw.GetFields(cbDataSets.Text, false);
			if (fields != null)
				lbFields.Items.AddRange(fields);
		}

		private void bRight_Click(object sender, System.EventArgs e)
		{
			ListBox.SelectedIndexCollection sic = lbFields.SelectedIndices;
			int count=sic.Count;
			foreach (int i in sic)
			{
				string fname = (string) lbFields.Items[i];
				lbTableColumns.Items.Add(fname);
			}
			// Need to remove backwards
			ArrayList ar = new ArrayList(sic);
			ar.Reverse();
			foreach (int i in ar)
			{
				lbFields.Items.RemoveAt(i);
			}
			bOK.Enabled = lbTableColumns.Items.Count > 0;
			if (count > 0 && lbFields.Items.Count > 0)
				lbFields.SelectedIndex = 0;
		}

		private void bLeft_Click(object sender, System.EventArgs e)
		{
			ICollection sic = lbTableColumns.SelectedIndices;
			int count = sic.Count;
			foreach (int i in sic)
			{
				string fname = (string) lbTableColumns.Items[i];
				lbFields.Items.Add(fname);
				if (fname == this.cbGroupColumn.Text)
					this.cbGroupColumn.Text = "";
			}
			// Need to remove backwards
			ArrayList ar = new ArrayList(sic);
			ar.Reverse();
			foreach (int i in ar)
			{
				lbTableColumns.Items.RemoveAt(i);
			}
			bOK.Enabled = lbTableColumns.Items.Count > 0;
			if (count > 0 && lbTableColumns.Items.Count > 0)
				lbTableColumns.SelectedIndex = 0;
		}

		private void bAllRight_Click(object sender, System.EventArgs e)
		{
			foreach (object fname in lbFields.Items)
			{
				lbTableColumns.Items.Add(fname);
			}
			lbFields.Items.Clear();		
			bOK.Enabled = lbTableColumns.Items.Count > 0;
		}

		private void bAllLeft_Click(object sender, System.EventArgs e)
		{
			foreach (object fname in lbTableColumns.Items)
			{
				lbFields.Items.Add(fname);
			}
			lbTableColumns.Items.Clear();		
			this.cbGroupColumn.Text = "";
			bOK.Enabled = false;
		}

		private void bUp_Click(object sender, System.EventArgs e)
		{
			int index = lbTableColumns.SelectedIndex;
			if (index <= 0)
				return;

			string prename = (string) lbTableColumns.Items[index-1];
			lbTableColumns.Items.RemoveAt(index-1);
			lbTableColumns.Items.Insert(index, prename);
		}

		private void bDown_Click(object sender, System.EventArgs e)
		{
			int index = lbTableColumns.SelectedIndex;
			if (index < 0 || index + 1 == lbTableColumns.Items.Count)
				return;

			string postname = (string) lbTableColumns.Items[index+1];
			lbTableColumns.Items.RemoveAt(index+1);
			lbTableColumns.Items.Insert(index, postname);
		}

		private void cbGroupColumn_Enter(object sender, System.EventArgs e)
		{
			cbGroupColumn.Items.Clear();
			cbGroupColumn.Items.Add("");
			if (lbTableColumns.Items.Count > 0)
			{
				object[] names = new object[lbTableColumns.Items.Count];
				lbTableColumns.Items.CopyTo(names, 0);
				cbGroupColumn.Items.AddRange(names);
			}
		}

		private void rbHorz_CheckedChanged(object sender, System.EventArgs e)
		{
			// only standard column report supports grouping and totals
			this.cbGroupColumn.Enabled = this.chkGrandTotals.Enabled = rbHorz.Checked;
		}
	}
}
