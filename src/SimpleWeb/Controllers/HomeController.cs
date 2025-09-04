using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SimpleWeb.Models;
using TwentyTwenty.Storage;

namespace SimpleWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IStorageProvider _storageProvider;
        private readonly IConfiguration _config;


        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env, IStorageProvider storageProvider, IConfiguration config)
        {
            _logger = logger;
            _env = env;
            _storageProvider = storageProvider;
            _config = config;
        }

        public IActionResult Index()
        {
            string userName;
            string identifier = "X-MS-CLIENT-PRINCIPAL-NAME";
            IEnumerable<string> headerValues = HttpContext.Request.Headers[identifier];
            if (!headerValues.Any())
                userName = "Not login yet";
            else
                userName = headerValues.FirstOrDefault();

            ViewBag.userName = userName;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Upload()
        {
            var fileName = _config.GetValue<string>("Storage:FileName");
            var cloudUrl = _storageProvider.GetBlobSasUrl("simpleweb", fileName,
                DateTimeOffset.Now.AddDays(1));
            var localUrl = $"~/uploads/{fileName}";

            ViewBag.cloudUrl = cloudUrl;
            ViewBag.localUrl = localUrl;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile imgFile)
        {
            if (imgFile != null && imgFile.Length > 0)
            {
                var storageType = _config.GetValue<StorageType>("Storage:Type");
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imgFile.FileName)}";
                var containerName = "simpleweb";
                if (storageType == StorageType.Local)
                    containerName = "uploads";
                await _storageProvider.SaveBlobStreamAsync(containerName, $"{fileName}", imgFile.OpenReadStream())
                    .ConfigureAwait(false);

                // update appsettings.json
                var jsonObj =
                    JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(Path.Combine(_env.ContentRootPath, "appsettings.json")));
                jsonObj["Storage"]["FileName"] = fileName;

                System.IO.File.WriteAllText(Path.Combine(_env.ContentRootPath, "appsettings.json"),
                    JsonConvert.SerializeObject(jsonObj, Formatting.Indented));
            }

            return RedirectToAction("Upload", "Home");
        }
    }
}
