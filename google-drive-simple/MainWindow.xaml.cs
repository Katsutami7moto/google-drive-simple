using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
// using System.Windows.Shapes;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;

namespace google_drive_simple
{
    public partial class MainWindow : Window
    {
        private FolderBrowserDialog uploadFolderBrowserDialog = new FolderBrowserDialog();
        string uploadFolderPath = "";

        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "gdrive-simple";

        Dictionary<string, string> gdFolders = new Dictionary<string, string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            uploadFolderBrowserDialog.ShowDialog();
            uploadFolderPath = uploadFolderBrowserDialog.SelectedPath;
            System.Windows.MessageBox.Show(uploadFolderPath);
        }

        private void gdAuthButton_Click(object sender, RoutedEventArgs e)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            
            FilesResource.ListRequest listRequest = service.Files.List();

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> folders = listRequest.Execute().Files;
            bool foldersFlag = false;
            if (folders != null && folders.Count > 0)
            {
                foreach (var file in folders)
                {
                    if (file.MimeType == "application/vnd.google-apps.folder")
                    {
                        foldersFlag = true;
                        gdFolders.Add(file.Id, file.Name);
                        
                        gdFoldersListBox.ItemsSource = gdFolders;
                        gdFoldersListBox.DisplayMemberPath = "value";
                        gdFoldersListBox.SelectedValuePath = "key";
                        gdFoldersListBox.SelectionChanged += new SelectionChangedEventHandler(gdFoldersListBox_SelectedValueChanged);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("The Drive is empty!");
            }
            if (!foldersFlag)
            {
                System.Windows.MessageBox.Show("There is no folders in this Drive!");
            }
        }

        private void gdFoldersListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (gdFoldersListBox.SelectedIndex != -1)
            {
                System.Windows.MessageBox.Show(gdFoldersListBox.SelectedValue.ToString());
                // If we also wanted to get the displayed text we could use
                // the SelectedItem item property:
                // string s = ((USState)ListBox1.SelectedItem).LongName;
            }
        }
    }
}
