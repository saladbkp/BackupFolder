using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using System.Windows.Shapes;

namespace Backup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        static string ServerPath ="";
        static string LocalPath = "";
        static string BackupPath = "";
        public MainWindow()
        {
            InitializeComponent();
            readPathName();
            Init();
        }

         private void Init()
        {
            List<File_c> filelist_left = ListFiles(LocalPath);
            List<File_c> filelist_right = ListFiles(ServerPath);

            dg1.ItemsSource = filelist_left;
            dg2.ItemsSource = filelist_right;
            dg3.ItemsSource = GetCompares(filelist_left, filelist_right);
        }

        public class File_c
        {
            public string Path { get; set; }
            public string Name { get; set; }
            public string Size { get; set; }
            public string Time { get; set; }
        }
        public class Compare
        {
            public string Name { get; set; }
            public string Changed { get; set; }
            public string Time { get; set; }
        }
        private List<File_c> ListFiles(string filepath)
        {
            List<File_c> filelist = new List<File_c>();

            string[] allfiles = Directory.GetFiles(filepath, "*.*", SearchOption.AllDirectories);
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                filelist.Add(new File_c() { 
                    Path = info.FullName,
                    Name = info.Name,
                    Size = info.Length.ToString(),
                    Time = info.CreationTime.ToString(),
                });
                // Do something with the Folder or just add them to a list via nameoflist.add();
            }
            return filelist;
        }


        private List<Compare> GetCompares(List<File_c> files_left, List<File_c> files_right)
        {
            List<Compare> comparelist = new List<Compare>();

            foreach (var item in files_right)
            {
                if (!files_left.Any(x => x.Name.Equals(item.Name)))
                {
                    comparelist.Add(new Compare() { Name = item.Name, Changed = "New Created", Time = item.Time });

                }
                else
                {
                    if (!files_left.Any(i => i.Size.Equals(item.Size)))
                    {
                        comparelist.Add(new Compare() { Name = item.Name, Changed = "File Modified", Time = item.Time });

                    }
                }

            }
            foreach (var item in files_left)
            {
                if (!files_right.Any(x => x.Name.Equals(item.Name)))
                {
                    comparelist.Add(new Compare() { Name = item.Name, Changed = "File Deleted", Time = item.Time });

                }

            }
            return comparelist;
        }

        private void sync_Click(object sender, RoutedEventArgs e)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(ServerPath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(ServerPath, LocalPath));
                Directory.CreateDirectory(dirPath.Replace(ServerPath, BackupPath));

            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(ServerPath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(ServerPath, LocalPath), true);
                File.Copy(newPath, newPath.Replace(ServerPath, BackupPath), true);          
            }
            save_Log(LocalPath);
            save_Log(BackupPath);
            MessageBox.Show("Backup Done");
            Init();
        }

        private void save_Log(string path)
        {
            TextWriter writer = new StreamWriter(path + "/Log.txt", true);
            string synctime = DateTime.Now.ToString();
            foreach(Compare dataRow in dg3.ItemsSource)
            {
                string line = dataRow.Name + "\t|\t" + dataRow.Changed + "\t|\t" + dataRow.Time + "\t|\t" +synctime;
                //MessageBox.Show(line);
                writer.WriteLine(line);
            }
            writer.Close();

        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            save_Log(BackupPath);
            MessageBox.Show("Log Written");

        }

        private void readPathName()
        {
            var fileStream = new FileStream("./BackupAddress.txt", FileMode.Open, FileAccess.Read);
            List<string> pathlist = new List<string>();
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    line = line.Split(new char[] { '=', }).Last();
                    line = line.Trim();
                    pathlist.Add(line);
                }
            }
            ServerPath = pathlist[0];
            LocalPath = pathlist[1];
            BackupPath = pathlist[2];
        }
    }

}
