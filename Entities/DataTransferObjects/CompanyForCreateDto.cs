using System;
using System.Collections.Generic;

namespace Entities.DataTransferObjects
{
    public class CompanyForCreateDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }

        public IEnumerable<EmployeeForCreateDto> Employees { get; set; }
    }
}
