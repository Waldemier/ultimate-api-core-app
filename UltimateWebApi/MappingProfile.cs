using System;
using AutoMapper;
using Entities.DataTransferObjects;
using Entities.Models;

namespace UltimateWebApi
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyDto>()
                .ForMember(cDto => cDto.FullAddress,
                    opt => opt.MapFrom(c => string.Join(' ', c.Address, c.Country)));

            CreateMap<Employee, EmployeeDto>();

            CreateMap<CompanyForCreateDto, Company>();

            CreateMap<EmployeeForCreateDto, Employee>();

            CreateMap<EmployeeForUpdateDto, Employee>().ReverseMap();

            CreateMap<CompanyForUpdateDto, Company>();

            CreateMap<UserForRegistrationDto, User>();
        }
    }
}
