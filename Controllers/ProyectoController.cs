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
using System.Globalization;
 

namespace WebApplication.Controllers
{
    public class ProyectoController: Controller{
        private ApplicationDbContext ctx = new ApplicationDbContext();
        private IHostingEnvironment _environment;

        public ProyectoController(IHostingEnvironment environment){
            _environment = environment;
        }

        public IActionResult Index()
        {
            CheckActualizado();
            if(HttpContext.Session.GetTipoUsuario() == 0){
                return RedirectToAction("Index", "AccesoDenegado");
            }
            ViewBag.TipoUsuario = HttpContext.Session.GetTipoUsuario();
            var proyectos = GetProyectos();
            var vm = new ViewModels();
            vm.Proyectos = proyectos;
            //var proyectos = ctx.Proyectos.Include(x => x); 
            
            return View(vm);
        }

       private List<Proyecto> GetProyectos()
        {
            string query ="SELECT * FROM Proyectos";
            var proyectos = ctx.Proyectos.FromSql(query).ToList(); 
                      
            
            return proyectos;
        }
        [HttpGet]
        public int SeleccionaProyecto(int id){
            HttpContext.Session.SetIdProyecto(id);            
            string query ="SELECT * FROM Proyectos WHERE id = {0}";
            var titulo = ctx.Proyectos.FromSql(query,id).First().Descripcion; 
            HttpContext.Session.SetTituloProyecto(titulo);
            //string urlFunct = "'@Url.Action("+"\"Index\"" +"," + "\"Traductor\"" +","+ "new { idProyecto = "+ id +"})'";
            return id;
        }

        public void CheckActualizado(){
            bool actualizar = false;

            var nombreProyecto = HttpContext.Session.GetTituloProyecto();
            //string url= "~/working/" + nombreProyecto + "/mert-work/moses.ini";
            string url = "";            
            CultureInfo provider = CultureInfo.InvariantCulture;

            int contadorActualizados = 0;
            var proyectos = GetProyectos();
            foreach(var proyecto in proyectos){
                if(proyecto.Actualizado == true) continue;

                var prymen = ctx.Proyecto_Memorias.Where(s => s.ProyectoId.Id ==proyecto.Id).ToList(); 
                foreach(var prm in prymen){
                    var mems = ctx.Memorias.Where(s=> s.Id == prm.Id).FirstOrDefault();
                    var lenguaje = ctx.Memorias.Include(i => i.LenguajeId).Where(m => m.Id==mems.Id).FirstOrDefault().LenguajeId;
                    url = "wwwroot/moses/working/" + proyecto.Descripcion + "/" +lenguaje.CodLenguaje_origen + "-" + lenguaje.CodLenguaje_destino + "/mert-work/moses.ini";
                    if(!System.IO.File.Exists(url)) break;
                    DateTime lastModified = System.IO.File.GetLastWriteTime(url);
                    if(lastModified>DateTime.ParseExact(mems.Fecha_modificacion,"dd/MM/yyyy HH:mm:ss",provider)){
                        actualizar = true;
                        contadorActualizados++;
                    }else actualizar = false;
                }                
                if (actualizar == true && contadorActualizados == prymen.Count){
                    var result  = ctx.Proyectos.SingleOrDefault(b => b.Id == proyecto.Id);
                    result.Actualizado = true;
                    result.Fecha_modificacion = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    ctx.SaveChanges();
                    actualizar = false;
                }
                contadorActualizados = 0;
            }
            
            
        }

       

    }
}