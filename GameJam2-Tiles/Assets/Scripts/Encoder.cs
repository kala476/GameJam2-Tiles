using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;


public enum SolidFileLineType 
{ 
    None, 
    Name, 
    Metric, 
    Vertex, 
    Face, 
}

public class Encoder
{
    private static string indentationString = "  ";

    private static Dictionary<char, SolidFileLineType> charToLineType = new Dictionary<char, SolidFileLineType>()
    {
        {'C', SolidFileLineType.Metric},
        {'V', SolidFileLineType.Vertex},
        {'{', SolidFileLineType.Face}
    };


    //private static void Read_None (string line, int lineIndex)
    //{
    //    return IsEmpty(line);
    //}

    //private static bool IsLineType_Name (string line, int lineIndex)
    //{
    //    return lineIndex == 1;
    //}

    //private static bool IsLineType_Constant (string line, int lineIndex)
    //{
    //    return Regex.IsMatch(line, ) && !IsEquation(line);
    //}

    //private static bool IsLineType_Equation (string line, int lineIndex)
    //{
    //    return Regex.IsMatch(line, @"C\d") && IsEquation(line);
    //}

    //private static bool IsLineType_Vertex (string line, int lineIndex)
    //{
    //    return Regex.IsMatch(line, @"V\d");
    //}

    //private static bool IsLineType_Face(string line, int lineIndex)
    //{
    //    return Regex.IsMatch(line, @"{a-zA-Z0-9 ,}");
    //}

    //private static Dictionary<SolidFileLineType, Action<string>> fileLineInterpreter = new Dictionary<SolidFileLineType, Action<string>>()
    //{

    //};

    //private static Dictionary< SolidFileLineType , Func<string, bool>> fileLineClassifier = new Dictionary<SolidFileLineType, Func<string, bool>>()
    //{
    //    { SolidFileLineType.None, x => IsEmpty(x) },
    //};

    public static string ColorIn(string text, string colName)
    {
        return string.Format("<color={0}>{1}</color>", colName, text);
    }



    // Read data type from text line -------------------------------------------------------------------

    public static SolidFileLineType GetLineType(string line, int lineIndex)
    {

        //Read header line
        if (lineIndex == 1)
        {
            return SolidFileLineType.Name;
        }

        //Skip empty line
        else if (IsEmpty(line))
        {
            return SolidFileLineType.None;
        }
        else
        {
            char c = line[0];
            
            //Read body line
            if (charToLineType.ContainsKey(c))
            {
                return charToLineType[line[0]];
            }

            //Skip unknwon line
            else
            {
                return SolidFileLineType.None;
            }
        }
    }


    // Read data set from text line -------------------------------------------------------------------

    public static void ReadMetric(ref SolidFileBuffer buffer, string line)
    {
        // Capture Groups from Regular Expresion
        string value_encoded = line.Split('=')[1].Replace(" ", "");

        if (Regex.IsMatch(value_encoded, @"[+|-|*|/]|[a-zA-Z]") )
        {
            buffer.polynomials.Add(value_encoded);
        }
        else
        {
            float value_decoded = -1.0f;

            try
            {
                value_decoded = float.Parse(value_encoded);
            }
            catch (FormatException e)
            {
                throw new Exception(value_encoded + " is not a float, Solid: " + buffer.name +"\n"+ e.Message);
            }


            buffer.geometricValues.Add(value_decoded);
        }


    }


   

    public static void ReadVertex(ref SolidFileBuffer buffer, string line)
    {
        //Divide by equal sign
        string[] sides = line.Split('=');

        //Divide by blank space
        string[] str_lst = sides[1].Split(' ');
        List<string> entries = new List<string>();
        entries.AddRange(str_lst);

        //Determin array of values
        int start, end;
        DeterminInterval(entries, out start, out end, false);

        //Get values with symbols
        List<string> values_raw = new List<string>();
        for (int i = start; i <= end; i++)
        {
            values_raw.Add(entries[i]);
        }
        values_raw.RemoveAll(IsEmpty);

        //Get values without symbols
        List<string> values_ref = new List<string>();
        List<bool> values_signed = new List<bool>();
        for (int i = 0; i < values_raw.Count; i++)
        {
            string value = values_raw[i];
            bool signed = false;
            FreeFromNotation(ref value, out signed);

            values_ref.Add(value);
            values_signed.Add(signed);
        }
        
        //Get parsed values
        List<float> values = new List<float>();
        for (int i = 0; i < values_ref.Count; i++)
        {
            string value_ref = values_ref[i];

            //Convert referenced metric to value
            if (values_ref[i].Contains("C"))
            {
                RemoveChar(ref value_ref, 'C');

                try
                {
                    int index = int.Parse(value_ref);
                    float value = buffer.geometricValues[index];
                    float sign = values_signed[i] ? -1.0f : 1.0f;
                    values.Add(sign * value);
                }
                catch (Exception e)
                {
                    Log(value_ref, "PARSE_INT");
                }

            }

            //Parse value directly
            else
            {
                try
                {
                    float value = float.Parse(value_ref);

                    float sign = values_signed[i] ? -1.0f : 1.0f;
                    values.Add(sign * value);
                }
                catch (Exception e)
                {
                    Log(value_ref, "PARSE_FLOAT");
                }
            }
        }
        try
        {
            Vector3 vertex = new Vector3(values[0], values[1], values[2]);

            buffer.vertices.Add(vertex);
        }
        catch (Exception e)
        {
            Log(values.ToArray(), "Vertex");
        }


    }

    public static void ReadFace(ref SolidFileBuffer buffer, string line)
    {
        //Divide by blank space
        string[] split_line = line.Split(' ');
        List<string> entries = new List<string>();
        entries.AddRange(split_line);

        //Determin array of values
        int start, end;
        DeterminInterval(entries, out start, out end, true);


        //Log(new int[] { start, end}, "interval");

        List<string> values_raw = new List<string>();
        for (int i = start; i <= end; i++)
        {
            values_raw.Add(entries[i]);
        }
        values_raw.RemoveAll(IsEmpty);

        List<string> values_ref = new List<string>();
        for (int i = 0; i < values_raw.Count; i++)
        {
            string value = values_raw[i];
            FreeFromNotation(ref value);
            values_ref.Add(value);
        }

        //Log(values_ref.ToArray(), "ref");

        List<int> values = new List<int>();
        for (int i = 0; i < values_ref.Count; i++)
        {
            int value = int.Parse(values_ref[i]);
            values.Add(value);
        }
        
        buffer.polygonList.Add(values);
    }

    // Text Functions ------------------------------------------------------------------------------------------

    public static void DeterminInterval(List<string> list, out int start, out int end, bool curly)
    {
        start = -1;
        end = -1;

        string open, close;

        if (curly)
        {
            open = "{";
            close = "}";
        }
        else
        {
            open = "(";
            close = ")";
        }

        for (int i = 0; i < list.Count; i++)
        {
            string entry = list[i];
            if (entry.Length == 1)
            {
                if (entry == open)
                {
                    start = i + 1;
                }
                else if (entry == close)
                {
                    end = i - 1;
                }
            }
            else
            {
                if (entry.Contains(open))
                {
                    start = i;
                }
                else if (entry.Contains(close))
                {
                    end = i;
                }
            }
        }
    }

  

    public static bool IsNotation(char c)
    {
        bool curly_brackets = c == '{' || c == '}';
        bool square_brackets = c == '[' || c == ']';
        bool round_brackets = c == '(' || c == ')';
        bool compare = c == '<' || c == '>';
        bool comma = c == ',' || c == ';';
        bool minus = c == '-';

        return curly_brackets || square_brackets || round_brackets || compare || comma || minus;
    }

    public static bool ContainsAny(string s, string[] marks)
    {
        for(int i = 0; i < marks.Length; i++)
        {
            if (s.Contains(marks[i]))
            {
                return true;
            }
        }
        return false;
    }


    public static string[] brackets = new string[] { "{", "[", "(", "<", ">", ")", "]", "}"};
    public static string[] oparators = new string[] {"+", /*"-",*/ "*", "/", "sqrt"};                   // has to exclude signed numbers

    public static bool IsEquation(string s)
    {
        bool hasOperators = ContainsAny(s, oparators);
        bool hasBrackets = ContainsAny(s, brackets);
        //bool hasLetters = Regex.Matches(s, @"[a-zA-Z]").Count > 0;
        return hasOperators && hasBrackets/* && hasLetters*/;  
    }

    public static bool IsSingleSymbol(string s)
    {
        string[] symbols = { "(", ")", "{", "}" };

        for (int i = 0; i < symbols.Length; i++)
        {
            if (s.Contains(symbols[i]))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsEmpty(string s)
    {
        if (s.Length == 0) return true;

        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] != ' ') return false;
        }

        return true;
    }

    public static bool IsBlankSpace(char c)
    {
        return c == ' ';
    }

    public static void RemoveChar(ref string s, char c)
    {
        string h = "";
        List<char> cl = new List<char>();
        cl.AddRange(s);
        cl.Remove(c);
        for (int i = 0; i < cl.Count; i++) h += cl[i];
        s = h;
    }

    public static void FreeFromNotation(ref string s, out bool negative)
    {
        //Convert to list
        List<char> list = new List<char>();
        list.AddRange(s);

        //IsNegative
        negative = list.Contains('-');

        //Filter
        list.RemoveAll(IsNotation);

        //Convert back
        string h = "";
        for (int i = 0; i < list.Count; i++) h += list[i];
        s = h;
    }

    public static void FreeFromNotation(ref string s)
    {
        //Convert to list
        List<char> list = new List<char>();
        list.AddRange(s);

        //Filter
        list.RemoveAll(IsNotation);

        //Convert back
        string h = "";
        for (int i = 0; i < list.Count; i++) h += list[i];
        s = h;
    }


    // Logging Functions ----------------------------------------------------------------------------------------------------------------------------


    public static void Log<T>(T s, string name)
    {
        Debug.Log(name + " : " + s);
    }

    public static string CompileTextBlock<T>(T[] entries, int indent)
    {
        string line = "{ ";
        indent++;
        for (int x = 0; x < entries.Length; x++)
        {
            T entryObject = entries[x];
            TypeInfo typeInfo = entryObject.GetType().GetTypeInfo();
            string entryText = "";
            bool isLast = x < entries.Length - 1;
            
            // Read Nested Generic Containers
            if (typeInfo.IsGenericType)
            {
                int typeArgCount = typeInfo.GetGenericArguments().Length;

                if (typeArgCount == 1)
                {
                    string genericTypeName = typeInfo.GetGenericTypeDefinition().ToString().Split('.').ToList().Last().Split('`')[0];
                    Type genericType = typeInfo.GetGenericTypeDefinition();
                    Type genericTypArg = typeInfo.GetGenericArguments()[0];


                    // Handle Generic List
                    if (genericTypeName.Equals("List"))
                    {
                        string enumSymbol = isLast ? ", " : "\n}";

                        Type typeArg = typeInfo.GenericTypeArguments[0];
                        MethodInfo toArray = typeInfo.GetMethod("ToArray")/*.MakeGenericMethod(new Type[] { typeArg })*/;
                        MethodInfo compile = typeof(Encoder).GetMethod("CompileTextBlock").MakeGenericMethod(typeArg);
                        object nestedList = toArray.Invoke(entryObject, new object[0]);
                        string lineIndent = string.Concat(Enumerable.Repeat(indentationString, indent));
                        entryText = string.Format(
                            "{0}{1}{2}{3}",
                            "\n",
                            lineIndent,
                            compile.Invoke(null, new object[] { nestedList, indent}) as string,
                            enumSymbol);
                    }
                    else
                    {
                        throw new Exception(string.Format("CompileTextBlock<T> handles nested generic type List<T>, not {0}", genericTypeName));
                    }

                }
                else
                {
                    throw new Exception(string.Format("CompileTextBlock<T> handles nested generic types with only one type argument"));
                }

            }
            else
            {
                string enumSymbol = isLast ? ", " : "}";
                entryText = entryObject.ToString() + enumSymbol;
            }

            line += entryText;
        }

        return line;
    }

    public static void Log<T>(T[] s, string message)
    {
        Type type = typeof(T);
        FieldInfo nameField = type.GetField("name");

        string name = nameField == null ? s.ToString() : nameField.GetValue(s) as string;

        string textBlock = string.Format("{0}\n{1}", message, CompileTextBlock(s, 0));
        

        Debug.Log(textBlock);
    }

    public static void Log(Solid solid)
    {
        //string solidName = solid.name;
        //Log(solid.polygonCounts, solidName + ".polygonCounts");
        //Log(solid.polygonNormals, solidName + ".polygonNormals");
        //Log(solid.polygonPositions, solidName + ".polygonPositions");
        //Log(solid.polygonOrientations, solidName + ".polygonOrientations");
        //Log(solid.ngons, solidName + ".ngons");
    }

}
