
namespace DataDrain.ORM.Interfaces.Objetos
{
    /// <summary>
    /// Retorna as informações para selecionar um arquivo
    /// </summary>
    public class OpenDadosFileDialog
    {
        public string Title { get; set; }

        public string Filter { get; set; }

        public string InitialDirectory { get; set; }

        public bool CheckFileExists { get; set; }

        public bool CheckPathExists { get; set; }

        public int FilterIndex { get; set; }

        public bool Multiselect { get; set; }
    }
}
