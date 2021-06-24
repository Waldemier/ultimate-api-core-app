using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Contracts;
using Entities.DataTransferObjects;
using Entities.LinkModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace UltimateWebApi.Utility
{
    public class EmployeeLinks
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IDataShaper<EmployeeDto> _dataShaper;
        public EmployeeLinks(IDataShaper<EmployeeDto> dataShaper, LinkGenerator linkGenerator)
        {
            this._dataShaper = dataShaper;
            this._linkGenerator = linkGenerator;
        }
        
        /// <summary>
        /// Main method, that returns either Shaped data with links or without links.
        /// </summary>
        /// <param name="employeesDto"></param>
        /// <param name="fields"></param>
        /// <param name="companyId"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public LinkResponse TryGenerateLinks(IEnumerable<EmployeeDto> employeesDto, string fields, Guid companyId,
            HttpContext httpContext)
        {
            var shapedEmployees = ShapeData(employeesDto, fields);

            if (ShouldGenerateLinks(httpContext)) // if context has require media type (application/...hateoas+json)
                return ReturnLinkedEmployees(employeesDto, fields, companyId, httpContext,
                    shapedEmployees);
            
            return ReturnShapedEmployees(shapedEmployees);
        }
        
        /// <summary>
        /// Returns specify objects with only specify fields (props), which are specified in fields attribute.
        /// </summary>
        /// <param name="employeesDto"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        private List<ExpandoObject> ShapeData(IEnumerable<EmployeeDto> employeesDto, string fields) =>
            this._dataShaper.ShapeData(employeesDto, fields)
                .Select(e => e.EntityProperties)
                .ToList();

        /// <summary>
        /// Checks if context have Media type item, if it is not then returns false.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        private bool ShouldGenerateLinks(HttpContext httpContext)
        {
            var mediaType = (Microsoft.Net.Http.Headers.MediaTypeHeaderValue)httpContext.Items["AcceptHeaderMediaType"];
            return mediaType.SubTypeWithoutSuffix.EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase); // If that media type ends with hateoas, the method returns true
        }
        
        /// <summary>
        /// Returns just Shaped Entities without links.
        /// </summary>
        /// <param name="shapedEmployees"></param>
        /// <returns></returns>
        private LinkResponse ReturnShapedEmployees(List<ExpandoObject> shapedEmployees) => 
            new LinkResponse() { ShapedEntities = shapedEmployees };

        /// <summary>
        /// Method which returns Shaped entities within links.
        /// </summary>
        /// <param name="employeesDto"></param>
        /// <param name="fields"></param>
        /// <param name="companyId"></param>
        /// <param name="httpContext"></param>
        /// <param name="shapedEmployees"></param>
        /// <returns></returns>
        private LinkResponse ReturnLinkedEmployees(IEnumerable<EmployeeDto> employeesDto, string fields, Guid companyId, HttpContext httpContext,
           List<ExpandoObject> shapedEmployees)
        {
            var employeeDtoList = employeesDto.ToList();
            
            for (int index = 0; index < employeeDtoList.Count(); index++)
            {
                var employeeLinks = CreateLinksForEmployee(httpContext, companyId, employeeDtoList[index].Id, fields);
                shapedEmployees[index].TryAdd("Links", employeeLinks); // Implementing addition property
            }

            var employeeCollection = new LinkCollectionWrapper<ExpandoObject>(shapedEmployees);
            var linkedEmployees = CreateLinksForEmployees(httpContext, employeeCollection);

            return new LinkResponse() { HasLinks = true, LinkedEntities = linkedEmployees };
        }

        private List<Link> CreateLinksForEmployee(HttpContext httpContext, Guid companyId, Guid Id, string fields = "")
        {
            // For example what one of the links looks like:
            // https://localhost:5001/api/companies/c9d4c053-49b6-410c-bc78-2d54a9991870/employees/86dba8c0-d178-41e7-938c-ed49778fb52a?fields=name,age
            
            var links = new List<Link>
            {
                new Link(this._linkGenerator.GetUriByAction(httpContext, "GetEmployee",
                        "Employees", values: new {companyId, Id, fields}), // values that need to be used to make the URL valid.
                    "self",
                    "GET"),

                new Link(this._linkGenerator.GetUriByAction(httpContext, "DeleteEmployeeForCompany",
                        "Employees", values: new {companyId, Id}),
                    "delete_employee",
                    "DELETE"),

                new Link(this._linkGenerator.GetUriByAction(httpContext, "UpdateEmployeeForCompany",
                        "Employees", values: new {companyId, Id}),
                    "update_employee",
                    "PUT"),

                new Link(this._linkGenerator.GetUriByAction(httpContext, "PartiallyUpdateEmployeeForCompany",
                        "Employees", values: new {companyId, Id, fields}),
                    "partially_update_employee",
                    "PATCH")
            };

            return links;
        }

        private LinkCollectionWrapper<ExpandoObject> CreateLinksForEmployees(HttpContext httpContext, LinkCollectionWrapper<ExpandoObject> employeesWrapperCollection)
        {
            employeesWrapperCollection.Links.Add(new Link(
                this._linkGenerator.GetUriByAction(httpContext, "GetEmployees", "Employees", values: new {}),
                "self",
                "GET"
                ));
            return employeesWrapperCollection;
        }

    }
}