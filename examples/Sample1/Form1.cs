#region LICENSE
/*
	MySQLDriverCS: An C# driver for MySQL.
	Copyright (c) 2002 Manuel Lucas Viñas Livschitz.

	This file is part of MySQLDriverCS.

    MySQLDriverCS is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    MySQLDriverCS is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MySQLDriverCS; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
#endregion
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using MySQLDriverCS;
namespace Sample1
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null)
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
			this.lvData = new System.Windows.Forms.ListView();
			this.SuspendLayout();
			// 
			// lvData
			// 
			this.lvData.Location = new System.Drawing.Point(8, 16);
			this.lvData.Name = "lvData";
			this.lvData.Size = new System.Drawing.Size(272, 248);
			this.lvData.TabIndex = 0;
			this.lvData.View = System.Windows.Forms.View.List;
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.lvData});
			this.Name = "Form1";
			this.Text = "Example 1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.Run(new Form1());
		}

		private System.Windows.Forms.ListView lvData;

		#region MySQL Reader sample
		/// <summary>
		/// Reads host and user columns from mysql database.
		/// </summary>
		public void Program()
		{

			MySQLConnection conn =  new MySQLConnection( new MySQLConnectionString("localhost","mysql","root","").AsString );
			MessageBox.Show("Connecting to database");
			try
			{
				conn.Open();
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
				return;
			}

			// Typical low level reading
			MySQLCommand cmd = new MySQLCommand("select host,user from mysql.user",conn);
			MySQLDataReader reader = cmd.ExecuteReaderEx();
			while(reader.Read())
			{
				ListViewItem lvi = new ListViewItem("Low Level - Host:"+reader.GetString(0)+" , User:"+reader.GetString(1));
				lvData.Items.Add(lvi);
			}
			reader.Close();
			cmd.Dispose();

			// High level reading
			DataTable dt = new MySQLSelectCommand(conn, new string[] {"host","user"}, new string[] {"user"}, null, null, null).Table;
			foreach(DataRow row in dt.Rows)
			{
				ListViewItem lvi = new ListViewItem("High Level - Host:"+row["host"].ToString()+" , User:"+row["user"].ToString());
				lvData.Items.Add(lvi);
			}

			MessageBox.Show("Closing database");
			conn.Close();
			MessageBox.Show("Read finished successfuly!");
		}
		#endregion

		private void Form1_Load(object sender, System.EventArgs e)
		{
			Program();
		}
	}
}
