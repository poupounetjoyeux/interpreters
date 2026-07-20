using KaraW3B.Interpreters.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KaraW3B.Interpreters.Tests.Mocks
{
    internal sealed class SongAlertMock
    {
        public AlertType Type { get; init; }

        public AlertLevel Level { get; init; }

        public string Message { get; init; }

        public int? Line { get; init; }
    }
}
