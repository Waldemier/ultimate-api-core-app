namespace Entities.RequestFeatures
{
    public class EmployeeParameters: RequestParameters
    {
        public EmployeeParameters()
        {
            OrderBy = "name";
        }
        
        public uint MinAge { get; set; } // by default are zero
        
        public uint MaxAge { get; set; } = int.MaxValue;

        public bool ValidAgeRange => MaxAge > MinAge;
        
        public string SearchTerm { get; set; }
    }
}