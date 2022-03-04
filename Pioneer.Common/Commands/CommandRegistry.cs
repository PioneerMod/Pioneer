using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Pioneer.Common.Commands.Converters;

namespace Pioneer.Common.Commands;

internal class CommandRegistry : ICommandRegistry
{
    private readonly List<CommandTree> _commands;

    internal CommandRegistry()
    {
        _commands = new List<CommandTree>();
    }

    public void Scan(object obj)
    {
        ScanInternally(obj.GetType(), obj);
    }

    public void Scan<T>()
    {
        ScanInternally(typeof(T));
    }

    public IEnumerable<string> GetCommandNames()
    {
        return from command in _commands from chain in command.GetChains() select chain.GetName();
    }

    public IEnumerable<string> GetCommandUsages()
    {
        return from command in _commands from chain in command.GetChains() select chain.GetUsage();
    }

    public async Task<bool> ProcessAsync(string line)
    {
        var parts = line.Split(' ');
        if (parts.Length <= 0) return false;
        var command = FindCommandByParts(null, parts, 0, out int endIndex);
        if (command?.Handler != null)
        {
            var argStrings = new string[parts.Length - endIndex];
            for (int i = 0; i < argStrings.Length; i++)
                argStrings[i] = parts[i + endIndex];

            var parameters = command.Handler.Method.GetParameters();
            if (parameters.Count(info => !info.IsOptional) > argStrings.Length)
                return false;
            
            var args = ParameterConverter.Convert(parameters, argStrings);
            var rval = command.Handler.Invoke(args);
            if (rval is Task task)
            {
                await task;
                return true;
            }
        }
        
        return false;
    }

    private CommandTree? FindCommandByParts(CommandTree? current, string[] parts, int index, out int endIndex)
    {
        endIndex = index;
        if (current != null)
        {
            if (parts.Length <= index)
            {
                return current;
            }

            var subcommand = current.FindByName(parts[index]);
            return subcommand != null ? FindCommandByParts(subcommand, parts, index + 1, out endIndex) : current;
        }

        var command = FindByName(parts[index]);
        return command != null ? FindCommandByParts(command, parts, index + 1, out endIndex) : null;
    }

    private CommandTree? FindByName(string name)
    {
        return _commands.FirstOrDefault(command => command.Meta.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
    }

    private void ScanInternally(Type @class, object? owner = null)
    {
        var classCommand = @class.GetCustomAttribute<Command>();
        if (classCommand != null)
        {
            CommandTree rootCommand = new CommandTree(classCommand);
            _commands.Add(rootCommand);
            ScanInternallyForRoot(rootCommand, @class, owner);
        }
        else
        {
            foreach (var subclass in @class.GetNestedTypes())
            {
                ScanInternally(subclass, owner);
            }

            foreach (var method in @class.GetRuntimeMethods())
            {
                var command = method.GetCustomAttribute<Command>();
                if (command != null)
                {
                    if (owner == null && !method.IsStatic || owner != null && method.IsStatic) continue;
                    _commands.Add(new CommandTree(command, new MethodHandle(method, owner)));
                }
            }
        }

        foreach (var command in _commands)
        {
            command.GetChains(); // generating chains
        }
    }

    private void ScanInternallyForRoot(CommandTree root, Type @class, object? owner)
    {
        foreach (var subclass in @class.GetNestedTypes())
        {
            var classCommand = subclass.GetCustomAttribute<Command>();
            if (classCommand != null)
            {
                var subRoot = new CommandTree(classCommand);
                root.Subcommands.Add(subRoot);
                ScanInternallyForRoot(subRoot, subclass, owner);
            }
            else
            {
                ScanInternallyForRoot(root, @class, owner);
            }
        }

        foreach (var method in @class.GetRuntimeMethods())
        {
            var command = method.GetCustomAttribute<Command>();
            if (command != null)
            {
                if (owner == null && !method.IsStatic || owner != null && method.IsStatic) continue;
                if (command.Name == Command.Index)
                {
                    root.Handler = new MethodHandle(method, owner);
                }
                else
                {
                    root.Subcommands.Add(new CommandTree(command, new MethodHandle(method, owner)));
                }
            }
        }
    }
}