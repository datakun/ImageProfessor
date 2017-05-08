using ImageProfessorWebServer.Data;
using ImageProfessorWebServer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProfessorWebServer.Controllers
{
    public class GalleriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _environment;

        public GalleriesController(ApplicationDbContext context)
        {
            _context = context;
            _environment = context.GetHostingEnvironment();
        }

        // GET: Galleries
        public async Task<IActionResult> Index()
        {
            return View(await _context.Gallery.ToListAsync());
        }

        // GET: Galleries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gallery = await _context.Gallery
                .SingleOrDefaultAsync(m => m.ID == id);
            if (gallery == null)
            {
                return NotFound();
            }

            return View(gallery);
        }

        // GET: Galleries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Galleries/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file, string effect)
        {
            if (ModelState.IsValid)
            {
                long size = file.Length;
                
                if (size > 0)
                {
                    int count = _context.Gallery.Count() + 1;

                    var folderPath = _environment.WebRootPath + "\\sources";
                    DirectoryInfo di = new DirectoryInfo(folderPath);
                    if (di.Exists == false)
                        di.Create();

                    string filename = count.ToString() + Path.GetExtension(file.FileName);

                    var filePath = folderPath + "\\" + filename;
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    Gallery gallery = new Gallery
                    {
                        UploadUser = "Me",
                        UploadDateTime = DateTime.Now.ToUniversalTime(),
                        SourceImagePath = filePath,
                        ResultImagePath = "",
                        IsProcessing = true
                    };

                    _context.Add(gallery);

                    await _context.SaveChangesAsync();

                    var factory = new ConnectionFactory();

                    factory.Port = 5672;
                    factory.HostName = "localhost";
                    factory.UserName = "professor";
                    factory.Password = "rlawnsdn";
                    factory.VirtualHost = "image_professor";

                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare("kimjunu.routing.ip.srcs", ExchangeType.Direct, true, false, null);
                        channel.QueueDeclare("kimjunu.queue.ip.srcs", true, false, false, null);
                        channel.QueueBind("kimjunu.queue.ip.srcs", "kimjunu.routing.ip.srcs", "sources_queue");

                        IBasicProperties properties = channel.CreateBasicProperties();
                        properties.Persistent = true;
                        properties.ContentType = "text/plain";

                        var message = gallery.UploadUser + "," + gallery.ID.ToString() + "," + filename + "," + effect;
                        var body = Encoding.UTF8.GetBytes(message);

                        PublicationAddress address = new PublicationAddress(ExchangeType.Direct, "kimjunu.routing.ip.srcs", "sources_queue");
                        channel.BasicPublish(address, properties, body);

                        channel.Close();
                        connection.Close();
                    }
                }

                return RedirectToAction("Index");
            }

            return View();
        }

        // GET: Galleries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gallery = await _context.Gallery.SingleOrDefaultAsync(m => m.ID == id);
            if (gallery == null)
            {
                return NotFound();
            }
            return View(gallery);
        }

        // POST: Galleries/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,UploadUser,UploadDateTime,SourceImagePath,ResultImagePath")] Gallery gallery)
        {
            if (id != gallery.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gallery);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GalleryExists(gallery.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(gallery);
        }

        // GET: Galleries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gallery = await _context.Gallery
                .SingleOrDefaultAsync(m => m.ID == id);
            if (gallery == null)
            {
                return NotFound();
            }

            return View(gallery);
        }

        // POST: Galleries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gallery = await _context.Gallery.SingleOrDefaultAsync(m => m.ID == id);

            if (gallery != null)
            {
                if (String.IsNullOrEmpty(gallery.SourceImagePath) == false)
                    System.IO.File.Delete(gallery.SourceImagePath);

                if (String.IsNullOrEmpty(gallery.ResultImagePath) == false)
                    System.IO.File.Delete(gallery.ResultImagePath);
            }

            _context.Gallery.Remove(gallery);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        private bool GalleryExists(int id)
        {
            return _context.Gallery.Any(e => e.ID == id);
        }
    }
}
