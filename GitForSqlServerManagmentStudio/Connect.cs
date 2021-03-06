// Project: GitForSqlServerManagmentStudio, File: Connect.cs
// Namespace: GitForSqlServerManagmentStudio, Class: Connect
// Path: C:\gitforsql\GitForSqlServerManagmentStudio, Author: mekonnen
// Code lines: 148, Size of file: 6.84 KB
// Creation date: 23/09/2010 13:59
// Last modified: 24/09/2010 11:12


using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GitForSqlServerManagmentStudio
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2
	{
		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
		}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_addInInstance = (AddIn)addInInst;

            IObjectExplorerService objectExplorer = ServiceCache.GetObjectExplorer();
            IObjectExplorerEventProvider provider = (IObjectExplorerEventProvider)objectExplorer.GetService(typeof(IObjectExplorerEventProvider));
            provider.SelectionChanged += new NodesChangedEventHandler(Provider_SelectionChanged);
            
        }

        private void Provider_SelectionChanged(object sender, NodesChangedEventArgs args)
        {
            Regex tableRegex = new Regex(@"^Server\[@Name='(?<Server>[^\]]*)'\]/Database\[@Name='(?<Database>[^']*)'\]/(?<ObjectType>(UserDefinedFunction|StoredProcedure|View))\[@Name='(?<Object>[^']*)' and @Schema='(?<Schema>[^']*)'\]$");

            var ob = args.ChangedNodes[0];

           
            Match match = tableRegex.Match(ob.Context);
          //  MessageBox.Show(ob.Context);

            if (match != null)
            {



                if (match.Groups["Server"].Success && match.Groups["Object"].Success && match.Groups["Schema"].Success && match.Groups["Database"].Success && match.Groups["ObjectType"].Success)
                {

                string server = match.Groups["Server"].Value;
                string obname = match.Groups["Object"].Value;
                string schema = match.Groups["Schema"].Value;
                string database = match.Groups["Database"].Value;
                string objectType = match.Groups["ObjectType"].Value;
                string connectionString = ob.Parent.Connection.ConnectionString + ";Database=" + database;


                try
                {
                    string sql = "SELECT  OBJECT_DEFINITION(OBJECT_ID('{0}.{1}'))";

                    SqlCommand command = new SqlCommand(string.Format(sql, schema, obname));
                    command.CommandType = CommandType.Text;
                    command.Connection = new SqlConnection(connectionString);
                    command.Connection.Open();

                    var defn = command.ExecuteScalar();

                    command.Connection.Close();

                    string path = string.Format("C:\\Scripts\\{0}-{1}", server, database);

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    path = path + string.Format("\\{0}", objectType);
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    File.WriteAllText(string.Format("{0}\\{1}.{2}.sql", path,  schema, obname), defn.ToString());
                } // try
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                } // catch

                } // if

               
            } // if

            
        }

        //void _provider_BufferedNodesAdded(object sender, Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer.NodesChangedEventArgs args)
        //{
        //    System.Windows.Forms.MessageBox.Show("test, on buffered node added"); 
        //}

        //void _provider_NodesAdded(object sender, Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer.NodesChangedEventArgs args)
        //{
        //    System.Windows.Forms.MessageBox.Show("test, on nodes added"); 
        //}

        //void _provider_QueryCanceled(object sender, EventArgs e)
        //{
        //    System.Windows.Forms.MessageBox.Show("test, on query canceled"); 
        //}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
		
		private AddIn _addInInstance;
	}
}