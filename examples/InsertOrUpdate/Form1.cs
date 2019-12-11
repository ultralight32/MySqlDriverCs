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
namespace Sample_Insert_Update_Delete
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
			this.components = new System.ComponentModel.Container();
			this.Size = new System.Drawing.Size(300,300);
			this.Text = "Form1";
		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			//Application.Run(new Form1());
			Program();
		}

		#region MySQL sample Insert or Update
		/// <summary>
		/// This sample uses insert, update and delete. Assumes that you have the default database 'test'.
		/// </summary>
		public static void Program()
		{
			MySQLConnection conn =  new MySQLConnection( new MySQLConnectionString("localhost","test","root","").AsString );
			MessageBox.Show("Connecting to database");
			conn.Open();
			MySQLCommand cmd;
			cmd = new MySQLDriverCS.MySQLCommand("DROP TABLE IF EXISTS test.mysqldrivercs_test",conn);
			MessageBox.Show(cmd.CommandText);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
			cmd = new MySQLDriverCS.MySQLCommand("CREATE TABLE test.mysqldrivercs_test("+
				"SettingID tinyint(3) unsigned NOT NULL auto_increment,"+
				"SettingValue text, "+
				"PRIMARY KEY  (SettingID), UNIQUE KEY SettingID(SettingID),  KEY SettingID_2 (SettingID))"+
				" TYPE=MyISAM COMMENT='MySQL test table'",conn);
			MessageBox.Show(cmd.CommandText);
			cmd.ExecuteNonQuery();
			cmd.Dispose();

			string Value = "Value";
			int SettingID = 1;

			MessageBox.Show("Inserting 1,\"Value\"");
			new MySQLInsertCommand(conn,
				new object[,] {{"SettingID",SettingID},{"SettingValue",Value}},
				"mysqldrivercs_test"
				);

			MessageBox.Show("Updating 1,\"Value2\"");
			Value = "Value2";
			new MySQLUpdateCommand(conn,
				new object[,] {{"SettingValue",Value}},
				"mysqldrivercs_test",
				new object[,] {{"SettingID","=",SettingID}},
				null
				);

			MessageBox.Show("Retrieving 1");
			DataTable dt = new MySQLSelectCommand(conn,
				new string[] {"SettingID","SettingValue"},
				new string[] {"mysqldrivercs_test"},
				new object[,] {{"SettingID","=",SettingID}},
				null,
				null
				).Table;
			string storedValue = dt.Rows[0]["SettingValue"].ToString();
			MessageBox.Show("1 -> \""+storedValue+"\"");

			MessageBox.Show("Deleting 1");
			new MySQLDeleteCommand(conn,"mysqldrivercs_test",new object[,] {{"SettingID","=",SettingID}},null);

			MessageBox.Show("Closing database");
			conn.Close();
			MessageBox.Show("Program finished successfuly!");
		}
		#endregion
	}
}
