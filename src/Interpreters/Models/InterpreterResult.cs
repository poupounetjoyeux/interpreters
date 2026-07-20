using System.Collections.Generic;

namespace KaraW3B.Interpreters.Models
{
    public sealed class InterpreterResult
    {
        private readonly List<InterpreterAlert> _fatals = new();
        private readonly List<InterpreterAlert> _errors = new();
        private readonly List<InterpreterAlert> _warnings = new();

        public IReadOnlyCollection<InterpreterAlert> Fatals => _fatals.AsReadOnly();

        public IReadOnlyCollection<InterpreterAlert> Errors => _errors.AsReadOnly();

        public IReadOnlyCollection<InterpreterAlert> Warnings => _warnings.AsReadOnly();

        internal void AddFatal(string message, int? fileLine = null)
        {
            _fatals.Add(new InterpreterAlert{Message = message, FileLine = fileLine});
        }

        internal void AddError(string message, int? fileLine = null)
        {
            _errors.Add(new InterpreterAlert{Message = message, FileLine = fileLine});
        }

        internal void AddWarning(string message, int? fileLine = null)
        {
            _warnings.Add(new InterpreterAlert{Message = message, FileLine = fileLine});
        }
    }
}
