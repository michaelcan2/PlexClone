using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using PlexClone.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Web;
using System.Diagnostics;

namespace PlexClone.Controllers{
	// [Route("/PlexClone")]
    public class PlexCloneController:Controller{
        // public static readonly IList<string> moviefiletype = new IReadOnlyCollection<string>(newList<string> {"m4v", "webm"});
        // const static List<string> moviefiletypes = new List<string>(new string[] {"m4v", "webm", "mkv", "avi", "mov", "wmv", "mp4", "m4p", "mpg"});
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        List<string> directories = new List<string>();
        
    	private PlexCloneContext _context;
        public static List<string> moviefiletypes{
            get{ return new List<string> {".m4v", ".webm", ".mkv", ".avi", ".mov", ".wmv", ".mp4", ".m4p", ".mpg"}; }
        }

    	public PlexCloneController(PlexCloneContext context){
    		_context = context;
    	}

        [HttpGet]
        [Route("")]
        public IActionResult Index(){
            ViewBag.drives = allDrives;
            System.Console.WriteLine(allDrives[0].ToString());
            return View("Index");
        }

        [HttpGet]
        [Route("/{path}")]
        public IActionResult GetDir(int path){
            // path = path.TrimEnd('!').Replace("!","/");
            // System.Console.WriteLine(path);
            // path = path.Substring(0, path.LastIndexOf('/') + 1);
            
            directories = Directory.GetDirectories(allDrives[path].ToString()).ToList();
            directories.Sort();
            ViewBag.drives = allDrives;
            ViewBag.current = directories;
            // path = path.Replace('/', '!');
            // if (path.Contains("!")){
            //     ViewBag.path = path.Substring(0, path.LastIndexOf('!') + 1);
            // } else{
            //     ViewBag.path = path;
            // }
            return View("Index");
        }

        [HttpGet]
        [Route("path/{path}")]
        public IActionResult GetNextDir(int path){
            // path = path.TrimEnd('!').Replace("!","/");
            // System.Console.WriteLine(path);
            // path = path.Substring(0, path.LastIndexOf('/') + 1);
            // List<string> directories = new List<string>();
            // ViewBag.currentdir = directories[path];
            System.Console.WriteLine(directories.Count());
            directories = Directory.GetDirectories(directories[path].ToString()).ToList();
            directories.Sort();
            ViewBag.drives = allDrives;
            ViewBag.current = directories;
            // path = path.Replace('/', '!');
            // if (path.Contains("!")){
            //     ViewBag.path = path.Substring(0, path.LastIndexOf('!') + 1);
            // } else{
            //     ViewBag.path = path;
            // }
            return View("Index");
        }
        [HttpGet]
        [Route("AddLibrary")]
        public IActionResult AddLibrary(){

        
            return View();
        }
        [HttpPost]
        [Route("AddLibrary")]
        public IActionResult AddNewLibrary(Libraries model){
            if (!Directory.Exists(model.Folder)){
                ModelState.AddModelError("Folder", "Path does not exist or you don't have permission.");
                return View("AddLibrary");
            }
            // IEnumerable<FileInfo> allfiles = model.Folder.EnumerateFiles();
            
            List<string> allfiles = Directory.GetFiles(model.Folder, "*.*", SearchOption.AllDirectories).ToList();
            allfiles = allfiles.Where(f => moviefiletypes.Contains(Path.GetExtension(f))).ToList();
            System.Console.WriteLine(allfiles.Count);
            foreach(var file in allfiles){
                //Add file to DB
                GetVideoInfo(file);
                System.Console.WriteLine(file);
                FileInfo finfo = new FileInfo(file);
                string temp = finfo.Directory.Name;
                int idx = temp.LastIndexOf(" - ");
                if(idx != -1){
                    System.Console.WriteLine("Movie Name is " + temp.Substring(0, idx));
                    System.Console.WriteLine("Year is " + temp.Substring(idx+3));
                    //search API http://www.omdbapi.com/?t=temp.Substring(0,idx)&y=temp.Substring(idx+3)&plot=full&apikey=???
                } else {
                    System.Console.WriteLine("Naming not matching");
                }
                
            }
            return RedirectToAction("AddLibrary");
        }

        private static void GetVideoInfo(string file){
            // System.Console.WriteLine(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "ffprobe.exe");
            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;
            string ffprobeName = "";
            switch (pid) 
                {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    if (System.IO.File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "ffprobe.exe")){
                        System.Console.WriteLine("ffprobe installed!");
                        ffprobeName = "ffprobe.exe";
                    } else {
                        System.Console.WriteLine("You need ffprobe!");
                        return;
                    }
                    break;
                case PlatformID.Unix:
                    if (System.IO.File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "ffprobe")){
                        System.Console.WriteLine("ffprobe installed!");
                        ffprobeName = "ffprobe";
                    } else {
                        System.Console.WriteLine("You need ffprobe!");
                        return;
                    }
                    break;
                case PlatformID.MacOSX:
                    if (System.IO.File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "ffprobe")){
                        System.Console.WriteLine("ffprobe installed!");
                        ffprobeName = "ffprobe";
                    } else {
                        System.Console.WriteLine("You need ffprobe!");
                        return;
                    }
                    break;
                default:
                    Console.WriteLine("Don't know your OS!");
                    return;
                }
            string basePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + ffprobeName;
            string filePath = file;
            string cmd = string.Format(" -v quiet -print_format json -show_format -show_streams  \"{0}\"", filePath);
            System.Console.WriteLine(basePath + cmd);
            Process proc = new Process();
            proc.StartInfo.FileName = basePath;
            proc.StartInfo.Arguments = cmd;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.UseShellExecute = false;
            if (!proc.Start())
            {
                Console.WriteLine("Error starting");
                return;
            }
            string info = proc.StandardOutput.ReadToEnd();
            System.Console.WriteLine(info);
            proc.WaitForExit();
            proc.Close();

        }
    }
    // public partial class _Directory : System.Web.UI.Page
    // {

    // }
}
