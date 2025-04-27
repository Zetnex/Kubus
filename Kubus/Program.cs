using Kubus.lexicalization;
using Kubus.parsing;
using Kubus.transpilation;

class Program
{
    public static void Main(string[] args)
    {
        if(args.Length == 0)
        {
            Console.WriteLine("Please provide a source file.");
            return;
        }
     
        if(!File.Exists(args[0]))
        {
            Console.WriteLine("The provided source file does not exist.");
            return;
        }
        
        // now the -y flag to skip the output question
        bool skipOutputQuestion = args.Contains("-y");
        if (skipOutputQuestion)
        {
            args = args.Where(arg => arg != "-y").ToArray();
        }
        
        double startTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
        
        // Read the source file
        string sourceFilePath = args[0];
        string rawSource = File.ReadAllText(sourceFilePath);

        Lexer lexer = new Lexer();
        LexingData.ApplyDefinitions(lexer);
        Token[] tokens = lexer.Tokenize(rawSource).ToArray();
        
        // filter out comments
        tokens = tokens.Where(t => t.Type != "single_line_comment" && t.Type != "multi_line_comment").ToArray();
        
        Parser parser = new Parser(tokens);
        Ast ast = parser.Parse();
        
        // transpile to PHP
        Transpiler transpiler = new Transpiler(ast);
        string phpSource = transpiler.Transpile();
        string? outputPath = Path.GetDirectoryName(sourceFilePath);
        string fileName = Path.GetFileNameWithoutExtension(sourceFilePath) + ".php";

        if (outputPath == null)
        {
            Console.WriteLine("Something unexpected happened, no output.");
            return;
        }
        
        double endTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
        double elapsedTime = endTime - startTime;
        Console.WriteLine($"Finished after {elapsedTime}ms.");
        
        Output(phpSource, outputPath, fileName, skipOutputQuestion);
    }

    public static void Output(string phpSource, string? path, string fileName, bool skipOutputQuestion)
    {
        // check if the folder exists at the path
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException();
        }
        
        // check if the folder "path/output" exists
        if (!Directory.Exists(Path.Combine(path, "output")))
        {
            Directory.CreateDirectory(Path.Combine(path, "output"));
        }
        
        // check if the file exists
        if (File.Exists(Path.Combine(path, "output", fileName)) && !skipOutputQuestion)
        {
            Console.WriteLine("The file already exists. Do you want to overwrite it? (y/n)");
            string answer = Console.ReadLine();
            if (answer != "y")
            {
                return;
            }
        }
        
        // write the phpSource to the file
        File.WriteAllText(Path.Combine(path, "output", fileName), phpSource);
        Console.WriteLine($"The file has been written to {Path.Combine(path, "output", fileName)}");
    }
}