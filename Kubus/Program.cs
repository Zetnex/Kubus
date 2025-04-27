using Kubus.lexicalization;
using Kubus.parsing;

// Die Parameter Nodes und andere brauchen noch einen Type (ist nicht gelistet im Ast sondern wird nur geparst und dann vergessen)
// Dann wollte ich noch die Complex.kub parsen

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
        Console.WriteLine("stop");
    }
}