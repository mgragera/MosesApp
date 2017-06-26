using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Moses.Models;
using Microsoft.EntityFrameworkCore;
 

namespace WebApplication.Controllers
{
    public class AccesoDenegadoController: Controller{
        private ApplicationDbContext ctx = new ApplicationDbContext();
        private IHostingEnvironment _environment;

        public AccesoDenegadoController(IHostingEnvironment environment){
            _environment = environment;
        }

        public IActionResult Index()
        {     
            HttpContext.Session.Clear();
            return View();
        }

       
       
        
    }
}