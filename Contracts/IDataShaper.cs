using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Entities.Models;

namespace Contracts
{
    public interface IDataShaper<T>
    {
        IEnumerable<ShapedEntityWrapper> ShapeData(IEnumerable<T> entities, string fieldsString);
        ShapedEntityWrapper ShapeData(T entity, string fieldsString);
    }
}