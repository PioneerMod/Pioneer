using System.Reflection;
using Microsoft.VisualBasic;

namespace Pioneer.Common.Commands;

/// <summary>
/// A wrapper class for chaining <see cref="Command"/>s and getting specific information.
/// </summary>
internal class CommandChain
{
    internal LinkedList<CommandTree> Commands { get; }

    internal CommandChain(CommandTree @base)
    {
        Commands = new LinkedList<CommandTree>();
        Commands.AddFirst(@base);
    }

    internal string GetUsage()
    {
        var name = GetName();
        var command = Commands.Last?.Value;
        if (command == null) return "NON FOUND";
        var paramsUsage = "";
        
        if (command.Handler != null)
        {
            var i = 0;
            var paramUsages = new string[command.Handler.Method.GetParameters().Length];
            var paramNames = command.Handler.Method.GetParameters().Collect(parameter =>
            {
                var meta = parameter.GetCustomAttribute<Param>();
                var paramName = (meta?.Name ?? parameter.Name) ?? "p" + i;
                paramUsages[i] = "\t" + paramName + " - " + (meta?.Description ?? "No description");
                i++;
                return $"<{paramName}{(parameter.IsOptional ? "?" : "")}>";
            }).Join(" ");
            if (!string.IsNullOrEmpty(paramNames)) name += " " + paramNames;
            paramsUsage = Strings.Join(paramUsages, "\n");
        }
        
        return name + " - " + command.Meta.Description + (string.IsNullOrEmpty(paramsUsage?.Trim()) ? "" : "\n" + paramsUsage);
    }

    internal string GetName()
    {
        var name = "";
        foreach (var command in Commands.Where(command => command.Meta.Name != Command.Index))
        {
            if (name == "")
                name = command.Meta.Name;
            else
                name += " " + command.Meta.Name;
        }

        return name;
    }
}