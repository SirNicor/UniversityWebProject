using System.Text;
using Dadata;
using Dadata.Model;
using Address = Dadata.Model.Address;

namespace Start.Request;

public static class FunctionForRequest
{
    /*public static StringBuilder PathReturn(string sortKey, Type typeOfClass, StringBuilder? path = null)
    {
        if (path == null)
        {
            path = new StringBuilder("");
        }
        foreach (var field in typeOfClass.GetProperties())
        {
            if (string.Equals(field.Name, sortKey, StringComparison.OrdinalIgnoreCase))
            {
                path.Append(field.Name);
                return path;
            }

            if (!field.PropertyType.IsPrimitive && field.PropertyType != typeof(string)
                                                && field.PropertyType != typeof(DateTime) &&
                                                !field.PropertyType.IsValueType)
            {
                path.Append($"{field.Name}.");
                path = PathReturn(sortKey, field.PropertyType, path);
                if (path.Length != 0)
                {
                    return path;
                }
            }
        }

        path.Length = 0;
        return path;
    }*/
}