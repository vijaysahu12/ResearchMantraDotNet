using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
namespace RM.API.Helpers
{
    public static class ListToDataTable
    {
        public static DataTable ToDataTableByJarvis<T>(this List<T> iList)
        {
            DataTable dataTable = new();
            PropertyDescriptorCollection propertyDescriptorCollection =
                TypeDescriptor.GetProperties(typeof(T));
            for (int i = 0; i < propertyDescriptorCollection.Count; i++)
            {
                PropertyDescriptor propertyDescriptor = propertyDescriptorCollection[i];
                Type type = propertyDescriptor.PropertyType;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = Nullable.GetUnderlyingType(type);
                }

                _ = dataTable.Columns.Add(propertyDescriptor.Name, type);
            }
            object[] values = new object[propertyDescriptorCollection.Count];
            foreach (T iListItem in iList)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = propertyDescriptorCollection[i].GetValue(iListItem);
                }
                _ = dataTable.Rows.Add(values);
            }
            return dataTable;
        }
    }
}