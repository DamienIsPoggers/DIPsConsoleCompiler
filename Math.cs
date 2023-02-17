using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIPsConsoleCompiler
{
    public class Math
    {
        public enum types
        {
            equals = 0,
            less = 1,
            greater = 2,
            lessEqual = 3,
            greaterEqual = 4,
            add = 5,
            sub = 6,
            mul = 7,
            div = 8,
            remainder = 9,
            gets = 10,
        }

        public types getMathTypeString(string type)
        {
            switch(type)
            {
                case "==":
                    return types.equals;
                case "<":
                    return types.less;
                case ">":
                    return types.greater;
                case "<=":
                    return types.lessEqual;
                case ">=":
                    return types.greaterEqual;
                case "+":
                    return types.add;
                case "-":
                    return types.sub;
                case "*":
                    return types.mul;
                case "/":
                    return types.div;
                case "%":
                    return types.remainder;
                case "=":
                    return types.gets;
                default:
                    return types.equals;
            }
        }

        public types getMathTypeInt(int type)
        {
            switch (type)
            {
                case 0:
                    return types.equals;
                case 1:
                    return types.less;
                case 2:
                    return types.greater;
                case 3:
                    return types.lessEqual;
                case 4:
                    return types.greaterEqual;
                case 5:
                    return types.add;
                case 6:
                    return types.sub;
                case 7:
                    return types.mul;
                case 8:
                    return types.div;
                case 9:
                    return types.remainder;
                case 10:
                    return types.gets;
                default:
                    return types.equals;
            }
        }

        public byte convertToByte(types type)
        {
            switch(type)
            {
                case types.equals:
                    return 0;
                case types.less:
                    return 1;
                case types.greater:
                    return 2;
                case types.lessEqual:
                    return 3;
                case types.greaterEqual:
                    return 4;
                case types.add:
                    return 5;
                case types.sub:
                    return 6;
                case types.mul:
                    return 7;
                case types.div:
                    return 8;
                case types.remainder:
                    return 9;
                case types.gets:
                    return 10;
                default:
                    return 0;
            }
        }
    }
}
