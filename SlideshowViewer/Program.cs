using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SlideshowViewer
{
    internal static class Program
    {
        private static List<string> _fileNames = new List<string>();
        private static int _delayInSec = 15;
        private static bool _loop = true;
        private static string _text = "{imageDescription}{eol}{description}{eol}{dateTime}{eol}{model}{eol}{fullName}  ( {index} / {total} )";
        private static bool _shuffle=false;
        private static string _resumeFile;
        private static long _minSize = 0;
        private static IEnumerable<string> _filenamepatterns;

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
                splashScreen.Icon = Properties.Resources.image_x_generic;
                splashScreen.Show();
                Application.DoEvents();

                AddToFolderContextMenu();
                _filenamepatterns =
                    ImageCodecInfo.GetImageEncoders().SelectMany(info => info.FilenameExtension.Split(';'));
                ReadDefaultConfig();
                ReadConfiguration(args);
                var shownFiles = new List<String>();
                if (_resumeFile != null && File.Exists(_resumeFile))
                {
                    IEnumerable<string> filesRegisteredAsShown = File.ReadLines(_resumeFile);
                    shownFiles.AddRange(_fileNames.Intersect(filesRegisteredAsShown));
                    _fileNames.RemoveAll(s => shownFiles.Contains(s));
                    if (_fileNames.Count == 0)
                    {
                        _fileNames = shownFiles;
                        shownFiles = new List<string>();
                        File.Delete(_resumeFile);
                    }
                }

                var allfiles = new List<string>();
                if (_shuffle)
                {
                    allfiles.AddRange(Shuffle(shownFiles));
                    allfiles.AddRange(Shuffle(_fileNames));
                }
                else
                {
                    allfiles.AddRange(shownFiles);
                    allfiles.AddRange(_fileNames);
                }
                if (allfiles.Count == 0)
                {
                    MessageBox.Show("No files found", "Slideshow viewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var pictureViewerForm = new PictureViewerForm();
                pictureViewerForm.Files = new List<PictureFile>(allfiles.Select(s => new PictureFile(s)));
                pictureViewerForm.FileIndex = shownFiles.Count;
                pictureViewerForm.Loop = _loop;
                pictureViewerForm.ResumeFile = _resumeFile;
                pictureViewerForm.DelayInSec = _delayInSec;
                pictureViewerForm.OverlayTextTemplate = _text;
                pictureViewerForm.Icon = Properties.Resources.image_x_generic;
                splashScreen.Dispose();
                Application.Run(pictureViewerForm);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show("Error:\n" + e, "Error showing slideshow", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void AddToFolderContextMenu()
        {

            var exeFileName = Process.GetCurrentProcess().MainModule.FileName.Replace(".vshost", "");
            FileAssociation.Associate(".ssv","Nemosoft.SlideshowViewer","Slideshow configuration",exeFileName,exeFileName);
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
", _loop, _shuffle, _text, _delayInSec,_minSize);
                }
            ReadConfiguration(File.ReadLines(defaultConfig));
        }

        private static List<string> Shuffle(List<string> fileNames)
        {
            List<string> list = fileNames.ToList();
            var ret = new List<string>();
            var r = new Random();
            while (list.Count > 0)
            {
                int next = r.Next(list.Count);
                ret.Add(list[next]);
                list.RemoveAt(next);
            }
            return ret;
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
                        case "files":
                            ReadNamesFromFile(value);
                            break;
                        case "delay":
                            _delayInSec = Convert.ToInt32(value);
                            break;
                        case "loop":
                            _loop = Convert.ToBoolean(value);
                            break;
                        case "file":
                            _fileNames.Add(value);
                            break;
                        case "commandfile":
                            ReadConfiguration(File.ReadLines(value));
                            break;
                        case "scanRecursive":
                        case "scan":
                            ScanRecursive(value);
                            break;
                        case "text":
                            _text = value;
                            break;
                        case "shuffle":
                            _shuffle = Convert.ToBoolean(value);
                            break;
                        case "resumefile":
                        case "resume":
                            _resumeFile = value;
                            break;
                        case "minSize":
                            _minSize = Convert.ToInt64(value);
                            break;
                        default:
                            throw new ApplicationException("Unknown command " + cmd);
                    }
                }
                else if (Directory.Exists(arg))
                {
                    ScanRecursive(arg);
                }
                else if (File.Exists(arg))
                {
                    if (_filenamepatterns.Any(s => Path.GetFileName(arg).MatchGlob(s)))
                        _fileNames.Add(arg);
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


        private static void ScanRecursive(string dirName)
        {
            _fileNames.AddRange(_filenamepatterns.SelectMany(pattern => Directory.GetFiles(dirName, pattern).Where(s => _minSize==0 || new FileInfo(s).Length>_minSize)).Distinct());
            foreach (string directory in Directory.GetDirectories(dirName))
            {
                if (Path.GetFileName(directory) != ".picasaoriginals")
                    ScanRecursive(directory);
            }
        }

        private static void ReadNamesFromFile(string arg)
        {
            _fileNames.AddRange(File.ReadAllLines(arg).Where(s => s.Trim().Length > 0));
        }
    }
}