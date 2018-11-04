﻿using InjectionScript.Parsing;
using InjectionScript.Parsing.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectionScript.Interpretation
{
    public class Runtime
    {
        public Metadata Metadata { get; } = new Metadata();
        public Interpreter Interpreter { get; }
        public Globals Globals { get; }
        public string CurrentFileName { get; private set; }

        public Runtime()
        {
            Interpreter = new Interpreter(Metadata);
            Globals = new Globals();
            RegisterNatives();
        }

        private void RegisterNatives()
        {
            Metadata.Add(new NativeSubrutineDefinition("UO", "SetGlobal", (Action<string, string>)Globals.SetGlobal));
            Metadata.Add(new NativeSubrutineDefinition("UO", "GetGlobal", (Func<string, string>)Globals.GetGlobal));
        }

        public void Load(string fileName)
        {
            var parser = new Parser();
            var errorListener = new MemorySyntaxErrorListener();
            parser.AddErrorListener(errorListener);
            var file = parser.ParseFile(File.ReadAllText(fileName));
            CurrentFileName = fileName;

            if (errorListener.Errors.Any())
            {
                throw new SyntaxErrorException(fileName, errorListener.Errors);
            }

            var collector = new DefinitionCollector(Metadata);
            collector.Visit(file);
        }

        public void Load(injectionParser.FileContext file)
        {
            var collector = new DefinitionCollector(Metadata);
            collector.Visit(file);
        }

        public InjectionValue CallSubrutine(string name, params string[] arguments)
        {
            if (Metadata.TryGetSubrutine(name, arguments.Length, out var subrutine))
            {
                return Interpreter.CallSubrutine(subrutine.Subrutine, 
                    arguments.Select(x => new InjectionValue(x)).ToArray());
            }
            else
                throw new NotImplementedException();
        }
    }
}
