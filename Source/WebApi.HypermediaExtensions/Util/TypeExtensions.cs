namespace WebApi.HypermediaExtensions.Util
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public static class TypeExtension
    {
        public static string BeautifulName(this Type t)
        {
            if (!t.IsConstructedGenericType)
            {
                return !t.IsNested ? t.Name : $"{t.DeclaringType.BeautifulName()}.{t.Name}";
            }

            try
            {
                var sb = new StringBuilder();

                var index = t.Name.LastIndexOf("`", StringComparison.Ordinal);
                if (index < 0)
                { 
                    return t.Name;
                }


                sb.Append(t.Name.Substring(0, index));
                var i = 0;
                t.GetGenericArguments().Aggregate(
                    sb,
                    (a, type) => a.Append(i++ == 0 ? "<" : ",").Append(BeautifulName(type)));
                sb.Append(">");

                return sb.ToString();
            }
            catch (Exception)
            {
                return t.Name;
            }
        }
    }
}