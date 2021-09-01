using System;

namespace geckoimagesBackend
{
    class Program
    {
        static void Main(string[] args)
        {
            Check check = new Check();
            check.setTimer().GetAwaiter().GetResult();

            while (true)
            {
                Console.Write("> ");

                string option = Console.ReadLine();

                switch (option)
                {
                    case "0":
                        check.checkDrive().GetAwaiter().GetResult();
                        break;
                    case "1":
                        return;
                }
            }
        }
    }
}
