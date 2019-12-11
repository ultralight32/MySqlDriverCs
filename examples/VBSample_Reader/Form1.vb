'MySQLDriverCS: An C# driver for MySQL.
'Copyright (c) 2002 Manuel Lucas Viñas Livschitz.

'This file is part of MySQLDriverCS.

'MySQLDriverCS is free software; you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation; either version 2 of the License, or
'(at your option) any later version.

'MySQLDriverCS is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with MySQLDriverCS; if not, write to the Free Software
'Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
Imports MySQLDriverCS
Public Class Form1
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Program()
    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents lvData As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.lvData = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = New System.Windows.Forms.ColumnHeader()
        Me.SuspendLayout()
        '
        'lvData
        '
        Me.lvData.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1})
        Me.lvData.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lvData.Name = "lvData"
        Me.lvData.Size = New System.Drawing.Size(292, 273)
        Me.lvData.TabIndex = 0
        Me.lvData.View = System.Windows.Forms.View.List
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Width = 200
        '
        'Form1
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(292, 273)
        Me.Controls.AddRange(New System.Windows.Forms.Control() {Me.lvData})
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)

    End Sub

#End Region
    Public Sub Program()
        Dim conn As MySQLConnection = New MySQLConnection(New MySQLConnectionString("localhost", "mysql", "root", "").AsString)
        MessageBox.Show("Connecting to database")
        conn.Open()

        ' Typical low level reading
        Dim cmd As MySQLCommand = New MySQLCommand("select host,user from mysql.user", conn)
        Dim reader As MySQLDataReader = cmd.ExecuteReaderEx()
        While (reader.Read())
            Dim lvi As ListViewItem = New ListViewItem("Low Level - Host:" + reader.GetString(0) + " , User:" + reader.GetString(1))
            lvData.Items.Add(lvi)
        End While
        reader.Close()
        cmd.Dispose()

        ' High level reading
        Dim dt As DataTable = New MySQLSelectCommand(conn, New String() {"host", "user"}, New String() {"user"}, Nothing, Nothing, Nothing).Table
        Dim row As DataRow
        For Each row In dt.Rows
            Dim lvi As ListViewItem = New ListViewItem("High Level - Host:" + row("host").ToString() + " , User:" + row("user").ToString())
            lvData.Items.Add(lvi)
        Next

        MessageBox.Show("Closing database")
        conn.Close()
        MessageBox.Show("Read finished successfuly!")

    End Sub
End Class
