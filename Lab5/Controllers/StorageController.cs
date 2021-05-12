using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lab5.Controllers
{
    [ApiController]
    [Route("storage")]
    public class StorageController : ControllerBase
    {
        private readonly string storagePath = @"C:\Storage";
        private readonly ILogger<StorageController> _logger;

        public StorageController(ILogger<StorageController> logger)
        {
            _logger = logger;
        }

        [HttpPut("{*path}")]
        public ActionResult HTTPPut(string path)
        {
            if (path == null)
                path = "";
            try
            {
                string fullPath = Path.Combine(storagePath, path);
                bool needToCopy = Request.Headers.ContainsKey("X-Copy-From");
                int count = 0;
                if (!needToCopy)
                {
                    IFormFileCollection files = Request.Form.Files;
                    if (Directory.Exists(fullPath))
                    {
                        foreach (var file in files)
                        {
                            try
                            {
                                using (FileStream fileStream = new FileStream(Path.Combine(fullPath, file.FileName), FileMode.Create))
                                {
                                    file.CopyTo(fileStream);
                                }
                                count++;
                            }
                            catch { }
                        }
                        if (count != 0)
                            return StatusCode(200);
                        else
                            return StatusCode(500);
                    }
                    else
                    {
                        return StatusCode(400);
                    }
                }
                else
                {
                    string pathToFile = Path.Combine(storagePath, Request.Headers["X-Copy-From"]);
                    if (System.IO.File.Exists(pathToFile))
                    {
                        if (Directory.Exists(fullPath))
                        {
                            string fileName = Path.GetFileName(pathToFile);
                            try
                            {
                                using (FileStream fileStream = new FileStream(Path.Combine(fullPath, fileName), FileMode.Create))
                                {
                                    using (FileStream fileStreamFrom = new FileStream(pathToFile, FileMode.Open))
                                    {
                                        fileStreamFrom.CopyTo(fileStream);
                                    }
                                }
                                return StatusCode(200);
                            }
                            catch
                            {
                                return StatusCode(500);
                            }
                        }
                        else
                        {
                            return StatusCode(400);
                        }
                    }
                    else
                    {
                        return StatusCode(404);
                    }
                }
            }
            catch
            {
                return StatusCode(400);
            }
        }

        [HttpGet("{*fileName}")]
        public ActionResult HTTPGet(string fileName)
        {
            if (fileName == null)
                fileName = "";
            if (System.IO.File.Exists(Path.Combine(storagePath, fileName)))
            {
                try
                {
                    string path = Path.Combine(storagePath, fileName);
                    FileStream file = new FileStream(path, FileMode.Open);
                    return File(file, "application/unknown", Path.GetFileName(fileName));
                }
                catch
                {
                    return StatusCode(500);
                }
            }
            else
            {
                string directoryName = fileName;
                try
                {
                    IReadOnlyCollection<string> files = FileSystem.GetFiles(Path.Combine(storagePath, directoryName));
                    IReadOnlyCollection<string> directories = FileSystem.GetDirectories(Path.Combine(storagePath, directoryName));
                    List<Item> content = new List<Item>();
                    foreach (var element in directories)
                    {
                        content.Add(new Item("Folder", Path.GetFileName(element)));
                    }
                    foreach (var element in files)
                    {
                        content.Add(new Item("File", Path.GetFileName(element)));
                    }
                    return new JsonResult(content, new JsonSerializerOptions { });
                }
                catch
                {
                    return StatusCode(404);
                }
            }
        }

        [HttpHead("{*fileName}")]
        public ActionResult HTTPHead(string fileName)
        {
            try
            {
                string path = Path.Combine(storagePath, fileName);
                FileInfo fileInfo = FileSystem.GetFileInfo(path);
                if (fileInfo.Exists)
                {
                    Response.Headers.Add("Name", fileName);
                    Response.Headers.Add("Path", path);
                    Response.Headers.Add("Extension", fileInfo.Extension);
                    Response.Headers.Add("Size", fileInfo.Length.ToString());
                    Response.Headers.Add("Last access time", fileInfo.LastWriteTime.ToString());
                    Response.Headers.Add("Creation time", fileInfo.CreationTime.ToString());
                    return StatusCode(200);
                }
                else
                {
                    return StatusCode(404);
                }
            }
            catch
            {
                return StatusCode(404);
            }
        }

        [HttpDelete("{*fileName}")]
        public ActionResult DeleteFile(string fileName)
        {
            string fullPath = Path.Combine(storagePath, fileName);
            try
            {
                if (System.IO.File.Exists(fullPath))
                {
                    FileSystem.DeleteFile(fullPath);
                }
                else
                {
                    FileSystem.DeleteDirectory(fullPath, DeleteDirectoryOption.DeleteAllContents);
                }
                return StatusCode(200);
            }
            catch
            {
                return StatusCode(404);
            }
        }
    }
}
