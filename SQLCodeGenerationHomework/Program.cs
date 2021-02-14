using System.Collections.Generic;
using System.Reflection;
using System;

namespace SQLCodeGenerationHomework
{
    class Program
    {
        static void Main(string[] args)
        {
            //C:\Users\Игорь Поливин\source\repos\SQL_Library\ClassCar\bin\Debug\netcoreapp3.1\SQL_Library.dll
            string[] fileExtensions = new string[] { ".exe", ".dll" };
            string path = string.Empty;
            const int fileTypeLength = 4;

            Dictionary<string, string> AllTypes = new Dictionary<string, string>
            {
                {"System.Boolean", "BOOLEAN"},
                { "System.String", "NVARCHAR(MAX)"},
                { "System.Char", "NVARCHAR(MAX)"},
                {"System.Int32", "INT"},
                {"System.Double", "DECIMAL"}

            };

            while (true)
            {
                string fileType = string.Empty;
                Console.Write("Введите путь до файла:");
                string input = Console.ReadLine();
                fileType = input.Substring(input.Length - 4);
                if (input.Length >= fileTypeLength && (fileType == fileExtensions[0] || fileType == fileExtensions[1]))
                {
                    path = input;
                    break;
                }
                else
                {
                    Console.WriteLine("Вы ввели неподходящее значение!!!");
                    Console.ReadLine();
                    Console.Clear();
                    continue;
                }
            }


            Assembly myLibrary;
            try
            {
                myLibrary = Assembly.LoadFile(path);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return;
            }


            foreach (Type type in myLibrary.GetTypes())
            {
                if (isDataClass(type) && type.IsClass)
                {

                    string sqlScript = "CREATE TABLE ";
                    sqlScript += type.Name + " \n(\n";

                    int countProperty = new int();
                    foreach (var member in type.GetMembers())
                        if (member is PropertyInfo)
                            countProperty++;

                    int counter = new int();
                    foreach (var member in type.GetMembers())
                    {
                        if (member is PropertyInfo)
                        {
                            counter++;
                            var propertyInfo = member as PropertyInfo;
                            string sqlType = string.Empty;
                            AllTypes.TryGetValue(propertyInfo.PropertyType.FullName, out sqlType);
                            sqlScript += $"\t{propertyInfo.Name} {sqlType} NOT NULL";
                            if (propertyInfo.Name.ToLower() == "id")
                                sqlScript += " PRIMARY KEY IDENTITY ";

                            if (counter < countProperty)
                                sqlScript += ",";
                            sqlScript += "\n";

                        }

                    }
                    sqlScript += ");";
                    Console.WriteLine();
                    Console.WriteLine();

                    Console.WriteLine(sqlScript);
                }
            }
        }

        private static bool isDataClass(Type type)
        {
            List<string> propertyNames = new List<string>();

            List<string> defaultMethods = new List<string>()
            {
                "GetHashCode",
                "GetType",
                "Equals",
                "ToString"
            };

            foreach (var member in type.GetMembers())
            {
                if (member is PropertyInfo)
                {

                    propertyNames.Add(member.Name);

                }
            }

            foreach (var member in type.GetMembers())
            {

                if (member is MethodInfo)
                {
                    if (defaultMethods.Contains(member.Name))
                        continue;
                    else
                    {
                        if (propertyNames.Contains(member.Name.Substring(4)))
                        {
                            continue;
                        }
                        else
                            return false;
                    }

                }

            }
            return true;
        }
    }
}
