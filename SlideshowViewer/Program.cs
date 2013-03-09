using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SlideshowViewer
{
    internal static class Program
    {
        private static List<string> _fileNames = new List<string>();
        private static int _delayInSec = 15;
        private static bool _loop = true;
        private static string _text = "{fullName}";
        private static bool _shuffle=false;
        private static string _resumeFile;
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
                _filenamepatterns = ImageCodecInfo.GetImageEncoders().SelectMany(info => info.FilenameExtension.Split(';'));
                var pictureViewerForm = new PictureViewerForm();
                ReadConfiguration(args);
                var shownFiles = new List<String>();
                if (_resumeFile != null && File.Exists(_resumeFile))
                {
                    foreach (var shownFile in File.ReadLines(_resumeFile))
                    {
                        if (_fileNames.RemoveAll(s => s==shownFile)>0)
                        {
                            shownFiles.Add(shownFile);
                        }
                    }
                    if (_fileNames.Count == 0)
                    {
                        _fileNames = shownFiles;
                        shownFiles=new List<string>();
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
                pictureViewerForm.Files = new List<PictureFile>(allfiles.Select(s => new PictureFile(s)));
                pictureViewerForm.FileIndex = shownFiles.Count;
                pictureViewerForm.Loop = _loop;
                pictureViewerForm.ResumeFile = _resumeFile;
                pictureViewerForm.DelayInSec = _delayInSec;
                pictureViewerForm.OverlayTextTemplate = _text;
                Application.Run(pictureViewerForm);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show("Error:\n" + e, "Error showing slideshow", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static List<string> Shuffle(List<string> fileNames)
        {
            var list = fileNames.ToList();
            List<string> ret=new List<string>();
            Random r=new Random();
            while (list.Count > 0)
            {
                var next = r.Next(list.Count);
                ret.Add(list[next]);
                list.RemoveAt(next);
            }
            return ret;

        }

        private static void ReadConfiguration(IEnumerable<string> args)
        {
            foreach (string arg in args)
            {
                var trimmed=arg.TrimStart();
                if (trimmed.Length==0 || trimmed.StartsWith("#"))
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
                            ScanRecursive(value);
                            break;
                        case "text":
                            _text = value;
                            break;
                        case "shuffle":
                            _shuffle = Convert.ToBoolean(value);
                            break;
                        case "resumefile":
                            _resumeFile = value;
                            break;
                        default:
                            throw new ApplicationException("Unknown command " + cmd);
                    }
                }
                else
                {
                    throw new ApplicationException("Not an command " + arg);
                }
            }
        }

        private static void ScanRecursive(string dirName)
        {
            _fileNames.AddRange(_filenamepatterns.SelectMany(pattern => Directory.GetFiles(dirName, pattern)));
            foreach (string directory in Directory.GetDirectories(dirName))
            {
                if (Path.GetFileName(directory)!=".picasaoriginals")
                    ScanRecursive(directory);
            }
        }

        private static void ReadNamesFromFile(string arg)
        {
            _fileNames.AddRange(File.ReadAllLines(arg).Where(s => s.Trim().Length > 0));
        }
    }
}