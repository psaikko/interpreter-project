using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.Languages
{
    // Mini-PL value types
    public enum ValueType { Integer, String, Boolean }

    // Representation of a Mini-PL value
    public class Value
    {
        Object value;
        // dynamically determine value type
        public ValueType Type()
        {
            if (value is Int32)
                return ValueType.Integer;
            if (value is String)
                return ValueType.String;
            if (value is Boolean)
                return ValueType.Boolean;
            else
                throw new Exception("UNEXPECTED VARIABLE TYPE");
        }
        // create a value from type- and value strings
        public Value(string typeString, string valueString)
        {
            switch (typeString)
            {
                case "string":
                    this.value = Unescape(valueString.Substring(1, valueString.Length - 2));
                    break;
                case "int":
                    this.value = Int32.Parse(valueString);
                    break;
                case "bool":
                    this.value = Int32.Parse(valueString) != 0;
                    break;
                default:
                    throw new Exception("UNEXPECTED TYPE STRING");
            }
        }
        // create value from type enum and value string
        public Value(ValueType type, string valueString)
        {
            switch (type)
            {
                case ValueType.String:
                    this.value = Unescape(valueString.Substring(1, valueString.Length - 2));
                    break;
                case ValueType.Integer:
                    this.value = Int32.Parse(valueString);
                    break;
                case ValueType.Boolean:
                    this.value = Int32.Parse(valueString) != 0;
                    break;
            }
        }
        // create value with default value for the type
        public Value(ValueType type)
        {
            switch (type)
            {
                case ValueType.String:
                    this.value = "";
                    break;
                case ValueType.Integer:
                    this.value = 0;
                    break;
                case ValueType.Boolean:
                    this.value = false;
                    break;
            }
        }
        // create value from primitive type
        public Value(bool value)
        {
            this.value = value;
        }
        public Value(int value)
        {
            this.value = value;
        }
        public Value(string value)
        {
            this.value = value;
        }

        // get typed values
        public int IntValue()
        {
            return (int)value;
        }
        public string StringValue()
        {
            return (string)value;
        }
        public bool BooleanValue()
        {
            // integers can be iterpreted as booleans
            if (Type() == ValueType.Boolean)
                return (bool)value;
            else
                return 0 != (int)value;
        }

        private static readonly string[] escapeStrings =
            new string[] { "\\a", "\\b", "\\f", "\\n", "\\r", "\\t", "\\v", "\\\\", "\\'", "\\\"", "\\0" };
        private static readonly string[] escapeChars =
            new string[] { "\a", "\b", "\f", "\n", "\r", "\t", "\v", "\\", "\'", "\"", "\0" };

        // unescape raw string values from text
        private static string Unescape(string s)
        {
            for (int i = 0; i < escapeStrings.Length; i++)
                s = s.Replace(escapeStrings[i], escapeChars[i]);
            return s;
        }

        // parse a type string
        public static ValueType TypeFromString(string s)
        {
            ValueType type;
            switch (s)
            {
                case "int":
                    type = ValueType.Integer;
                    break;
                case "bool":
                    type = ValueType.Boolean;
                    break;
                case "string":
                    type = ValueType.String;
                    break;
                default:
                    throw new Exception("UNEXPECTED TYPE STRING");
            }
            return type;
        }
    }
}
