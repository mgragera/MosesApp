namespace Moses.Models
{
    public class Proyecto_Glosario
    {
        public int Id { get; set; }
        public Proyecto ProyectoId { get; set; }
        public Glosario GlosarioId { get; set; }
    }
}