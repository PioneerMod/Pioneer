namespace Pioneer.Common.Commands;

/// <summary>
/// The command tree defines the main component of the internal command framework. A command tree is one command
/// which can be executed but which also can have child command trees.
/// </summary>
internal class CommandTree
{
    /// <summary>
    /// The meta of this command tree defining name and description of this command.
    /// </summary>
    internal Command Meta { get; }
    
    /// <summary>
    /// The handler of this command tree defined by a <see cref="MethodHandle"/>.
    /// </summary>
    internal MethodHandle? Handler { get; set; }
    
    /// <summary>
    /// A list containing other <see cref="CommandTree"/>s which represent the children of this command tree.
    /// </summary>
    internal List<CommandTree> Subcommands { get; }

    private List<CommandChain>? _chains;

    internal CommandTree(Command meta, MethodHandle? handler = null)
    {
        Meta = meta;
        Handler = handler;
        Subcommands = new List<CommandTree>();
    }

    internal CommandTree? FindByName(string name)
    {
        return Subcommands.FirstOrDefault(command => command.Meta.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
    }

    internal List<CommandChain> GetChains()
    {
        return _chains ??= CreateChains();
    }

    private List<CommandChain> CreateChains()
    {
        var chains = new List<CommandChain>();
        
        if (Handler != null)
        {
            chains.Add(new CommandChain(this));
        }
        
        foreach (var subChain in Subcommands.Select(subcommand => subcommand.CreateChains()).SelectMany(subChains => subChains))
        {
            subChain.Commands.AddFirst(this);
            chains.Add(subChain);
        }

        return chains;
    }
}