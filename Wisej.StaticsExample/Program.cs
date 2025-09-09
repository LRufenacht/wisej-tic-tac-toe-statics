using System;
using System.Collections.Specialized;
using Wisej.Web;

namespace Wisej.StaticsExample
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Arguments from the URL.</param>
        static void Main(NameValueCollection args)
        {
            new Page1().Show();
        }
    }
}