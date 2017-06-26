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
using Newtonsoft.Json;

namespace WebApplication.Controllers
{
    public class GestionController: Controller{
        private ApplicationDbContext ctx = new ApplicationDbContext();
        private IHostingEnvironment _environment;

        public GestionController(IHostingEnvironment environment){
            _environment = environment;
        }

        public IActionResult Index()
        {
            ViewBag.TipoUsuario = HttpContext.Session.GetTipoUsuario();
            if(HttpContext.Session.GetTipoUsuario() != 1){
                return RedirectToAction("Index", "AccesoDenegado");
            }
            var proyectos = GetProyectos();
            var vm = new ViewModels();
            vm.Proyectos = proyectos;            
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string titulo){ 
            CrearProyecto(titulo);
            var proyectos = GetProyectos();
            var vm = new ViewModels();
            vm.Proyectos = proyectos;            
            return View(vm);
            
        }

        public Proyecto CrearProyecto (string descripcion){
            try
            {
                var proyecto = new Proyecto {
                    Descripcion = descripcion,
                    Fecha_modificacion = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    Actualizado = false                    
                };
                ctx.Proyectos.Add(proyecto);
                ctx.SaveChanges();
                var url = "wwwroot/uploads/" + proyecto.Descripcion;
                Directory.CreateDirectory(url);      

                Log4NetProvider.logInfo("Gestion","CrearProyecto", JsonConvert.SerializeObject(proyecto));

                return proyecto;
            }
            catch(Exception e)
            {
                ErrorLog error = new ErrorLog(){
                    NOMBRE_PROYECTO = HttpContext.Session.GetTituloProyecto(),
                    ERROR = e.Message
                };
                Log4NetProvider.logError("Gestion", "CrearProyecto", JsonConvert.SerializeObject(error));
                return null;
                throw;
            }
            
        }

        public void BorrarProyecto (int id){
            try
            {   
                /*Eliminamos todas las memorias y glosarios */
                var prymen = ctx.Proyecto_Memorias.Include(i => i.MemoriaId).Where(s => s.ProyectoId.Id ==id).ToList(); 
                foreach(var obj in prymen){
                    var memoria = ctx.Memorias.Where(m => m.Id==obj.MemoriaId.Id).FirstOrDefault();
                    ctx.Proyecto_Memorias.Remove(obj);
                    ctx.Memorias.Remove(memoria);
                }

                var prygl = ctx.Proyecto_Glosarios.Include(i => i.GlosarioId).Where(s => s.ProyectoId.Id ==id).ToList();                
                foreach(var gl in prygl){
                    var glosario = ctx.Glosarios.Where(g => g.Id== gl.GlosarioId.Id).FirstOrDefault();
                    ctx.Proyecto_Glosarios.Remove(gl);
                    ctx.Glosarios.Remove(glosario);
                }
                
                string query ="SELECT * FROM Proyectos WHERE id = {0}";
                /*Borrado de las memorias subidas */
                var titulo = ctx.Proyectos.FromSql(query,id).First().Descripcion; 
                var url = "wwwroot/uploads/" + titulo;
                if(Directory.Exists(url)){
                    DirectoryInfo di = new DirectoryInfo(url);
                
                    foreach(DirectoryInfo dir in di.GetDirectories()){
                        foreach(FileInfo file in dir.GetFiles()){
                            file.Delete();
                        }
                        dir.Delete();
                    }
                    Directory.Delete(url);
                }
                  

                var urlCorpus = "wwwroot/moses/corpus/" + titulo;
                if(Directory.Exists(urlCorpus)){
                    DirectoryInfo diCorpus = new DirectoryInfo(urlCorpus);
                    foreach(DirectoryInfo dir in diCorpus.GetDirectories()){
                        foreach(FileInfo file in dir.GetFiles()){
                            file.Delete();
                        }
                        dir.Delete();
                    }
                    Directory.Delete(urlCorpus); 
                }

                var urlLm = "wwwroot/moses/lm/" + titulo;
                if(Directory.Exists(urlLm)){
                    DirectoryInfo diLm = new DirectoryInfo(urlLm);
                    foreach(DirectoryInfo dir in diLm.GetDirectories()){
                        foreach(FileInfo file in dir.GetFiles()){
                            file.Delete();
                        }
                        dir.Delete();
                    }
                    Directory.Delete(urlLm);
                }

                var urlWorking = "wwwroot/moses/working" + titulo;
                if(Directory.Exists(urlWorking)){
                    DirectoryInfo diWorking = new DirectoryInfo(urlWorking);
                    foreach(DirectoryInfo dir in diWorking.GetDirectories()){
                        foreach(FileInfo file in dir.GetFiles()){
                            file.Delete();
                        }
                        dir.Delete();
                    }
                    Directory.Delete(urlWorking); 
                }

                var proyecto = ctx.Proyectos.Where(se => se.Id==id).First();    
                /*query ="DELETE FROM Proyectos WHERE id = {0}";
                var proyectos = ctx.Proyectos.FromSql(query, id);*/  
                ctx.Proyectos.Remove(proyecto);
                              
                ctx.SaveChanges();          

                Log4NetProvider.logInfo("Gestion", "BorrarProyecto", JsonConvert.SerializeObject(proyecto));                      
                
            }
            catch(Exception e)
            {   
                ErrorLog error = new ErrorLog(){
                    NOMBRE_PROYECTO = HttpContext.Session.GetTituloProyecto(),
                    ERROR = e.InnerException.Message
                };
                Log4NetProvider.logError("Gestion", "BorrarProyecto", JsonConvert.SerializeObject(error));             
                throw;
            }
            
        }

        private List<Proyecto> GetProyectos()
        {
            try
            {
                string query ="SELECT * FROM Proyectos";
                var proyectos = ctx.Proyectos.FromSql(query).ToList(); 
                return proyectos;
            }
            catch (System.Exception e)
            {
                ErrorLog error = new ErrorLog(){
                    NOMBRE_PROYECTO = HttpContext.Session.GetTituloProyecto(),
                    ERROR = e.Message
                };
                Log4NetProvider.logError("Gestion", "GetProyectos", JsonConvert.SerializeObject(error)); 
                throw;
            }
           
                      
            
            
        }
        public void SeleccionaProyecto(int id){
            try
            {
                SessionManager.PROYECTO.id = id;
                string query ="SELECT * FROM Proyectos WHERE id = {0}";
                var titulo = ctx.Proyectos.FromSql(query,id).First().Descripcion; 
                SessionManager.PROYECTO.titulo = titulo;
            }
            catch (System.Exception e)
            {
                ErrorLog error = new ErrorLog(){
                    NOMBRE_PROYECTO = HttpContext.Session.GetTituloProyecto(),
                    ERROR = e.Message
                };
                Log4NetProvider.logError("Gestion", "SeleccionaProyecto", JsonConvert.SerializeObject(error));
                throw;
            }
            
        }
    

        public Lenguaje CrearLenguaje (string  codOrigen, string codDestino){
            try
            {
                var lenguaje = new Lenguaje{
                    CodLenguaje_origen = codOrigen,
                    CodLenguaje_destino = codDestino
                };
                ctx.Lenguajes.Add(lenguaje);
                ctx.SaveChanges();
                return lenguaje;
            }
            catch (System.Exception e)
            {
                ErrorLog error = new ErrorLog(){
                    NOMBRE_PROYECTO = HttpContext.Session.GetTituloProyecto(),
                    ERROR = e.Message
                };
                Log4NetProvider.logError("Gestion", "CrearLenguaje", JsonConvert.SerializeObject(error));
                throw;
            }
        }

        public void EjecutarMoses(int id, bool merge){
            try
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                var proyecto = ctx.Proyectos.Where(s => s.Id==id).First();

                Log4NetProvider.logInfo("Gestion","EjecutarMoses", JsonConvert.SerializeObject(proyecto));

                var relProyectos = ctx.Proyecto_Memorias.Where(r => r.ProyectoId.Id==id).ToList();                

                foreach(var rel in relProyectos){
                    var memoria = ctx.Memorias.Where(m => m.Id==rel.Id).FirstOrDefault();
                    var lenguaje = ctx.Memorias.Include(i => i.LenguajeId).Where(m => m.Id==rel.Id).FirstOrDefault().LenguajeId;
                    /*Comprobamos que la creacion del sistema de traduccion del lenguaje es necesaria, comparando la fecha con la de las memorias subidas */
                    string url = "wwwroot/moses/working/" + proyecto.Descripcion + "/" +lenguaje.CodLenguaje_origen + "-" + lenguaje.CodLenguaje_destino + "/mert-work/moses.ini";
                    if(System.IO.File.Exists(url)){
                        DateTime lastModified = System.IO.File.GetLastWriteTime(url);
                        if(lastModified>DateTime.ParseExact(memoria.Fecha_modificacion,"dd/MM/yyyy HH:mm:ss",provider)){
                            continue;
                        }                        
                    }                    
                    /*TODO: si no hay ficheros mergeados, cambiar el nombre del fichero de la url (merged-file) o por fecha */
                    var urlMemoria = "wwwroot/uploads/" + proyecto.Descripcion+"/"+lenguaje.CodLenguaje_origen+"-"+lenguaje.CodLenguaje_destino;
                    string urlMemoria1 = "";
                    string urlMemoria2 = "";
                    if(merge==true){                        
                        urlMemoria1 = urlMemoria+"/"+"merged-file."+lenguaje.CodLenguaje_origen;
                        urlMemoria2 = urlMemoria+"/"+"merged-file."+lenguaje.CodLenguaje_destino;
                    }else{
                        var carpeta = new DirectoryInfo(urlMemoria);
                        var archivo1 = (from f in carpeta.GetFiles() where f.Extension.Contains(lenguaje.CodLenguaje_origen) where f.Name.Contains("20") orderby f.LastWriteTime descending select f).FirstOrDefault();                        
                        var archivo2 = (from f in carpeta.GetFiles() where f.Extension.Contains(lenguaje.CodLenguaje_destino) where f.Name.Contains("20") orderby f.LastWriteTime descending select f).FirstOrDefault();
                        urlMemoria1 = archivo1.FullName;
                        urlMemoria2 = archivo2.FullName;
                    }
                    var uploadsPath1 = Path.Combine(_environment.WebRootPath, urlMemoria1);
                    var uploadsPath2 = Path.Combine(_environment.WebRootPath, urlMemoria2);
                    string folderName1 = proyecto.Descripcion+"/"+lenguaje.CodLenguaje_origen+"-"+lenguaje.CodLenguaje_destino;
                    //string folderName2 = proyecto.Descripcion+"/"+lenguaje.CodLenguaje_destino+"-"+lenguaje.CodLenguaje_origen;
                    OpenTerminal(uploadsPath1, lenguaje.CodLenguaje_origen, uploadsPath2, lenguaje.CodLenguaje_destino, folderName1);

                }
            }
            catch (System.Exception e)
            {
                ErrorLog error = new ErrorLog(){
                    NOMBRE_PROYECTO = HttpContext.Session.GetTituloProyecto(),
                    ERROR = e.Message
                };
                Log4NetProvider.logError("Gestion", "EjecutarMoses", JsonConvert.SerializeObject(error));
                throw;
            }
            
        }

        public string OpenTerminal(string file1, string lang1, string file2, string lang2, string folderName1)
        {   
            try{
                
                /*Primero creamos las carpetas para almacenar los ficheros generados por Moses*/      
                CrearDirectorios(folderName1);                    
                
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName="ejecutarMoses.sh";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true; 
                startInfo.Arguments = file1 + " " + lang1 + " " + file2 + " " + lang2 + " " + folderName1;
                //startInfo.Arguments = "~/corpus/training/prueba_es_en/news-commentary-v8.es-en.es es ~/corpus/training/prueba_es_en/news-commentary-v8.es-en.en en ~/corpus/prueba/dev/news-test2008.es ~/corpus/prueba/dev/news-test2008.en";               
                Process proc = Process.Start(startInfo);
                string outputTerminal = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                return outputTerminal;
                
            }
            catch(Exception e){
                ErrorLog error = new ErrorLog(){
                    NOMBRE_PROYECTO = HttpContext.Session.GetTituloProyecto(),
                    ERROR = e.Message
                };
                Log4NetProvider.logError("Gestion", "OpenTerminal", JsonConvert.SerializeObject(error));
                return e.Message;
            }         
           
        }

        public void CrearDirectorios(string folderName1){
            var urlCorpus1 = "wwwroot/moses/corpus/" + folderName1;
            if(!Directory.Exists(urlCorpus1)){                    
                Directory.CreateDirectory(urlCorpus1);
            }
            var urlLm1 = "wwwroot/moses/lm/" + folderName1;
            if(!Directory.Exists(urlLm1)){                    
                Directory.CreateDirectory(urlLm1);
            }
            var urlWorking1 = "wwwroot/moses/working/" + folderName1;
            if(!Directory.Exists(urlWorking1)){                    
                Directory.CreateDirectory(urlWorking1);
            }

            /*var urlCorpus2 = "wwwroot/moses/corpus/" + folderName2;
            if(!Directory.Exists(urlCorpus2)){                    
                Directory.CreateDirectory(urlCorpus2);
            }
            var urlLm2 = "wwwroot/moses/lm/" + folderName2;
            if(!Directory.Exists(urlLm2)){                    
                Directory.CreateDirectory(urlLm2);
            }
            var urlWorking2 = "wwwroot/moses/working/" + folderName2;
            if(!Directory.Exists(urlWorking2)){                    
                Directory.CreateDirectory(urlWorking2);
            }*/
        }

        public FileResult DescargarLog(){
            
            string filePath = "./logs/app.log";  
            
            //return File(filePath, "text/x-log");
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/x-msdownload", "app.log");
        }
       
                 
        
    }
}