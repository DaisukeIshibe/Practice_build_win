using System;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            // 引数 --throw を渡すと例外を発生させて catch ブロックを確認できます
            if (args.Length > 0 && args[0] == "--throw")
            {
                throw new InvalidOperationException("forced error for testing");
            }

            Console.WriteLine("Hello World!");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            return 1;
        }
    }
}