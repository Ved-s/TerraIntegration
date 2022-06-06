﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;

namespace TerraIntegration.DataStructures
{
    public struct VariableMatch
    {
        public HashSet<string> Names { get; set; } = new();
        public HashSet<Type> Types { get; set; } = new();

        public static VariableMatch OfType<TVariable>() where TVariable : Variable
        {
            VariableMatch match = new();
            match.Types.Add(typeof(TVariable));
            return match;
        }
        public static VariableMatch OfType(Type type)
        {
            VariableMatch match = new();
            match.Types.Add(type);
            return match;
        }
        public static VariableMatch OfTypeName(string typeName)
        {
            VariableMatch match = new();
            match.Names.Add(typeName);
            return match;
        }

        public VariableMatch WithType<TVariable>() where TVariable : Variable
        {
            Types.Add(typeof(TVariable));
            return this;
        }
        public VariableMatch WithType(Type type)
        {
            Types.Add(type);
            return this;
        }
        public VariableMatch WithTypeName(string typeName)
        {
            Names.Add(typeName);
            return this;
        }

        public bool Match(Variable var)
        {
            return Names.Contains(var.TypeName) || Types.Contains(var.GetType());
        }
        public IEnumerable<Variable> Matches(IEnumerable<Variable> variables)
        {
            foreach (var variable in variables)
                if (Match(variable)) 
                    yield return variable;
        }

        public string ToTypeNameString()
        {
            string result = string.Join(", ",
                Types.Select(t => Variable.ByType.TryGetValue(t, out Variable var) ? var.TypeDisplayName : t.Name)
                .Concat(Names.Select(n => Variable.ByTypeName.TryGetValue(n, out Variable var) ? var.TypeDisplayName : n))
                );
            if (result.IsNullEmptyOrWhitespace()) return "None";
            return result;
        }
        public Type[] ToTypeArray() 
        {
            return Types.Concat(Names
                .Select(n => Variable.ByTypeName.TryGetValue(n, out Variable var) ? var.GetType() : null)
                .Where(t => t is not null)
                ).ToArray();
        }
    }
}