using Microsoft.AspNetCore.Http;
using System;

public class ProyectoSesion{
    public int id { get; set; }
    public string titulo { get; set; }
}
public static class SessionManager
{
    public static int TIPO_USUARIO=-1;

    public static string COD_USUARIO = "COD_USUARIO";
    public static ProyectoSesion PROYECTO = new ProyectoSesion();

    public static void SetTipoUsuario(this ISession session, int tipoU){
        string key = "tipoUsuario";
        session.SetString(key, tipoU.ToString());
    }

    public static void SetTituloProyecto(this ISession session, string proy){
        string key = "tituloProyecto";
        session.SetString(key, proy);
    }
    public static void SetIdProyecto(this ISession session, int proy){
        string key = "idProyecto";
        session.SetString(key, proy.ToString());
    }

    public static int GetTipoUsuario(this ISession session){
        string key = "tipoUsuario";
        return Convert.ToInt32(session.GetString(key));
    }

    public static string GetTituloProyecto(this ISession session){
        string key = "tituloProyecto";
        return session.GetString(key);
    }
    public static int GetIdProyecto(this ISession session){
        string key = "idProyecto";
        return Convert.ToInt32(session.GetString(key));
    }
}