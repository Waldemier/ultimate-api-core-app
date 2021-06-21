using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entities.DataTransferObjects
{
    public class CompanyForCreateDto
    {
        [Required(ErrorMessage = "Company name is required field.")]
        [MaxLength(60, ErrorMessage = "Maximum lenght for the Name is 60 characters.")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Company address is required field.")]
        [MaxLength(60, ErrorMessage = "Maximum lenght for the Address is 60 characters.")]
        public string Address { get; set; }
        
        public string Country { get; set; }

        public IEnumerable<EmployeeForCreateDto> Employees { get; set; }
    }
}
