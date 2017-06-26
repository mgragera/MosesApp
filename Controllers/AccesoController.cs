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
    public class AccesoController: Controller{
        private ApplicationDbContext ctx = new ApplicationDbContext();
        private IHostingEnvironment _environment;

        public AccesoController(IHostingEnvironment environment){
            _environment = environment;
        }

        public IActionResult Index()
        {     
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string codUsuario, string contrasena){ 
            if(IsContrasenaCorrecta(codUsuario, contrasena)){
                //return View("~/Views/Proyecto/Index.cshtml");
                Log4NetProvider.logInfo("Acceso","LoginCorrecto", codUsuario);
                return RedirectToAction("Index", "Proyecto");
            }else{
                ViewBag.error = "Usuario o contraseña incorrectos";
                return View();
            }     
            
        }
       
        private bool IsContrasenaCorrecta(string codUsuario, string contrasena){try
        {
            var usuario = GetUsuarioByCodUsuario(codUsuario);
            if(contrasena == usuario.Contrasena){
                HttpContext.Session.SetTipoUsuario(usuario.Tipo);

                return true;
            }
            return false;
        }
        catch (System.Exception e)
        {
            Log4NetProvider.logError("Acceso", "IsContrasenaCorrecta", e.Message);
            return false;
        }

            
        }
        private string GetContrasenaByCodUsuario(string codUsuario)
        {
            try
            {
                string query ="SELECT * FROM Usuarios WHERE CodUsuario = {0}";
                var pass = ctx.Usuarios.FromSql(query, codUsuario).SingleOrDefault().Contrasena;
            
                return pass;
            }
            catch (System.Exception e)
            {
                Log4NetProvider.logError("Acceso", "GetContrasenaByCodUsuario", e.Message);
                return null;
            }
            
        }

         private Usuario GetUsuarioByCodUsuario(string codUsuario)
        {
            try
            {
                string query ="SELECT * FROM Usuarios WHERE CodUsuario = {0}";
                var usuario = ctx.Usuarios.FromSql(query, codUsuario).SingleOrDefault();
            
                return usuario;
            }
            catch (System.Exception e)
            {
                Log4NetProvider.logError("Acceso", "GetUsuarioByCodUsuario", e.Message);
                return null;
                
            }
            
        }        
        
    }
}