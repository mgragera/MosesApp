using Moses;
using Newtonsoft.Json;

public class ErrorLog{
    public string NOMBRE_PROYECTO { get; set; }
    public string ERROR { get; set; }

}

public class MemoriasLog{
    public string NOMBRE_PROYECTO { get; set; }
    public string NOMBRE_FICHERO_ORIGEN { get; set; }

    public string NOMBRE_FICHERO_DESTINO { get; set; }

    public string LENGUAJES { get; set; }
}

public class GlosariosLog{
    public string NOMBRE_PROYECTO { get; set; }
    public string FICHERO { get; set; }
}



public static class Log4NetProvider{
    public static void logError(string nombreControlador, string nombreFuncion,string mensaje){
        log4net.ILog log =  log4net.LogManager.GetLogger(typeof(Program));
        log.Error(nombreControlador + "-" + nombreFuncion + ": " + mensaje);
    }

    public static void logInfo(string nombreControlador, string nombreFuncion, string mensaje){
        log4net.ILog log =  log4net.LogManager.GetLogger(typeof(Program));
        log.Info(nombreControlador + "-" + nombreFuncion + ": " + mensaje);
    }
}