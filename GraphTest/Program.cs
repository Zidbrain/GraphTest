﻿using System;

namespace GraphTest
{
    public static class Program
    {
        public static GraphTest GraphTest { get; private set; }

        public static void Main(string[] args)
        {
            GraphTest = new GraphTest();
            GraphTest.Run();
            Environment.Exit(0);
        }
    }
}