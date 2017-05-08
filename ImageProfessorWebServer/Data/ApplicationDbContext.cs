using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ImageProfessorWebServer.Models;
using Microsoft.AspNetCore.Hosting;

namespace ImageProfessorWebServer.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHostingEnvironment _environment;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            IHostingEnvironment environment)
            : base(options)
        {
            _environment = environment;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        public IHostingEnvironment GetHostingEnvironment()
        {
            return _environment;
        }

        public DbSet<ImageProfessorWebServer.Models.Gallery> Gallery { get; set; }
    }
}
