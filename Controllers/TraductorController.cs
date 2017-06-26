using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Moses.Models;
using Newtonsoft.Json;

namespace Moses.Controllers
{
    public class PalabrasGlosario{
        public string palabraOrigen;
        public string palabraNueva;
    }
    public class LenguajeTraduccion{
        public string codigo;
        public string nombre;
    }    

    public class LenguajeDisponible{
        public LenguajeTraduccion origen;

        public LenguajeTraduccion destino;
    }

    public class TraductorController: Controller{
        private IHostingEnvironment _environment;
        private ApplicationDbContext ctx = new ApplicationDbContext();

        Dictionary<string,string> diccionaroLenguajes = new Dictionary<string,string>();

        
        public TraductorController(IHostingEnvironment environment){
            _environment = environment;
            CreaDiccionarioLenguajes();
        }

        public IActionResult Index(int idProyecto)
        {           
            ViewBag.TipoUsuario = HttpContext.Session.GetTipoUsuario();
            ViewBag.proyecto = HttpContext.Session.GetTituloProyecto();            
            
            /*Traductor sin login, habilitamos desplegable con le selección de proyectos */
            if(idProyecto==0){
                ViewBag.proyecto=null;
                var vm = new ViewModels();
                var proyectos = ctx.Proyectos.ToList();
                vm.Proyectos = proyectos;
                return View(vm);
            }else{
                if(!SeleccionaProyecto(idProyecto)) return RedirectToAction("Index", "AccesoDenegado");  
                return View();
            }
            
        }

        public void CreaDiccionarioLenguajes(){            
            diccionaroLenguajes.Add("es","Español");
            diccionaroLenguajes.Add("en","Inglés");
            diccionaroLenguajes.Add("fr","Francés");
            diccionaroLenguajes.Add("it","Italiano");
            diccionaroLenguajes.Add("pt","Portugués");
            diccionaroLenguajes.Add("de","Alemán");
            diccionaroLenguajes.Add("ja","Japonés");
            diccionaroLenguajes.Add("zh","Chino");
        }
        [HttpGet]
        public string BindLenguajesOrigen(string idProyecto){
            List<LenguajeDisponible> salida = new List<LenguajeDisponible>();
            if(idProyecto == null){
                idProyecto = HttpContext.Session.GetTituloProyecto();
                if(idProyecto==null){
                    var a = Request.Query;
                    var b = Request.QueryString;
                }
            } 
            var proy = ctx.Proyectos.Where(s => s.Descripcion.Equals(idProyecto)).FirstOrDefault();
            var proymem = ctx.Proyecto_Memorias.Include(i => i.MemoriaId).Where(s => s.ProyectoId == proy).ToList();
            foreach(var memoria in proymem){
                var mem = ctx.Memorias.Include(i => i.LenguajeId).Where(w => w.Id == memoria.MemoriaId.Id).FirstOrDefault();
                string nombreOrigen = diccionaroLenguajes[mem.LenguajeId.CodLenguaje_origen];                
                LenguajeTraduccion lengOrigen = new LenguajeTraduccion(){
                    codigo = mem.LenguajeId.CodLenguaje_origen,
                    nombre = nombreOrigen
                };
                string nombreDestino = diccionaroLenguajes[mem.LenguajeId.CodLenguaje_destino];
                LenguajeTraduccion lengDestino = new LenguajeTraduccion(){
                    codigo = mem.LenguajeId.CodLenguaje_destino,
                    nombre = nombreDestino
                };
                LenguajeDisponible leng1 = new LenguajeDisponible(){
                    origen = lengOrigen,
                    destino = lengDestino
                };
                salida.Add(leng1);
            }            
            string salidaJSON = JsonConvert.SerializeObject(salida);
            return salidaJSON;
        }
        [HttpGet]
        public string BindLenguajesDestino(string idProyecto, string codLenguaje){
            List<LenguajeTraduccion> salida = new List<LenguajeTraduccion>();
            if(idProyecto == null) idProyecto = HttpContext.Session.GetTituloProyecto();
            var proy = ctx.Proyectos.Where(s => s.Descripcion.Equals(idProyecto)).FirstOrDefault();
            var proymem = ctx.Proyecto_Memorias.Include(i => i.MemoriaId).Where(s => s.ProyectoId == proy).ToList();
            foreach(var memoria in proymem){
                var mem = ctx.Memorias.Include(i => i.LenguajeId).Where(w => w.Id == memoria.MemoriaId.Id && w.LenguajeId.CodLenguaje_origen.Equals(codLenguaje)).FirstOrDefault();
                if(mem==null) continue;                
                string nombreDestino = diccionaroLenguajes[mem.LenguajeId.CodLenguaje_destino];
                LenguajeTraduccion lengDestino = new LenguajeTraduccion(){
                    codigo = mem.LenguajeId.CodLenguaje_destino,
                    nombre = nombreDestino
                };                
                salida.Add(lengDestino);
            }
            
            string salidaJSON = JsonConvert.SerializeObject(salida);
            return salidaJSON;
        }

         public bool SeleccionaProyecto(int id){
            try
            {
                SessionManager.PROYECTO.id = id;
                string query ="SELECT * FROM Proyectos WHERE id = {0}";
                var proyecto = ctx.Proyectos.FromSql(query,id).FirstOrDefault(); 
                string titulo = "";
                if(proyecto != null){
                    titulo = proyecto.Descripcion;
                    SessionManager.PROYECTO.titulo = titulo;                
                    ViewBag.proyecto = titulo;
                    HttpContext.Session.SetIdProyecto(id);
                    HttpContext.Session.SetTituloProyecto(titulo);
                    return true;
                }else{                    
                    return false;                               
                }
                
            }
            catch (System.Exception e)
            {
                ErrorLog error = new ErrorLog(){
                    NOMBRE_PROYECTO = "id: " + id,
                    ERROR = e.Message
                };
                Log4NetProvider.logError("Traductor", "SeleccionaProyecto", JsonConvert.SerializeObject(error));
                throw;
            }
            
        }

       
        [HttpPost]
        public async Task<IActionResult> Index(string text1, string lang1, string text2,string lang2, string proyecto,bool chkGlosario = false){ 
            ViewBag.textOriginal = text1;
            var entrada = text1.ToLower();
            string texto = entrada.Replace(".", " .");
            texto = texto.Replace(",", " ,");    
            texto = texto.Replace("?", " ?"); 
            texto = texto.Replace("!", " !"); 
            texto = texto.Replace("¿", "¿ "); 
            texto = texto.Replace("¡", "¡ ");       
            if(chkGlosario == true){
                entrada = UsarGlosario(lang1,lang2,texto);
            }else entrada = texto;
            
            var traducccion = Traducir(entrada, lang1, lang2, proyecto);
            traducccion = traducccion.Replace("&apos;", "'");
            ViewBag.traduccion = traducccion;
            ViewBag.TipoUsuario = HttpContext.Session.GetTipoUsuario();
            ViewBag.proyecto = HttpContext.Session.GetTituloProyecto();
            ViewBag.lang1 = lang1;
            ViewBag.lang2 = lang2;
            //Traducir(text1, lang1);
            if(proyecto!=null){
                ViewBag.proyecto=null;
                var vm = new ViewModels();
                var proyectos = ctx.Proyectos.ToList();
                vm.Proyectos = proyectos;
                ViewBag.proyectoSeleccionado = proyecto;
                return View(vm);
            }else{
                return View();
            }
            
        }

        [HttpPost]
        public string TraducirFormulario(string text1, string lang1, string text2,string lang2, string proyecto,bool chkGlosario = false){ 
            ViewBag.textOriginal = text1;
            var entrada = text1;
            if(chkGlosario == true){
                entrada = UsarGlosario(lang1,lang2,text1);
            }
            
            var traducccion = Traducir(entrada, lang1, lang2, proyecto);
            ViewBag.traduccion = traducccion;
            ViewBag.TipoUsuario = HttpContext.Session.GetTipoUsuario();
            ViewBag.proyecto = HttpContext.Session.GetTituloProyecto();
            ViewBag.lang1 = lang1;
            ViewBag.lang2 = lang2;
            //Traducir(text1, lang1);
            return JsonConvert.SerializeObject(traducccion);
            
        }

        public string InvocarTraduccion(string text1)
        {   
            try{
                 /*ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "gnome-terminal"};
                 startInfo.Arguments = "-x moses.sh";*/
                /*Process proc = new Process() {StartInfo = startInfo, };
                proc.Start();*/
                Console.WriteLine("ENTRAAAA");                                 
                
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName="traduccion.sh";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true; 
                startInfo.Arguments = text1;
                //startInfo.Arguments = "~/corpus/training/prueba_es_en/news-commentary-v8.es-en.es es ~/corpus/training/prueba_es_en/news-commentary-v8.es-en.en en ~/corpus/prueba/dev/news-test2008.es ~/corpus/prueba/dev/news-test2008.en";               
                Process proc = Process.Start(startInfo);
                string outputTerminal = proc.StandardOutput.ReadToEnd();
                Thread.Sleep(3000);
                 Console.WriteLine("SI");
                using(StreamWriter sw = proc.StandardInput)
                {
                    if(sw.BaseStream.CanWrite)
                    {
                        sw.WriteLine("si");
                    }
                }
                
                
                proc.WaitForExit();
                return "outputTerminal";
                
            }
            catch(Exception e){
                Log4NetProvider.logError("Traductor", "Invocar", e.Message);
                return null;
            }         
           
        }
        public string Traducir(string text1, string lang1, string lang2, string sProyecto){         
            try
            {         
                string proyectName = "" ;      
                ViewBag.errorTraduccion = "";
                if(sProyecto==null){
                    proyectName = HttpContext.Session.GetTituloProyecto();
                }else proyectName = sProyecto;
                
                /*Si no está logueado el proyecto por defecto es el primero de la tabla actualizado o el último en añadirse */
                if(proyectName== null || proyectName==""){
                    var proyecto = ctx.Proyectos.Where(p => p.Actualizado==true).FirstOrDefault();
                    if(proyecto==null){
                        proyecto = ctx.Proyectos.Last();
                    }
                    proyectName = proyecto.Descripcion;
                }  
                string ubicacion = proyectName + "/" + lang1 + "-" + lang2;
                var urlWorking = "wwwroot/moses/working/" + ubicacion;
                if(!Directory.Exists(urlWorking)){
                    ViewBag.errorTraduccion = "Memoría de traduccción inexistente para los lenguajes seleccionados";
                    return "";
                }

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName="traduccion.sh";                
                startInfo.UseShellExecute = false;                
                startInfo.RedirectStandardOutput = true; 
                startInfo.Arguments = ubicacion + " " + text1;;  
                //startInfo.Arguments= "echo " + "'" +text1+ "'" + "| ~/mosesdecoder/bin/moses -f ~/working/prueba/mert-work/moses.ini";             
                Process proc = Process.Start(startInfo);
                string outputTerminal = proc.StandardOutput.ReadToEnd();
                
                
                
                proc.WaitForExit();
                return outputTerminal;
            }
            catch (Exception e)
            {
                Log4NetProvider.logError("Traductor", "Invocar", e.Message);                
                return null;
            }  
                
        }

        public string UsarGlosario(string lang1,string lang2, string traduccion){
            try
            {
                var glosarios = ctx.Proyecto_Glosarios.Include(i => i.GlosarioId).Where(p => p.ProyectoId.Id == HttpContext.Session.GetIdProyecto()).ToList();
                var palabras1 = new List<Glosario>();
                var palabras2 = new List<Glosario>();
                var palabrasReemplazadas = new List<PalabrasGlosario>();
                
                foreach(var gl in glosarios){                    
                    var palAux1 = ctx.Glosarios.Where(g => g.Id == gl.GlosarioId.Id && g.CodLenguaje==lang1).FirstOrDefault();
                    if(palAux1 != null) palabras1.Add(palAux1);
                    var palAux2 = ctx.Glosarios.Where(g => g.Id == gl.GlosarioId.Id && g.CodLenguaje==lang2).FirstOrDefault();
                    if(palAux2 != null) palabras2.Add(palAux2);
                }
                foreach(var palabra1 in palabras1){
                        foreach(var palabra2 in palabras2){
                            if(palabra1.Grupo == palabra2.Grupo){
                                if(traduccion.Contains(palabra1.Palabra)){
                                    traduccion = traduccion.Replace(palabra1.Palabra, palabra2.Palabra);
                                    var pg = new PalabrasGlosario(){
                                        palabraOrigen = palabra1.Palabra,
                                        palabraNueva = palabra2.Palabra
                                    };
                                    palabrasReemplazadas.Add(pg);
                                }
                                
                            }
                        }
                        
                    }
                ViewBag.palabrasReemplazadas = palabrasReemplazadas;
                return traduccion;
            }
            catch (System.Exception e)
            {
                Log4NetProvider.logError("Traductor", "UsarGlosario", e.Message);
                throw;
            }
            
            
        }
        
    }
}