using System;
using System.Collections.Generic;

namespace Savescum
{
    class ArgumentProperties
    {
        protected Dictionary<string, string> argumentMap;

        public ArgumentProperties(string[] args, string separator)
        {
            argumentMap = new Dictionary<string, string>(args.Length);

            foreach (string argument in args)
            {
                String[] argumentParts = argument.Split(separator);
                if (argumentParts.Length != 2)
                {
                    throw new ArgumentException("Improperly formed argument: " + argument);
                }

                argumentMap.Add(argumentParts[0], argumentParts[1]);
            }
        }

        internal string GetString(string name, string defaultValue)
        {
            if (!argumentMap.TryGetValue(name, out string value))
            {
                if (null != defaultValue)
                {
                    value = defaultValue;
                }

                else
                {
                    throw new ArgumentException("Required argument not found: " + name);
                }
            }

            return value;
        }

        internal int GetInteger(string name, string defaultValue)
        {
            string valueString = GetString(name, defaultValue);
            return Int32.Parse(valueString);
        }
    }
}
