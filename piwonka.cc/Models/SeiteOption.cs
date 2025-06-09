namespace Piwonka.CC.Models
{
    public class SeiteOption
    {
        public int Id { get; set; }
        public string Titel { get; set; } = string.Empty;
        public int Level { get; set; } = 0;
        public string DisplayTitel => new string('-', Level * 2) + " " + Titel;
    }
}
