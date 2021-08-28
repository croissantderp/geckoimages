using System;

namespace geckoimagesBackend
{
    class Program
    {
        static void Main(string[] args)
        {
            Check check = new Check();
            check.setTimer().GetAwaiter().GetResult();
        }
    }
}
