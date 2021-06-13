using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UltimateWebApi.ModelBinders
{
    public class ArrayModelBinder: IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if(!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            var providedValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString(); // Getting parameter (in our case it a string which contains the guids)
            if(string.IsNullOrEmpty(providedValue))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            var geneticType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0]; // Get type from IEnumerable parameter (in our case it is Guid)
            var converter = TypeDescriptor.GetConverter(geneticType); // Create converter to Guid type

            var objectArray = providedValue.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(x => converter.ConvertFromString(x.Trim()))
                                            .ToArray(); // From strings to guids
            var guidArray = Array.CreateInstance(geneticType, objectArray.Length); // Creates an array template of guid type in which will be copied a object type array, which above.
            objectArray.CopyTo(guidArray, 0);
            bindingContext.Model = guidArray; 

            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;
        }
    }
}
