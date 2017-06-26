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

namespace Moses.Controllers
{
    public class MemoriaController: Controller{
        private IHostingEnvironment _environment;
        private ApplicationDbContext ctx = new ApplicationDbContext();
        public MemoriaController(IHostingEnvironment environment){
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

        /*public string Index()
        {
            try{
                Thread oThread = new Thread(new ThreadStart(OpenTerminal));
                oThread.Start();
                return OpenTerminal();                

            }
            catch(Exception e){
                return e.Message;
            }
            
}*/
        [HttpPost]
        public async Task<IActionResult> Index(ICollection<IFormFile> files, string lang1, string lang2){ 
            var urlFile1 = "";
            var urlFile2 = "";
            ViewBag.TipoUsuario = HttpContext.Session.GetTipoUsuario();
            ViewBag.proyecto = HttpContext.Session.GetTituloProyecto();
            var urlProyecto = "uploads/" + HttpContext.Session.GetTituloProyecto()+"/"+lang1+"-"+lang2;
            var uploadsPath = Path.Combine(_environment.WebRootPath, urlProyecto);

            var nombreFichero = DateTime.Now.ToString("ddMMyyyyHHmmss");

            if(InsertMemoria(lang1,lang2)){
                if(files.ElementAt(0).Length > 0){
                    using(var fileStream = new FileStream(Path.Combine(uploadsPath, nombreFichero +"."+lang1),FileMode.Create)){
                            await files.ElementAt(0).CopyToAsync(fileStream);
                            urlFile1 = fileStream.Name;
                        }
                }
                if(files.ElementAt(1).Length > 0){
                    using(var fileStream = new FileStream(Path.Combine(uploadsPath, nombreFichero +"."+lang2),FileMode.Create)){
                            await files.ElementAt(1).CopyToAsync(fileStream);        
                            urlFile2 = fileStream.Name;                
                        }
                }
            
                
                MergeFiles(files, uploadsPath, lang1,lang2); 
            }
            MemoriasLog memlog = new MemoriasLog(){
                NOMBRE_PROYECTO = HttpContext.Session.GetTituloProyecto(),
                NOMBRE_FICHERO_ORIGEN = files.ElementAt(0).FileName,
                NOMBRE_FICHERO_DESTINO = files.ElementAt(1).FileName,
                LENGUAJES = lang1+"-"+lang2
            };

            Log4NetProvider.logInfo("Memoria", "Subida", JsonConvert.SerializeObject(memlog));           
            ViewBag.ok=true;
            return View();
        }
        
        public bool MergeFiles(ICollection<IFormFile> files, string uploadsPath,string lang1, string lang2){ 
            try
            {
                var proyectName = HttpContext.Session.GetTituloProyecto();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName="memorias.sh";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true; 
                startInfo.Arguments = uploadsPath + " " + lang1 + " " + lang2;
                //startInfo.Arguments = "~/corpus/training/prueba_es_en/news-commentary-v8.es-en.es es ~/corpus/training/prueba_es_en/news-commentary-v8.es-en.en en ~/corpus/prueba/dev/news-test2008.es ~/corpus/prueba/dev/news-test2008.en";               
                Process proc = Process.Start(startInfo);
                string outputTerminal = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                return true;
            }
            catch (System.Exception e)
            {
                ErrorLog error = new ErrorLog(){
                    NOMBRE_PROYECTO = HttpContext.Session.GetTituloProyecto(),
                    ERROR = e.Message
                };
                Log4NetProvider.logError("Memoria", "MergeFiles",JsonConvert.SerializeObject(error));
                return false;
                
            }           
            
        }
        
        /*Devuelve la ruta en para insertar las memorias */
        public bool InsertMemoria(string lang1, string lang2){    

            try
            {
                Lenguaje lenguaje = new Lenguaje();      
                string query ="SELECT * FROM Lenguajes WHERE CodLenguaje_origen = {0} AND CodLenguaje_destino = {1}";
                lenguaje = ctx.Lenguajes.FromSql(query,lang1,lang2).FirstOrDefault();             
                var url = "wwwroot/uploads/" + HttpContext.Session.GetTituloProyecto() +"/"+lang1+"-"+lang2;

                if(lenguaje==null){
                    lenguaje = new Lenguaje{
                        CodLenguaje_origen = lang1,
                        CodLenguaje_destino = lang2
                    };
                    ctx.Lenguajes.Add(lenguaje);                    
                     
                }     

                if(!Directory.Exists(url)){
                    Directory.CreateDirectory(url);
                }
                
                var memoria = new Memoria {
                    Fecha_modificacion = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    LenguajeId =  lenguaje
                };                

                query ="SELECT * FROM Proyectos WHERE Id = {0}";
                Proyecto pr = ctx.Proyectos.FromSql(query,HttpContext.Session.GetIdProyecto()).First();

                var proyecto_memoria = new Proyecto_Memoria{
                    ProyectoId = pr,
                    MemoriaId = memoria
                };
                ctx.Memorias.Add(memoria);
                ctx.Proyecto_Memorias.Add(proyecto_memoria);

                var result  = ctx.Proyectos.SingleOrDefault(b => b.Id == pr.Id);
                result.Actualizado = false;
                result.Fecha_modificacion = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");                

                ctx.SaveChanges();

                return true;
            }
            catch (System.Exception e)
            {
                ErrorLog error = new ErrorLog(){
                    NOMBRE_PROYECTO = HttpContext.Session.GetTituloProyecto(),
                    ERROR = e.Message
                };
                Log4NetProvider.logError("Memoria", "InsertMemoria", JsonConvert.SerializeObject(error));
                return false;                
            }

            
            
        }
        
    }
}