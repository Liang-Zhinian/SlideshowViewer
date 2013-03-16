using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using SlideshowViewer.Properties;

namespace SlideshowViewer
{
    internal static class Program
    {
        private static IEnumerable<string> _filenamepatterns;
        private static PictureViewer _pictureViewer;
        private static DirectoryTree _directoryTree;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var splashScreen = new SplashScreen();
                splashScreen.Icon = Resources.image_x_generic;
                splashScreen.Show();
                Application.DoEvents();
                _pictureViewer=new PictureViewer();
                _directoryTree=new DirectoryTree(_pictureViewer);
                AddToFolderContextMenu();
                _filenamepatterns =
                    ImageCodecInfo.GetImageEncoders().SelectMany(info => info.FilenameExtension.Split(';'));
                ReadDefaultConfig();
                ReadConfiguration(args);
                splashScreen.Dispose();

                Application.Run(_directoryTree);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show("Error:\n" + e, "Error showing slideshow", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void FilesSelected(IEnumerable<PictureFile> files)
        {
            _directoryTree.BeginInvoke(new MethodInvoker(delegate
                {
                    new PictureViewer().ShowPictures(files);

                }));
        }

        private static void AddToFolderContextMenu()
        {
            string exeFileName = Process.GetCurrentProcess().MainModule.FileName.Replace(".vshost", "");
            FileAssociation.Associate(".ssv", "Nemosoft.SlideshowViewer", "Slideshow configuration", exeFileName,
                                      exeFileName);
            using (RegistryKey shell = Registry.ClassesRoot.OpenSubKey("Folder").OpenSubKey("shell", true))
            {
                using (RegistryKey ssv = shell.OpenSubKey("ssv", true) ?? shell.CreateSubKey("ssv"))
                {
                    ssv.SetValue(null, "Show Slideshow");
                    ssv.SetValue("Icon", exeFileName);
                    using (RegistryKey command = ssv.OpenSubKey("command", true) ?? ssv.CreateSubKey("command"))
                    {
                        command.SetValue(null,
                                         String.Format(@"""{0}"" shuffle=false ""%1""",
                                                       exeFileName));
                    }
                }
                using (RegistryKey ssv = shell.OpenSubKey("ssvshuffle", true) ?? shell.CreateSubKey("ssvshuffle"))
                {
                    ssv.SetValue(null, "Show Slideshow (Shuffle)");
                    ssv.SetValue("Icon", exeFileName);
                    using (RegistryKey command = ssv.OpenSubKey("command", true) ?? ssv.CreateSubKey("command"))
                    {
                        command.SetValue(null,
                                         String.Format(@"""{0}"" shuffle=true ""%1""",
                                                       exeFileName));
                    }
                }
            }
        }

        private static void ReadDefaultConfig()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                              "SSV");
            if (!System.IO.Directory.Exists(appDataPath))
                System.IO.Directory.CreateDirectory(appDataPath);
            string defaultConfig = Path.Combine(appDataPath, "defaults.ssv");
            if (!File.Exists(defaultConfig))
                using (StreamWriter streamWriter = File.CreateText(defaultConfig))
                {
                    streamWriter.Write(
                        @"
# loop={0}
# shuffle={1}
# text={2}
# delay={3}
# minSize={4}
", _pictureViewer.Loop, _pictureViewer.Shuffle, _pictureViewer.Text, _pictureViewer.DelayInSec, _directoryTree.MinFileSize);
                }
            ReadConfiguration(File.ReadLines(defaultConfig));
        }


        private static void ReadConfiguration(IEnumerable<string> args)
        {
            foreach (string arg in args)
            {
                string trimmed = arg.TrimStart();
                if (trimmed.Length == 0 || trimmed.StartsWith("#"))
                    continue;

                string[] param = trimmed.Split(new[] {'='}, 2);
                if (param.Length == 2)
                {
                    string cmd = param[0];
                    string value = param[1];
                    switch (cmd)
                    {
                        case "delay":
                            _pictureViewer.DelayInSec = Convert.ToInt32(value);
                            break;
                        case "loop":
                            _pictureViewer.Loop = Convert.ToBoolean(value);
                            break;
                        case "file":
                            _directoryTree.AddFile(new PictureFile(new FileInfo(value)));
                            break;
                        case "commandfile":
                            ReadConfiguration(File.ReadLines(value));
                            break;
                        case "scanRecursive":
                        case "scan":
                            AddFolder(value);
                            break;
                        case "text":
                            _pictureViewer.Text = value;
                            break;
                        case "shuffle":
                            _pictureViewer.Shuffle = Convert.ToBoolean(value);
                            break;
                        case "autorun":
                            _directoryTree.AutoRun = Convert.ToBoolean(value);
                            break;
                        case "resumefile":
                        case "resume":
                            _pictureViewer.ResumeManager = new FileResumeManager(value);
                            break;
                        case "minSize":
                            _directoryTree.MinFileSize = Convert.ToInt64(value);
                            break;
                        default:
                            throw new ApplicationException("Unknown command " + cmd);
                    }
                }
                else if (System.IO.Directory.Exists(arg))
                {
                    AddFolder(arg);
                }
                else if (File.Exists(arg))
                {
                    if (_filenamepatterns.Any(s => Path.GetFileName(arg).MatchGlob(s)))
                        _directoryTree.AddFile(new PictureFile(new FileInfo(arg)));
                    else if (Path.GetFileName(arg).MatchGlob("*.ssv"))
                        ReadConfiguration(File.ReadLines(arg));
                    else throw new ApplicationException("Unknown file format " + arg);
                }
                else
                {
                    throw new ApplicationException("Not an command " + arg);
                }
            }
        }

        private static void AddFolder(string s)
        {
            _directoryTree.AddBaseDir(new DirectoryInfo(s).FullName);
            IEnumerable<FileInfo> scanRecursive = ScanRecursive(s);
            foreach (FileInfo file in scanRecursive)
            {
                _directoryTree.AddFile(new PictureFile(file));
            }
        }

        private static IEnumerable<FileInfo> ScanRecursive(string dirName)
        {
            IEnumerable<FileInfo> files =
                _filenamepatterns.SelectMany(
                    pattern =>
                    new DirectoryInfo(dirName).EnumerateFiles(pattern)).Distinct(new CompareFileNames());
            foreach (FileInfo file in files)
            {
                yield return file;
            }
            foreach (string directory in System.IO.Directory.GetDirectories(dirName))
            {
                if (Path.GetFileName(directory) != ".picasaoriginals")
                    foreach (FileInfo file in ScanRecursive(directory))
                        yield return file;
            }
        }

        private class CompareFileNames : IEqualityComparer<FileInfo>
        {
            public bool Equals(FileInfo x, FileInfo y)
            {
                return x.FullName == y.FullName;
            }

            public int GetHashCode(FileInfo obj)
            {
                return obj.FullName.GetHashCode();
            }
        }
    }
}