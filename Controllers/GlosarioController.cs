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
using Newtonsoft.Json;

namespace WebApplication.Controllers
{
    public class GlosarioController: Controller{
        private ApplicationDbContext ctx = new ApplicationDbContext();
        private IHostingEnvironment _environment;

        public GlosarioController(IHostingEnvironment environment){
            _environment = environment;
        }

        public IActionResult Index()
        {
            ViewBag.TipoUsuario = HttpContext.Session.GetTipoUsuario();
            ViewBag.proyecto = HttpContext.Session.GetTituloProyecto();
            if(HttpContext.Session.GetTipoUsuario() == 0){
                return RedirectToAction("Index", "AccesoDenegado");
            }
            ViewBag.ok=false;
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file){ 
            try
            {
                ViewBag.proyecto = HttpContext.Session.GetTituloProyecto();
                int grupoUltimo = 0;
                var ultimo = ctx.Glosarios.LastOrDefault();
                if(ultimo!=null) grupoUltimo = ultimo.Grupo;

                string texto="";

                using(var fs = file.OpenReadStream())            
                using(var ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    var filebytes = ms.ToArray();
                    string s = Convert.ToBase64String(filebytes);
                    texto = System.Text.Encoding.UTF8.GetString(filebytes);                
                }      
                var proyecto = ctx.Proyectos.Where(p => p.Id == HttpContext.Session.GetIdProyecto()).First();

                var partesTexto = texto.Split('\n');
                var lenguajes = partesTexto[0].Split(',');
                for(int i=1; i<partesTexto.Length-1;i++){
                    var palabras = partesTexto[i].Split(',');
                    for(int j=0; j<lenguajes.Length;j++){
                    var glosario = new Glosario {
                        CodLenguaje = lenguajes[j],
                        Palabra = palabras[j],
                        Grupo = grupoUltimo + i
                    };
                    var proyecto_glosario = new Proyecto_Glosario {
                        ProyectoId = proyecto,
                        GlosarioId = glosario
                    };
                    ctx.Glosarios.Add(glosario);
                    ctx.Proyecto_Glosarios.Add(proyecto_glosario);

                    }
                }
                
                ctx.SaveChanges();
                GlosariosLog glLog = new GlosariosLog(){
                    NOMBRE_PROYECTO = HttpContext.Session.GetTituloProyecto(),
                    FICHERO = file.FileName
                };

                Log4NetProvider.logInfo("Glosario", "Subida", JsonConvert.SerializeObject(glLog));
                ViewBag.TipoUsuario = HttpContext.Session.GetTipoUsuario();
                ViewBag.proyecto = HttpContext.Session.GetTituloProyecto();
                ViewBag.ok=true;
                return View();
            }
            catch (System.Exception e)
            {
                ErrorLog error = new ErrorLog(){
                    NOMBRE_PROYECTO = HttpContext.Session.GetTituloProyecto(),
                    ERROR = e.Message
                };
                Log4NetProvider.logError("Glosario", "Index", JsonConvert.SerializeObject(error));
                throw;
            }
           
            
        }        
       
    }
}