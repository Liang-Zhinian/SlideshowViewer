using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using SlideshowViewer.FileGroup;
using SlideshowViewer.Properties;
using SlideshowViewer.ResumeManager;

namespace SlideshowViewer
{
    internal static class Program
    {
        private static DirectoryTree _directoryTreeForm;
        private static readonly RootFileGroup _root = new RootFileGroup();

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs eventArgs)
                {
                    MessageBox.Show("Exception:\n" + eventArgs.ExceptionObject, "Error", MessageBoxButtons.OK,
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
            var errorList=new List<string>();
            errorList.AddRange(ReadDefaultConfig());
            errorList.AddRange(ReadConfiguration(args));
            _root.StartScan();
            _directoryTreeForm.SetRoot(_root);
            _directoryTreeForm.SetErrors(errorList);
            splashScreen.Dispose();
            _directoryTreeForm.Run();
        }


        private static IEnumerable<string> ReadDefaultConfig()
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
            return ReadConfiguration(File.ReadLines(defaultConfig));
        }


        private static IEnumerable<string> ReadConfiguration(IEnumerable<string> args)
        {
            List<string> errorsList=new List<string>();
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
                        case "load":
                            if (!_root.Add(value))
                                errorsList.Add("Can not load " + value);
                            break;
                        case "commandfile":
                            ReadConfiguration(File.ReadLines(value));
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
                else if (!_root.Add(arg))
                {
                    if (Path.GetFileName(arg).MatchGlob("*.ssv"))
                        ReadConfiguration(File.ReadLines(arg));
                    else 
                        errorsList.Add("Can not load " + arg);
                }
            }
            return errorsList;
        }
    }
}