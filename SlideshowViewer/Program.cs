using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using SlideshowViewer.Properties;

namespace SlideshowViewer
{
    internal static class Program
    {
        private static DirectoryTree _directoryTreeForm;
        private static readonly FileScanner _fileScanner = new FileScanner();

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException+=delegate(object sender, UnhandledExceptionEventArgs eventArgs)
                {
                    MessageBox.Show("Exception:\n" + eventArgs.ExceptionObject.ToString(), "Error", MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                };
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var splashScreen = new SplashScreen();
            splashScreen.Icon = Resources.image_x_generic;
            splashScreen.Show();
            Application.DoEvents();
            _directoryTreeForm = new DirectoryTree();
            //AddToFolderContextMenu();

            ReadDefaultConfig();
            ReadConfiguration(args);
            RootFileGroup fileGroup = _fileScanner.GetRoot();
            fileGroup.StartScan();
            _directoryTreeForm.SetRoot(fileGroup);
            splashScreen.Dispose();
            _directoryTreeForm.Run();
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
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);
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
", _directoryTreeForm.Loop, _directoryTreeForm.Shuffle, _directoryTreeForm.OverlayText, _directoryTreeForm.DelayInSec,
                        _directoryTreeForm.MinFileSize);
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
                            _directoryTreeForm.DelayInSec = Convert.ToInt32(value);
                            break;
                        case "loop":
                            _directoryTreeForm.Loop = Convert.ToBoolean(value);
                            break;
                        case "file":
                            if (!_fileScanner.AddFile(value))
                                throw new ApplicationException("Not picture file " + value);
                            break;
                        case "commandfile":
                            ReadConfiguration(File.ReadLines(value));
                            break;
                        case "scanRecursive":
                        case "scan":
                            _fileScanner.AddFolder(value);
                            break;
                        case "text":
                            _directoryTreeForm.OverlayText = value;
                            break;
                        case "shuffle":
                            _directoryTreeForm.Shuffle = Convert.ToBoolean(value);
                            break;
                        case "browse":
                            _directoryTreeForm.Browse = Convert.ToBoolean(value);
                            break;
                        case "autorun":
                            _directoryTreeForm.AutoRun = Convert.ToBoolean(value);
                            break;
                        case "resumefile":
                        case "resume":
                            _directoryTreeForm.ResumeManager = new FileResumeManager(value);
                            break;
                        case "minSize":
                            _directoryTreeForm.MinFileSize = Convert.ToInt64(value);
                            break;
                        default:
                            throw new ApplicationException("Unknown command " + cmd);
                    }
                }
                else if (Directory.Exists(arg))
                {
                    _fileScanner.AddFolder(arg);
                }
                else if (File.Exists(arg))
                {
                    if (_fileScanner.AddFile(arg))
                    {
                    }
                    else if (Path.GetFileName(arg).MatchGlob("*.ssv"))
                        ReadConfiguration(File.ReadLines(arg));
                    else throw new ApplicationException("Unknown file format " + arg);
                }
                else
                {
                    throw new ApplicationException("File not found " + arg);
                }
            }
        }
    }
}