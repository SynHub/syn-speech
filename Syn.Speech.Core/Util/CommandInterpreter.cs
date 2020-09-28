//INCOMPLETE
namespace Syn.Speech.Util
{
    ///// <summary>
    ///// This class is a command interpreter. It reads strings from an input stream, parses them into commands and executes
    ///// them, results are sent back on the output stream.
    /////
    ///// @see CommandInterpreter
    ///// </summary>
    ///// 
    //public class CommandInterpreter    
    //{
    //    private HashMap<String, ICommandInteface> _commandList;
    //    private int _totalCommands;
    //    private StreamReader _in;
    //    private StreamWriter _out;
    //    private string _prompt;
    //    private Boolean _done;
    //    private Boolean _trace;
    //    private CommandHistory _history;

    //    private Socket _socket;


    //    /**
    //    /// Creates a command interpreter that reads/writes on the given streams.
    //     *
    //    /// @param in  the input stream.
    //    /// @param out the output stream.
    //     */

    //    public CommandInterpreter(StreamReader @in, StreamWriter @out) 
    //    {
    //        Init(@in, @out);
            
    //    }


    //    /**
    //    /// Sets the trace mode of the command interpreter.
    //     *
    //    /// @param trace true if tracing.
    //     */

    //    public void SetTrace(Boolean trace) 
    //    {
    //        this._trace = trace;
    //    }


    //    /** Creates a command interpreter that won't read a stream. */

    //    public CommandInterpreter() 
    //    {
    //        var bin = new StreamReader(Console.OpenStandardInput());
    //        var pw = new StreamWriter(Console.OpenStandardOutput());
    //        Init(bin, pw);
    //    }


    //    /** Initializes the CI */

    //    private void Init(StreamReader p_in, StreamWriter p_out) 
    //    {
    //        _commandList = new HashMap<string, ICommandInteface>();
    //        addStandardCommands();
    //        SetStreams(p_in, p_out);
    //        _history = new CommandHistory(this);
    //    }


    //    /**
    //    /// Sets the I/O streams
    //     *
    //    /// @param in the input stream.
    //    /// @param    out    the output stream.
    //     */
    //    public void SetStreams(StreamReader p_in, StreamWriter p_out) 
    //    {
    //        _in = p_in;
    //        _out = p_out;
    //    }


    //    /** Returns the Socket this CommandInterpreter uses. */
    //    public Socket GetSocket() 
    //    {
    //        return _socket;
    //    }


    //    /**
    //    /// Sets the Socket for this CommandInterpreter.
    //     *
    //    /// @param skt the Socket this CommandInterpreter uses
    //     */
    //    public void SetSocket(Socket skt) 
    //    {
    //        _socket = skt;
    //    }

    //    public string Help(CommandInterpreter ci, String[] args) 
    //    {
    //        dumpCommands();
    //        return "";
    //    }

    //    public string historyCmd(CommandInterpreter ci, String[] args) 
    //    {
    //        _history.dump();
    //        return "";
    //    }

    //     public string status(CommandInterpreter ci, String[] args) 
    //     {
    //        putResponse("Total number of commands: " + _totalCommands);
    //        return "";
    //    }

    //    public string echo(CommandInterpreter ci, String[] args) 
    //    {
    //        var b = new StringBuilder(80);

    //        for (var i = 1; i < args.Length; i++) 
    //        {
    //            b.Append(args[i]);
    //            b.Append(' ');
    //        }
    //        putResponse(b.ToString());
    //        return "";
    //    }

    //    public string quit(CommandInterpreter ci, String[] args) 
    //    {
    //        _done = true;
    //        return "";
    //    }

    //    public string on_exit(CommandInterpreter ci, String[] args) 
    //    {
    //        return "";
    //    }

    //    public string version(CommandInterpreter ci, String[] args) 
    //    {
    //        putResponse("Command Interpreter - Version 1.1 ");
    //        return "";
    //    }

    //    public string gc(CommandInterpreter ci, String[] args) 
    //    {
    //        //Runtime.getRuntime().gc();
    //        return "";
    //    }

    //    public string memory(CommandInterpreter ci, String[] args) 
    //    {
    //        //long totalMem = Runtime.getRuntime().totalMemory();
    //        //long freeMem = Runtime.getRuntime().freeMemory();

    //        //putResponse("Free Memory  : "
    //        //        + freeMem / (1024.0/// 1024) + " mbytes");
    //        //putResponse("Total Memory : "
    //        //        + totalMem / (1024.0/// 1024) + " mbytes");
    //        return "";
    //    }

    //    public string delay(CommandInterpreter ci, String[] args) 
    //    {
    //        if (args.Length == 2) 
    //        {
    //            try {
    //                var seconds = Convert.ToSingle(args[1]);
    //                Thread.Sleep((int)(seconds* 1000));
    //            } 
    //            catch (FormatException) 
    //            {
    //                putResponse("Usage: delay time-in-seconds");
    //            } 
    //            catch (Exception) 
    //            {
    //            }
    //        } else {
    //            putResponse("Usage: delay time-in-seconds");
    //        }
    //        return "";
    //    }

    //   public string repeat(CommandInterpreter ci, String[] args) 
    //   {
    //        //if (args.Length >= 3) {
    //        //    try {
    //        //        int count = Integer.parseInt(args[1]);
    //        //        String[] subargs = Arrays.copyOfRange(args, 2, args.length);
    //        //        for (int i = 0; i < count; i++) {
    //        //            putResponse(CommandInterpreter.this.execute(subargs));
    //        //        }
    //        //    } catch (NumberFormatException nfe) {
    //        //        putResponse("Usage: repeat count command args");
    //        //    }
    //        //} else {
    //        //    putResponse("Usage: repeat count command args");
    //        //}
    //        return "";
    //    }

    //    public string load(CommandInterpreter ci, String[] args) 
    //    {
    //        //if (args.Length == 2) {
    //        //    if (!load(args[1])) {
    //        //        putResponse("load: trouble loading " + args[1]);
    //        //    }
    //        //} else {
    //        //    putResponse("Usage: load filename");
    //        //}
    //        return "";
    //    }

    //    public string chain(CommandInterpreter ci, String[] args) 
    //    {
    //        //if (args.Length > 1) {
    //        //    String[] subargs = new String[args.length - 1];
    //        //    List<String[]> commands = new ArrayList<String[]>(5);
    //        //    int count = 0;
    //        //    for (int i = 1; i < args.length; i++) {
    //        //        if (args[i].equals(";")) {
    //        //            if (count > 0) {
    //        //                commands.add(Arrays.copyOf(subargs, count));
    //        //                count = 0;
    //        //            }
    //        //        } else {
    //        //            subargs[count++] = args[i];
    //        //        }
    //        //    }

    //        //    if (count > 0) {
    //        //        commands.add(Arrays.copyOf(subargs, count));
    //        //    }

    //        //    for (String[] command : commands) {
    //        //        putResponse(CommandInterpreter.this.execute(command));
    //        //    }
    //        //} else {
    //        //    putResponse("Usage: chain cmd1 ; cmd2 ; cmd3 ");
    //        //}
    //        return "";
    //    }

    //    public string time(CommandInterpreter ci, String[] args) 
    //    {
    //        //if (args.length > 1) {
    //        //    String[] subargs = Arrays.copyOfRange(args, 1, args.length);
    //        //    long startTime = System.currentTimeMillis();
    //        //    long endTime;

    //        //    putResponse(CommandInterpreter.this.execute(subargs));
    //        //    endTime = System.currentTimeMillis();

    //        //    putResponse("Time: " + ((endTime - startTime) / 1000.0)
    //        //            + " seconds");

    //        //} else {
    //        //    putResponse("Usage: time cmd [args]");
    //        //}
    //        return "";
    //    }



    //    /** Adds the set of standard commands */

    //    private void addStandardCommands() 
    //    {
    //        add("help", new ICommandInteface(Help, "lists available commands"));

    //        add("history", new ICommandInteface(historyCmd,"shows command history"));

    //        add("status", new ICommandInteface(status,"shows command status"));

    //        add("echo", new ICommandInteface(echo, "display a line of text"));

    //        add("quit", new ICommandInteface(quit,"exit the shell"));

    //        add("on_exit", new ICommandInteface(on_exit, "command executed upon exit"));

    //        add("version", new ICommandInteface(version,"displays version information"));
                
    //        add("gc", new ICommandInteface(gc,"performs garbage collection"));

    //        add("memory", new ICommandInteface(memory,"shows memory statistics"));

    //        add("delay", new ICommandInteface(delay,"pauses for a given number of seconds"));

    //        add("repeat", new ICommandInteface(repeat,"repeatedly execute a command"));

    //        add("load", new ICommandInteface(load,"load and execute commands from a file"));

    //        add("chain", new ICommandInteface(chain, "execute multiple commands on a single line"));

    //        add("time", new ICommandInteface(time, "report the time it takes to run a command"));
    //    }


    //    /** Dumps the commands in the interpreter */

    //    private void dumpCommands() 
    //    {
    //        foreach (var entry in  _commandList)
    //            putResponse(entry.Key + " - " + entry.Value.GetHelp);
    //    }


    //    /**
    //    /// Adds the given command to the command list.
    //     *
    //    /// @param name    the name of the command.
    //    /// @param command the command to be executed.
    //     */
    //    public void add(String name, ICommandInteface command) 
    //    {
    //        _commandList.Add(name, command);
    //    }


    //    /**
    //    /// Adds an alias to the command
    //     *
    //    /// @param command the name of the command.
    //    /// @param alias   the new aliase
    //     */
    //    public void addAlias(String command, string alias) 
    //    {
    //        _commandList.Add(alias, _commandList[command]);
    //    }


    //    /**
    //    /// Add the given set of commands to the list of commands.
    //     *
    //    /// @param newCommands the new commands to add to this interpreter.
    //     */
    //    public void add(Dictionary<String, ICommandInteface> newCommands) 
    //    {
    //        _commandList.Concat(newCommands);
    //    }


    //    /**
    //    /// Outputs a response to the sender.
    //     *
    //    /// @param response the response to send.
    //     */

    //    public void putResponse(String response) 
    //    {
    //        if (response != null && response.Length !=0) 
    //        {
    //            _out.WriteLine(response);
    //            _out.Flush();
    //            if (_trace) 
    //            {
    //                this.LogInfo("Out: " + response);
    //            }
    //        }
    //    }


    //    /** Called when the interpreter is exiting. Default behavior is to execute an "on_exit" command. */

    //    protected void onExit() 
    //    {
    //        execute("on_exit");
    //        this.LogInfo("----------\n");
    //    }


    //    /**
    //    /// Execute the given command.
    //     *
    //    /// @param args command args, args[0] contains name of cmd.
    //     */

    //    protected string execute(String[] args) 
    //    {
    //        var response = "";

    //        ICommandInteface ci;

    //        if (args.Length > 0) 
    //        {

    //            ci = _commandList[args[0]];
    //            if (ci != null) 
    //            {
    //                response = ci.delegateCommand(this, args);
    //            } 
    //            else 
    //            {
    //                response = "ERR  CMD_NOT_FOUND";
    //            }

    //            _totalCommands++;
    //        }
    //        return response;
    //    }


    //    /**
    //    /// Execute the given command string.
    //     *
    //    /// @param cmdString the command string.
    //     */
    //    public string execute(String cmdString) 
    //    {
    //        if (_trace) 
    //        {
    //            this.LogInfo("Execute: " + cmdString);
    //        }
    //        return execute(parseMessage(cmdString));
    //    }


    //    /**
    //    /// Parses the given message into an array of strings.
    //     *
    //    /// @param message the string to be parsed.
    //    /// @return the parsed message as an array of strings
    //     */
    //    protected String[] parseMessage(String message) 
    //    {
    //        int tokenType;
    //        var words = new List<String>(20);
    //        var st = new StreamTokenizer(message);

    //        st.ResetSyntax();
    //        st.WhitespaceChars(0, ' ');
    //        st.WordChars('!', 255);
    //        st.QuoteChar('"');
    //        st.QuoteChar('\"');
    //        st.CommentChar('#');

    //        while (true) 
    //        {
    //            try {
    //                tokenType = st.NextToken();
    //                if (tokenType == StreamTokenizer.TT_WORD) {
    //                    words.Add(st.StringValue);
    //                } else if (tokenType == '\'' || tokenType == '"') 
    //                {
    //                    words.Add(st.StringValue);
    //                } 
    //                else if (tokenType == StreamTokenizer.TT_NUMBER) 
    //                {
    //                    this.LogInfo("Unexpected numeric token!");
    //                } 
    //                else {
    //                    break;
    //                }
    //            } catch (IOException e) {
    //                break;
    //            }
    //        }
    //        return words.ToArray();
    //    }

    //    // inherited from thread.


    //    public void run() 
    //    {
    //        while (!_done) {
    //            try {
    //                printPrompt();
    //                var message = getInputLine();
    //                if (message == null) {
    //                    break;
    //                } else {
    //                    if (_trace) {
    //                        this.LogInfo("\n----------");
    //                        this.LogInfo("In : " + message);
    //                    }
    //                    message = message.Trim();
    //                    if (message.Length!=0) 
    //                    {
    //                        putResponse(execute(message));
    //                    }
    //                }
    //            }
    //            catch (IOException e) 
    //            {
    //                this.LogInfo("Exception: CommandInterpreter.run()");
    //                break;
    //            }
    //        }
    //        onExit();
    //    }

    //    // some history patterns used by getInputLine()

    //    private static string historyPush ="(.+):p";
    //    private static string editPattern =
    //            "\\^(.+?)\\^(.*?)\\^?";
    //    private static string bbPattern = "(!!)";


    //    /**
    //    /// Gets the input line. Deals with history. Currently we support simple csh-like history. !! - execute last command,
    //    /// !-3 execute 3 from last command, !2 execute second command in history list, !foo - find last command that started
    //    /// with foo and execute it. Also allows editing of the last command wich ^old^new^ type replacesments
    //     *
    //    /// @return the next history line or null if done
    //     */
    //    private string getInputLine()
    //    {
    //        var message = _in.ReadLine();
    //        if (message == null)
    //            return null;

    //        var justPush = false;
    //        var echo = false;
    //        var error = false;

    //        var m = Regex.Match(message,historyPush);
            
    //        if (m.Success) 
    //        {
    //            justPush = true;
    //            echo = true;
    //            message = m.Groups[1].Value;
    //        }

    //        //if (message.StartsWith("^")) 
    //        //{ 
    //        //    // line editing ^foo^fum^
    //        //    m = Regex.Match(message,editPattern);
    //        //    if (m.Success) 
    //        //    {
    //        //        string orig = m.Groups[0].Value;
    //        //        string sub = m.Groups[1].Value;
    //        //        try 
    //        //        {
    //        //            Pattern pat = Pattern.compile(orig);
    //        //            Matcher subMatcher = pat.matcher(history.getLast(0));
    //        //            if (subMatcher.find()) 
    //        //            {
    //        //                message = subMatcher.replaceFirst(sub);
    //        //                echo = true;
    //        //            } else {
    //        //                error = true;
    //        //                putResponse(message + ": substitution failed");
    //        //            }
    //        //        } catch (Exception pse) {
    //        //            error = true;
    //        //            putResponse("Bad regexp: " + pse.getDescription());
    //        //        }
    //        //    } else {
    //        //        error = true;
    //        //        putResponse("bad substitution sytax, use ^old^new^");
    //        //    }
    //        //} else if ((m = bbPattern.matcher(message)).find()) {
    //        //    message = m.replaceAll(history.getLast(0));
    //        //    echo = true;
    //        //} else if (message.startsWith("!")) {
    //        //    if (message.matches("!\\d+")) {
    //        //        int which = Integer.parseInt(message.substring(1));
    //        //        message = history.get(which);
    //        //    } else if (message.matches("!-\\d+")) {
    //        //        int which = Integer.parseInt(message.substring(2));
    //        //        message = history.getLast(which - 1);
    //        //    } else {
    //        //        message = history.findLast(message.substring(1));
    //        //    }
    //        //    echo = true;
    //        //}
    //        throw new NotImplementedException();
    //        if (error) {
    //            return "";
    //        }

    //        if (message.Length!=0) {
    //            _history.add(message);
    //        }

    //        if (echo) {
    //            putResponse(message);
    //        }
    //        return justPush ? "" : message;
    //    }


    //    public void close() {
    //        _done = true;
    //    }


    //    /** Prints the prompt. */

    //    private void printPrompt() 
    //    {
    //        if (_prompt != null) {
    //            _out.Write(_prompt);
    //            _out.Flush();
    //        }
    //    }


    //    public Boolean load(String filename) 
    //    {
    //        try {
    //            var fr = new StreamReader(filename);
    //            string inputLine;

    //            while ((inputLine = fr.ReadLine()) != null) 
    //            {
    //                var response = execute(inputLine);
    //                if (!response.Equals("OK")) 
    //                {
    //                    putResponse(response);
    //                }
    //            }
    //            fr.Close();
    //            return true;
    //        } catch (IOException ioe) {
    //            return false;
    //        }
    //    }


    //    /**
    //    /// Sets the prompt for the interpreter
    //     *
    //    /// @param prompt the prompt.
    //     */

    //    public void setPrompt(String prompt) 
    //    {
    //        this._prompt = prompt;
    //    }


    //    /**
    //    /// Gets the prompt for the interpreter
    //     *
    //    /// @return the prompt.
    //     */

    //    public string getPrompt() 
    //    {
    //        return _prompt;
    //    }


    //    /**
    //    /// Returns the output stream of this CommandInterpreter.
    //     *
    //    /// @return the output stream
    //     */
    //    public StreamWriter getPrintWriter() 
    //    {
    //        return _out;
    //    }


    //    /** manual tester for the command interpreter. */

    //    public static void main(String[] args) {
    //        var ci = new CommandInterpreter();

    //        try {
    //            Logger.LogInfo(typeof(CommandInterpreter), "Welcome to the Command interpreter test program");
    //            ci.setPrompt("CI> ");
    //            ci.run();
    //            Logger.LogInfo(typeof(CommandInterpreter), "Goodbye!");
    //        } 
    //        catch (Exception t) 
    //        {
    //            Logger.LogInfo(typeof(CommandInterpreter), t);
    //        }
    //    }


    //    }

    //public class CommandHistory
    //{

    //    private readonly CommandInterpreter _ci;
    //    public CommandHistory(CommandInterpreter ci)
    //    {
    //        _ci = ci;
    //    }

    //    private readonly List<String> history = new List<String>();


    //    /**
    //    /// Adds a command to the history
    //     *
    //    /// @param command the command to add
    //     */
    //    public void add(String command)
    //    {
    //        history.Add(command);
    //    }


    //    /**
    //    /// Gets the most recent element in the history
    //     *
    //    /// @param offset the offset from the most recent command
    //    /// @return the last command executed
    //     */
    //    public string getLast(int offset)
    //    {
    //        if (history.Count > offset)
    //        {
    //            return history[(history.Count - 1) - offset];
    //        }
    //        else
    //        {
    //            if(_ci!=null)
    //                _ci.putResponse("command not found");
    //            return "";
    //        }
    //    }


    //    /**
    //    /// Gets the most recent element in the history
    //     *
    //    /// @param which the offset from the most recent command
    //    /// @return the last command executed
    //     */
    //    public string get(int which)
    //    {
    //        if (history.Count > which)
    //        {
    //            return history[which];
    //        }
    //        else
    //        {
    //            if (_ci != null)
    //                _ci.putResponse("command not found");
    //            return "";
    //        }
    //    }


    //    /**
    //    /// Finds the most recent message that starts with the given string
    //     *
    //    /// @param match the string to match
    //    /// @return the last command executed that matches match
    //     */
    //    public string findLast(String match)
    //    {
    //        for (var i = history.Count - 1; i >= 0; i--)
    //        {
    //            var cmd = get(i);
    //            if (cmd.StartsWith(match))
    //            {
    //                return cmd;
    //            }
    //        }
    //        if (_ci != null)
    //            _ci.putResponse("command not found");
    //        return "";
    //    }


    //    /** Dumps the current history */
    //    public void dump()
    //    {
    //        for (var i = 0; i < history.Count; i++)
    //        {
    //            var cmd = get(i);
    //            if (_ci != null)
    //                _ci.putResponse(i + " " + cmd);
    //        }
    //    }
    //}
}
