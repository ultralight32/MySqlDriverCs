using System;
using System.Data;
using System.Reflection;
using System.IO;
using MySQLDriverCS;
using System.Runtime.InteropServices;

namespace MySQLTest
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Class1
	{
		static int count =10;

		static void Main(string[] args)
		{
			try
			{
				long t1, t2, t3, t4, t5, t6;
				int affectedRows;
				Class1 test = new Class1();

				MySQLConnection con = new MySQLConnection("Data Source=mysql;Location=localhost;User ID=root;Password=root");
				bool usenew=true, useold=true;
				if (args!=null && args.Length>0)
				{
					usenew = args[0]=="new";
					useold = args[0]=="old";
				}

				test.CreateTable(con);
				test.Delete(con);
				//--------------------------------Direct statements-------------------
				if(useold)
				{
					t1 = DateTime.Now.Ticks;
					affectedRows= test.InsertTest(con, false);
					t2 = DateTime.Now.Ticks;
					PrintTimeDiff("Insert of " + affectedRows + " rows with DIRECT statement",t2-t1);

					affectedRows= test.TestSelect(con, false);
					t5 = DateTime.Now.Ticks;
					PrintTimeDiff("Select of " + affectedRows + " rows with DIRECT statement",t5-t2);
				}

				//--------------------------------Prepared statements-------------------
				if (usenew)
				{
					t3 = DateTime.Now.Ticks;
					affectedRows=test.InsertTest(con, true);
					t4 = DateTime.Now.Ticks;
					PrintTimeDiff("Insert of " + affectedRows + " rows with PREPARED statement",t4-t3);

					affectedRows=test.TestSelect(con, true);
					t6 = DateTime.Now.Ticks;
					PrintTimeDiff("Select of " + affectedRows + " rows with PREPARED statement",t6-t4);
				}
				Console.WriteLine("End of the Test, Enter to exit");
				Console.ReadLine();
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message + e.StackTrace);
				Console.ReadLine();
			}
		}

		public struct test
		{
			public IntPtr length;
		}

		public void release(test t)
		{
			if (t.length!=IntPtr.Zero)
			{
				Marshal.FreeHGlobal(t.length);
			}
		}

		public void create(test t)
		{
			t.length = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(uint)));
		}

		public static void PrintTimeDiff(string msg, long diff)
		{
			Console.WriteLine("Total Time " + msg +":"+ TimeSpan.FromTicks(diff).Minutes + ":"   + TimeSpan.FromTicks(diff).Seconds + ":" + TimeSpan.FromTicks(diff).Milliseconds);
		}

		public void CreateTable(MySQLConnection con)
		{
			try
			{
				con.Open();
				MySQLCommand cmd = new MySQLCommand("DROP TABLE IF EXISTS test_table", con);
				cmd.ExecuteNonQuery();
				cmd.Dispose();
				cmd = new MySQLCommand("CREATE TABLE test_table(col1 INT,col2 VARCHAR(40),col3 SMALLINT,col4 TIMESTAMP)", con);
				cmd.ExecuteNonQuery();
				cmd.Dispose();
				con.Close();
			}
			catch(Exception e)
			{
				if (con!=null) con.Close();
				throw e;
			}

		}

		public int InsertTest(MySQLConnection con, bool useNew)
		{
			try
			{
				con.Open();

				MySQLCommand cmd;
				if (useNew)
				{
					cmd = new MySQLCommand("INSERT INTO `test_table` (col1, col2, col3, col4) values (?,?,?,?)", con);//, `SerialNumberLastUsed`
					cmd.UsePreparedStatement=true;
					cmd.ServerCursor = false;
					cmd.Prepare();
				}
				else
				{
					cmd = new MySQLCommand("INSERT INTO `test_table`(col1, col2, col3, col4) values (@col1, @col2, @col3, @col4)", con);//, `SerialNumberLastUsed`
				}

				MySQLParameter p1 = new MySQLParameter("@col1", DbType.Int32);
				cmd.Parameters.Add(p1);
				MySQLParameter p2 = new MySQLParameter("@col2", DbType.String);
				cmd.Parameters.Add(p2);
				MySQLParameter p3 = new MySQLParameter("@col1", DbType.Int16);
				cmd.Parameters.Add(p3);
				MySQLParameter p4 = new MySQLParameter("@col4", DbType.DateTime);
				cmd.Parameters.Add(p4);
				int affectedrows=0;
				for (int i=0; i<count; i++)
				{
					p4.Value=DateTime.Now;
					p1.Value=i;
					p2.Value="Registro número " + i;
					p2.Size = p2.Value.ToString().Length;
					p3.Value=i*10;
					affectedrows += cmd.ExecuteNonQuery();
				}
				cmd.Dispose();
				con.Close();
				return affectedrows;
			}
			catch(Exception e)
			{
				if (con!=null) con.Close();
				throw e;
			}
		}

		public void Delete(MySQLConnection con)
		{
			try
			{
				con.Open();

				MySQLCommand cmd = new MySQLCommand("DELETE FROM `test_table`", con);//, `SerialNumberLastUsed`
				cmd.ExecuteNonQuery();
				cmd.Dispose();
				con.Close();
			}
			catch(Exception e)
			{
				if (con!=null) con.Close();
				throw e;
			}
		}

		public int TestSelect(MySQLConnection con, bool useNew)
		{
			int affectedrows=0;
			try
			{
				con.Open();

				MySQLCommand cmd = new MySQLCommand("SELECT col1, col2, col3, col4 FROM `test_table`", con);
				if (useNew)
				{
					cmd.UsePreparedStatement=true;
					cmd.ServerCursor = true;
					cmd.Prepare();
				}
				IDataReader reader= cmd.ExecuteReader();
				MySQLCommand cmd1 = new MySQLCommand("SELECT col1, col2, col3, col4 FROM `test_table`", con);
				if (useNew)
				{
					cmd1.UsePreparedStatement=true;
					//cmd.ServerCursor = true;
					cmd.FetchSize = 10;
					cmd1.Prepare();
				}
				while (reader.Read())
				{

					affectedrows++;
					IDataReader reader1= cmd1.ExecuteReader();
					while (reader1.Read())
					{
						affectedrows++;
						long s1  =reader1.GetInt64(0);
						string st1 =reader1.GetString(1);
						DateTime dt1 = reader1.GetDateTime(3);
					}
					reader1.Close();
					
					long s  =reader.GetInt64(0);
					string st =reader.GetString(1);
					DateTime dt = reader.GetDateTime(3);
				}
				reader.Close();
				cmd.Dispose();
				con.Close();
				return affectedrows;
			}
			catch(Exception e)
			{
				if (con!=null) con.Close();
				throw e;
			}
		}


		#region Utilities
		protected static byte[] GetBinary(string fileName)
		{
			try
			{
				if	(fileName != null && fileName.Trim()!="")
				{
						
					FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
					BinaryReader br = new BinaryReader(fs);
					byte[] binary = br.ReadBytes((int)fs.Length);

					br.Close();
					fs.Close();

					return binary;
				}
				else
				{
					return new byte[0];
				}

			}
			catch (IOException)
			{
				throw new Exception("Can't find BLOB file " + fileName);			
			}

		}

		public string getBLOBFile(string fileName, MySQLDataReader reader, int fieldnumber)
		{
			FileStream fs;                          
			BinaryWriter bw;                        
			int bufferSize = 4096;                   
			byte[] outbyte = new byte[bufferSize];  
			long retval;                            
			long startIndex = 0;                    
			try
			{
				// Reset the starting byte for the new BLOB.
				startIndex = 0;

				retval = reader.GetBytes(fieldnumber, startIndex, outbyte, 0, bufferSize);

				if (retval==0)
				{
					return "";
				}

				fs = new FileStream(fileName,FileMode.OpenOrCreate, FileAccess.Write);
				bw = new BinaryWriter(fs);

				// Continue reading and writing while there are bytes beyond the size of the buffer.
				while (retval == bufferSize)
				{
					bw.Write(outbyte);
					bw.Flush();

					// Reposition the start index to the end of the last buffer and fill the buffer.
					startIndex += bufferSize;
					retval = reader.GetBytes( fieldnumber, startIndex, outbyte, 0, bufferSize);
				}

				// Write the remaining buffer.
				bw.Write(outbyte, 0, (int)retval);
				bw.Flush();

				// Close the output file.
				bw.Close();
				fs.Close();

				fileName = new FileInfo(fileName).FullName;
			}
			catch (IOException e)
			{
				throw e;
			}

			return fileName;

		}

		public void InsertBlob(MySQLConnection con)
		{
			try
			{
				con.Open();

				MySQLCommand cmd = new MySQLCommand("INSERT INTO `trntipos`(col1, col2) values (?, ?)", con);//, `SerialNumberLastUsed`
				MySQLParameter p1 = new MySQLParameter("@col1", DbType.Int32);
				MySQLParameter p2 = new MySQLParameter("@col2", DbType.Binary);
				p2.Value=GetBinary(@"myblob.jpg");
				p1.Value=1;
				cmd.UsePreparedStatement=true;
				cmd.Parameters.Add(p1);
				cmd.Parameters.Add(p2);
				cmd.Prepare();
				int affectedrows = cmd.ExecuteNonQuery();

				cmd.Dispose();
				con.Close();
			}
			catch(Exception e)
			{
				if (con!=null) con.Close();
				throw e;
			}
		}

		public void SelectBlob(MySQLConnection con)
		{
			try
			{
				con.Open();

				MySQLCommand cmd = new MySQLCommand("select col1, col2 from `trntipos` where col1=3", con);//, `SerialNumberLastUsed`
				cmd.UsePreparedStatement=true;
				cmd.ServerCursor=true;
				cmd.Prepare();
				MySQLDataReader reader = (MySQLDataReader)cmd.ExecuteReader();
				while (reader.Read())
				{
					getBLOBFile("output.jpg", reader, 1);
				}
				reader.Close();

				cmd.Dispose();
				con.Close();
			}
			catch(Exception e)
			{
				if (con!=null) con.Close();
				throw e;
			}
		}

		public void Test(MySQLConnection con)
		{
			try
			{
				con.Open();

				MySQLCommand cmd = new MySQLCommand("SELECT col1, col2, col3, col4 FROM `test_table`", con);//, `SerialNumberLastUsed`
				cmd.UsePreparedStatement=true;
				cmd.ServerCursor = true;
				cmd.Prepare();
					
				MySQLCommand cmd1 = new MySQLCommand("SELECT col1, col2 FROM `test_table`", con);//SerialNumberID
				IDataReader reader= cmd.ExecuteReader();
				while (reader.Read())
				{
					IDataReader reader1= cmd1.ExecuteReader();

					while (reader1.Read())
					{
						Console.Write(reader1.GetInt32(0) + " ");
						Console.WriteLine(reader1.GetString(1));
					}
					reader1.Close();

					Console.Write(reader.GetInt32(0) + " ");

					Console.WriteLine(reader.GetString(1));
					Console.WriteLine(reader.GetDateTime(3));
					Console.WriteLine(reader.IsDBNull(2));
					Console.WriteLine(reader.GetInt16(2));
					Console.WriteLine(reader.IsDBNull(0));
				}
				reader.Close();
				cmd1.Dispose();
				cmd.Dispose();
				con.Close();
			}
			catch(Exception e)
			{
				if (con!=null) con.Close();
				throw e;
			}
		}

		#endregion
	}
}

