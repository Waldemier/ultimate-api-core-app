using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Contracts;
using Entities.Models;

namespace Repository.DataShaping
{
    /// <summary>
    /// For return of the specific properties of the specify entity with their values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataShaper<T>: IDataShaper<T> where T: class
    {
        // Array which needs to contains real properties of the object (entity)
        private PropertyInfo[] Properties { get; set; }

        public DataShaper()
        {
            // Gets entity properties
            Properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance); // gets public and non-static properties
        }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="fieldsString"></param>
        /// <returns>All specify objects with specific fields.</returns>
        public IEnumerable<ShapedEntityWrapper> ShapeData(IEnumerable<T> entities, string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString);
            return FetchData(entities, requiredProperties);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="fieldsString"></param>
        /// <returns>One specify object with him specific fields.</returns>
        public ShapedEntityWrapper ShapeData(T entity, string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString);
            return FetchDataForEntity(entity, requiredProperties);
        }

        /// <summary>
        /// Helper method which returns the real object properties, based on fieldsString string argument
        /// </summary>
        /// <param name="fieldsString"></param>
        /// <returns></returns>
        private IEnumerable<PropertyInfo> GetRequiredProperties(string fieldsString)
        {
            var requiredProperties = new List<PropertyInfo>();
            if (!string.IsNullOrWhiteSpace(fieldsString))
            {
                var fields = fieldsString.Split(",", StringSplitOptions.RemoveEmptyEntries);
                foreach (var field in fields)
                {
                    var property = Properties.FirstOrDefault(prop => prop.Name.Equals(field.Trim(),
                        StringComparison.InvariantCultureIgnoreCase));
                    
                    if (property == null)
                        continue;
                    
                    requiredProperties.Add(property);
                }
            }
            else
            {
                requiredProperties = Properties.ToList();
            }
            
            return requiredProperties;
        }
        
        /// <summary>
        /// Helper method which returns the objects with their specify properties and values (key/value)
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="requiredProperties"></param>
        /// <returns></returns>
        private IEnumerable<ShapedEntityWrapper> FetchData(IEnumerable<T> entities, IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapedData = new List<ShapedEntityWrapper>();

            foreach (var entity in entities)
            {
                var shapeObject = FetchDataForEntity(entity, requiredProperties);
                shapedData.Add(shapeObject);
            }

            return shapedData;
        }

        /// <summary>
        /// Helper method which returns one object with specific properties and values (key/value)
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="requiredProperties"></param>
        /// <returns>key/value of object property</returns>
        private ShapedEntityWrapper FetchDataForEntity(T entity, IEnumerable<PropertyInfo> requiredProperties)
        {
            //var shapedObject = new ExpandoObject();
            var shapedObject = new ShapedEntityWrapper();
            
            foreach (var property in requiredProperties)
            {
                var objectPropertyValue = property.GetValue(entity);
                        shapedObject.EntityProperties.TryAdd(property.Name, objectPropertyValue);
            }

            var objectIdProperty = entity.GetType().GetProperty("Id");
            shapedObject.Id = (Guid)objectIdProperty.GetValue(entity);
            
            return shapedObject;
        }
    }
}