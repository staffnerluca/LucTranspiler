using System;
using System.Collections.Generic;

public static class FunctionMapping
{
    public static readonly Dictionary<string, List<string>> DirectMapping = new Dictionary<string, List<string>>()
    {
        /* Basic Input/Output */
        {"print", new List<string>{"Console.WriteLine", "parameter", "using System"}},
        {"read", new List<string>{"Console.ReadLine", "parameter", "using System"}},
        {"read_nex_input", new List<string>{"Console.ReadKey", "parameter", "using System"}},

        /* String Methods */
        {"len", new List<string>{"Count", "object", ""}},
        {"len_of_text", new List<string>{"Length", "object", ""}},
        {"upper", new List<string>{"ToUpper", "object", ""}},
        {"lower", new List<string>{"ToLower", "object", ""}},
        {"substring", new List<string>{"Substring", "object", ""}},
        {"replace", new List<string>{"Replace", "object", ""}},
        {"contains", new List<string>{"Contains", "object", ""}},
        {"begins_with", new List<string>{"StartsWith", "object", ""}},
        {"ends_with", new List<string>{"EndsWith", "object", ""}},
        {"remove_blank_space", new List<string>{"Trim", "object", ""}},
        {"remove_blank_space_from_start", new List<string>{"TrimStart", "object", ""}},
        {"remove_blank_space_from_end", new List<string>{"TrimEnd", "object", ""}},
        {"split", new List<string>{"Split", "object", ""}},
        {"join", new List<string>{"string.Join", "parameter", "using System"}},
        {"get_first_index_of_characters", new List<string>{"IndexOf", "object", ""}},
        {"get_last_index_of_characters", new List<string>{"LastIndexOf", "object", ""}},
        {"strings_equal", new List<string>{"string.Equals", "parameter", "using System"}},

        /* Math Methods */
        {"absolut_value", new List<string>{"Math.Abs", "parameter", "using System"}},
        {"max", new List<string>{"Math.Max", "parameter", "using System"}},
        {"min", new List<string>{"Math.Min", "parameter", "using System"}},
        {"power", new List<string>{"Math.Pow", "parameter", "using System"}},
        {"sqrt", new List<string>{"Math.Sqrt", "parameter", "using System"}},
        {"round", new List<string>{"Math.Round", "parameter", "using System"}},
        {"to_whole_number_without_rounding", new List<string>{"Math.Truncate", "parameter", "using System"}},
        {"round_down", new List<string>{"Math.Floor", "parameter", "using System"}},
        {"round_up", new List<string>{"Math.Ceiling", "parameter", "using System"}},
        {"log", new List<string>{"Math.Log", "parameter", "using System"}},
        {"log10", new List<string>{"Math.Log10", "parameter", "using System"}},
        {"exponential", new List<string>{"Math.Exp", "parameter", "using System"}},
        {"sin", new List<string>{"Math.Sin", "parameter", "using System"}},
        {"cos", new List<string>{"Math.Cos", "parameter", "using System"}},
        {"tan", new List<string>{"Math.Tan", "parameter", "using System"}},
        {"asin", new List<string>{"Math.Asin", "parameter", "using System"}},
        {"acos", new List<string>{"Math.Acos", "parameter", "using System"}},
        {"atan", new List<string>{"Math.Atan", "parameter", "using System"}},
        {"atan_with_two_arguments", new List<string>{"Math.Atan2", "parameter", "using System"}},
        {"normalilze_values_to_be_between_min_and_max", new List<string>{"Math.Clamp", "parameter", "using System"}},

        /* List/Collection Methods */
        //{"len", new List<string>{"Count", "object", "using System.Collections.Generic"}},
        {"add_to_list", new List<string>{"Add", "object", "using System.Collections.Generic"}},
        {"remove_from_list", new List<string>{"Remove", "object", "using System.Collections.Generic"}},
        {"remove_at_index", new List<string>{"RemoveAt", "object", "using System.Collections.Generic"}},
        {"clear", new List<string>{"Clear", "object", "using System.Collections.Generic"}},
        {"insert_at_index", new List<string>{"Insert", "object", "using System.Collections.Generic"}},
        {"get_first_index_of", new List<string>{"IndexOf", "object", "using System.Collections.Generic"}},
        {"sort", new List<string>{"Sort", "object", "using System.Collections.Generic"}},
        {"reverse", new List<string>{"Reverse", "object", "using System.Collections.Generic"}},
        {"find", new List<string>{"Find", "object", "using System.Collections.Generic"}},
        {"find_all", new List<string>{"FindAll", "object", "using System.Collections.Generic"}},
        {"search", new List<string>{"BinarySearch", "object", "using System.Collections.Generic"}},

        /* LINQ Methods */
        // are kept the same because they are mostly the same as in SQL and or target audience probably knows SQL
        {"select", new List<string>{"Select", "object", "using System.Linq"}},
        {"where", new List<string>{"Where", "object", "using System.Linq"}},
        {"first_value", new List<string>{"First", "object", "using System.Linq"}},
        {"first_or_default", new List<string>{"FirstOrDefault", "object", "using System.Linq"}},
        {"last_value", new List<string>{"Last", "object", "using System.Linq"}},
        {"last_or_default", new List<string>{"LastOrDefault", "object", "using System.Linq"}},
        {"single", new List<string>{"Single", "object", "using System.Linq"}},
        {"single_or_default", new List<string>{"SingleOrDefault", "object", "using System.Linq"}},
        {"sum", new List<string>{"Sum", "object", "using System.Linq"}},
        {"average", new List<string>{"Average", "object", "using System.Linq"}},
        {"count", new List<string>{"Count", "object", "using System.Linq"}},
        {"max_of_list", new List<string>{"Max", "object", "using System.Linq"}},
        {"min_of_list", new List<string>{"Min", "object", "using System.Linq"}},
        {"order_by", new List<string>{"OrderBy", "object", "using System.Linq"}},
        {"order_by_descending", new List<string>{"OrderByDescending", "object", "using System.Linq"}},
        {"group_by", new List<string>{"GroupBy", "object", "using System.Linq"}},
        {"distinct", new List<string>{"Distinct", "object", "using System.Linq"}},
        {"to_list", new List<string>{"ToList", "object", "using System.Linq"}},

        /* File and Directory Methods */
        {"read_file", new List<string>{"File.ReadAllText", "parameter", "using System.IO"}},
        {"write_file", new List<string>{"File.WriteAllText", "parameter", "using System.IO"}},
        {"append_file", new List<string>{"File.AppendAllText", "parameter", "using System.IO"}},
        {"read_lines", new List<string>{"File.ReadLines", "parameter", "using System.IO"}},
        {"does_file_exist", new List<string>{"File.Exists", "parameter", "using System.IO"}},
        {"delete_file", new List<string>{"File.Delete", "parameter", "using System.IO"}},
        {"create_directory", new List<string>{"Directory.CreateDirectory", "parameter", "using System.IO"}},
        {"delete_directory", new List<string>{"Directory.Delete", "parameter", "using System.IO"}},
        {"list_files", new List<string>{"Directory.GetFiles", "parameter", "using System.IO"}},
        {"list_directories", new List<string>{"Directory.GetDirectories", "parameter", "using System.IO"}},
        {"move_file", new List<string>{"File.Move", "parameter", "using System.IO"}},
        {"copy_file", new List<string>{"File.Copy", "parameter", "using System.IO"}},

        /* Dictionary Methods */
        {"add_to_dictionary", new List<string>{"Add", "object", "using System.Collections.Generic"}},
        {"remove_from_dictionary", new List<string>{"Remove", "object", "using System.Collections.Generic"}},
        {"dictionary_contains_key", new List<string>{"ContainsKey", "object", "using System.Collections.Generic"}},
        {"dictionary_contains_value", new List<string>{"ContainsValue", "object", "using System.Collections.Generic"}},
        {"get_dictionary_keys", new List<string>{"Keys", "object", "using System.Collections.Generic"}},
        {"get_dictionary_values", new List<string>{"Values", "object", "using System.Collections.Generic"}},

        /* Date and Time Methods */
        {"now", new List<string>{"DateTime.Now", "parameter", "using System"}},
        {"utc_now", new List<string>{"DateTime.UtcNow", "parameter", "using System"}},
        {"add_days_to_date", new List<string>{"AddDays", "object", ""}},
        {"add_hours_to_date", new List<string>{"AddHours", "object", ""}},
        {"to_string", new List<string>{"Convert.ToString", "parameter", ""}},
        {"format_date", new List<string>{"DateTime.Parse", "parameter", "using System"}},
        {"try_parse_date", new List<string>{"DateTime.TryParse", "parameter", "using System"}},
        {"get_day_of_week", new List<string>{"DayOfWeek", "object", ""}},
        {"get_in_100_nanoseconds", new List<string>{"Ticks", "object", ""}}
    };

    // functinos where some parts of the parameters are pre determined
    
    // functions where the ouput was changed
    //{"is_positive_or_zeros", new List<string>{"Math.Sign", "parameter", "using System"}},
    // Math.Sign outputs +1 if the number is positive, 0 if it is 0 and -1 if it is negatively
}