namespace RM.Model.RequestModel
{
    public class IncomeStatementAnalysis
    {

        public string Name { get; set; }
        public IncomeStatementAnalysisData Data { get; set; }

    }

    public class IncomeStatementAnalysisData
    {
        public string Year { get; set; }
        public string Value { get; set; }
    }
}
