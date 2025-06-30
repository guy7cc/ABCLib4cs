using System;
using ABCLib4cs;
using ABCLib4cs.IO;
using ABCLib4cs.Models;
using static System.Console;
using static System.Math;
using static System.String;

namespace SubmissionCodes
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Class1 obj1 = new Class1();
            object nestedObj = new Class1.NestedClass.InnerNestedClass();
            var val = Class1.NestedClass.NestedConstant;
            WriteLine(val);
            var scanner = new StreamScanner(Console.OpenStandardOutput());
        }
    }
}
