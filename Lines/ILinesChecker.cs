using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lines
{
    interface ILinesChecker
    {
        void Cancel();
        
        Boolean Contains(String line);

        void ContainsAsync(String line, Action<Boolean> onSuccess, Action<String> onFailure);
        
        void Reset();
    }
}
