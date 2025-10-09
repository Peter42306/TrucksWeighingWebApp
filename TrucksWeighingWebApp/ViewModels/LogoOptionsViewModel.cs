using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.ViewModels
{
    public class LogoOptionsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Height { get; set; }
        public int PaddingBottom { get; set; }
        public LogoPosition Position { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}
