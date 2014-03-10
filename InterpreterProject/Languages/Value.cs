using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.Languages
{
    public enum ValueType { Integer, String, Boolean }

    public class Value
    {
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

        public Object value;
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
        public Value(string typeString, string valueString)
        {
            switch (typeString)
            {
                case "string":
                    this.value = valueString;
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
        public Value(ValueType type, string value)
        {
            switch (type)
            {
                case ValueType.String:
                    this.value = value;
                    break;
                case ValueType.Integer:
                    this.value = Int32.Parse(value);
                    break;
                case ValueType.Boolean:
                    this.value = Int32.Parse(value) != 0;
                    break;
            }
        }
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
            return (bool)value;
        }
    } 
}
