using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace App.Domain.WEB.Utils
{
    public static class Utils
    {
        public static void AddAllGenericTypes(this IServiceCollection services
                , Type t
                , Assembly[] assemblies
                , bool additionalRegisterTypesByThemself = false
                , ServiceLifetime lifetime = ServiceLifetime.Transient
            )
        {
            var genericType = t;
            var typesFromAssemblies = assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType)));

            foreach (var type in typesFromAssemblies)
            {
                services.Add(new ServiceDescriptor(t, type, lifetime));
                if (additionalRegisterTypesByThemself)
                    services.Add(new ServiceDescriptor(type, type, lifetime));
            }
        }

        public static List<T> CreatePaginatedList<T>(List<T> initialList, int pageSize, int? pageNum)
        {
            if (!pageNum.HasValue)
                return initialList.Take(pageSize).ToList();

            return initialList.Skip((pageNum.Value - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}