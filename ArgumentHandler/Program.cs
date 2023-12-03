/*
 * chain flags
 * ex. -ha
 * does -h exist? does -a exist?
 * 
 * use --show
 * specify a capture parameter that can be "--option", "-flag" or "parameter"
 * shows all of those options, can call multiple
 * 
 */

using System.Xml.Linq;

namespace ConsoleApp1
{
    static class Program
    {
        public static void Main(string[] args)
        {
            var ArgumentList = new ArgumentManager(args, 
                "b", "yello");
            //--help poopy -hab butthead --yello=blop -b 

            var paramaters = ArgumentList.GetArgs(ArgType.Parameter);

            foreach (var param in paramaters)
            {
                Console.WriteLine($"\"{param}\"");
            }
        }
    }

    /// <summary>
    /// takes the commandline arguments and puts them in a list
    /// </summary>
    public class ArgumentManager
    {
        static IReadOnlyList<Argument> Arguments = null!;

        public ArgumentManager(string[] args, params string[] capturingArgs)
        {
            Arguments = ArgumentParser(args, capturingArgs);
        }

        public static IReadOnlyList<Argument> ArgumentParser(string[] args, string[] capturingArgs)
        {
            List<Argument> l = new List<Argument>();

            for (int i = 0; i < args.Length; ++i)
            {
                string arg = args[i];
                
                // if it starts with a '-', it could be a flag or an option
                if (arg.StartsWith('-'))
                {
                    // remove the '-'
                    arg = arg[1..];

                    // check for a capture with "="
                    string? capture = null;
                    int eqPos = arg.IndexOf('=');

                    // if an equals exists, theres a capture
                    if (eqPos != -1)
                    {
                        capture = arg[(eqPos+1)..];
                        arg= arg[..eqPos];
                    }

                    // check if there is another '-', that means it is an option
                    if (arg.StartsWith('-'))
                    {
                        // remove the second '-'
                        arg = arg[1..];

                        // check for a capture without "="
                        if (capture == null && capturingArgs.Contains(arg) && (i+1 < args.Length) && !args[i+1].StartsWith('-'))
                        {
                            capture = args[++i];
                        }

                        l.Add(new CaptureArgument(arg, ArgType.Option, capture));
                    }
                    //it is a flag
                    else
                    {
                        int j = 0;

                        // check for chained flags
                        if (arg.Length > 1)
                        {
                            for (int jmax = arg.Length-1; j < jmax; ++j)
                            {
                                l.Add(new Argument(arg[j].ToString(), ArgType.Flag));
                            }
                        }
                        var lastFlag = new string(arg[j], 1);

                        // check for a capture without "="
                        if (capture == null && capturingArgs.Contains(lastFlag) && (i + 1 < args.Length) && !args[i + 1].StartsWith('-'))
                        {
                            capture = args[++i];
                        }

                        l.Add(new CaptureArgument(lastFlag, ArgType.Flag, capture));
                    }
                    
                }
                // if no '-' its a parameter
                else
                {
                    l.Add(new Argument(args[i], ArgType.Parameter));
                }
            }

            return l.AsReadOnly();
        }

        public bool Contains(string name)
        {
            foreach (var item in Arguments)
            {
                if (item.Name == name)
                {
                    return true;
                }
            }
            
            return false;
        }

        public bool ContainsAny(params string[] names)
        {
            foreach (var item in Arguments)
            {
                foreach(var name in names)
                {
                    if (item.Name == name)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public int IndexOf(string name)
        {
            for (int i = 0; i < Arguments.Count; ++i)
            {
                if (Arguments[i].Name == name)
                {
                    return i;
                }
            }

            return -1;
        }

        #region GetArgs;
        /// <summary>
        /// Gets all arguments with the specified <paramref name="argumentType"/> and any one of the specified <paramref name="names"/>.
        /// </summary>
        public Argument[] GetArgs(ArgType argumentType, params string[] names)
        {
            return Arguments.Where((arg) =>
            {
                return argumentType.HasFlag(arg.Type) &&
                names.Any((name) => name.Equals(arg.Name));

            }).ToArray();
        }

        public Argument[] GetArgs(ArgType argumentType)
        {
            return Arguments.Where((arg) => argumentType.HasFlag(arg.Type)).ToArray();
        }

        public Argument[] GetArgs()
        {
            return GetArgs(ArgType.Option | ArgType.Flag | ArgType.Parameter);
        }
        #endregion GetArgs;
    }

    [Flags]
    public enum ArgType
    {
        None = 0,
        Parameter = 1, 
        Flag = 2, 
        Option = 4
    }

    /// <summary>
    /// class for storing a single argument
    /// </summary>
    public class Argument
    {
        public readonly string Name;
        public readonly ArgType Type;

        public Argument(string name, ArgType argType)
        {
            Name = name;
            Type = argType;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class CaptureArgument : Argument
    {
        public readonly string? Capture;
        public CaptureArgument(string Name, ArgType argType, string? capture) : base(Name, argType)
        {
            Capture = capture;
        }
    }
}