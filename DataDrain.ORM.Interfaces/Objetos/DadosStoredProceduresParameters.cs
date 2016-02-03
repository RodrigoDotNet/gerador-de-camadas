

namespace DataDrain.ORM.Interfaces.Objetos
{
    public class DadosStoredProceduresParameters
    {
        public string ParameterName { get; set; }

        public object ParameterValue { get; set; }

        public string ParameterDataType { get; set; }

        public string ParameterDotNetType { get; set; }

        public string ParameterMaxBytes { get; set; }

        public bool IsOutPutParameter { get; set; }

        public bool DefineNull { get; set; }
    }
}
