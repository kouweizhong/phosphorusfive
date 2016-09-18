/*
 * Phosphorus Five, copyright 2014 - 2016, Thomas Hansen, phosphorusfive@gmail.com
 * Phosphorus Five is licensed under the terms of the MIT license, see the enclosed LICENSE file for details
 */

using p5.exp;
using p5.core;
using p5.exp.exceptions;
using p5.lambda.helpers;

namespace p5.lambda.keywords
{
    /// <summary>
    ///     Class wrapping the [while] keyword in p5 lambda
    /// </summary>
    public static class While
    {
        /// <summary>
        ///     The [while] keyword allows you to loop for as long as some condition is true
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "while", Protection = EventProtection.LambdaClosed)]
        public static void lambda_while (ApplicationContext context, ActiveEventArgs e)
        {
            // Storing old while "body" such that we can evaluate [while] immutably for each iteration
            Node oldWhile = e.Args.Clone ();

            // Storing old while value, since Evaluate changes it to either true or false
            var oldWhileValue = e.Args.Value;

            // Trying to prevent infinite loops
            int iterations = 0;
            bool uncheck = e.Args.GetExChildValue ("_unchecked", context, false);

            // Actual [while] loop
            var condition = new Conditions ();
            while (condition.Evaluate (context, e.Args)) {

                // Changing value back to what it was, to support things like "while:int:5" and so on
                e.Args.Value = oldWhileValue;

                // Executing current scope as long as while evaluates to true
                condition.ExecuteCurrentScope (context, e.Args);

                // Making sure each iteration is immutable
                e.Args.Clear ();
                e.Args.AddRange (oldWhile.Clone ().Children);

                // Checking if we got a [return] invocation during evaluation
                var rootChildName = e.Args.Root.FirstChild != null ? e.Args.Root.FirstChild.Name : null;
                if (rootChildName == "_break") {
                    e.Args.Root.FirstChild.UnTie ();
                    return;
                } else if (rootChildName == "_return") {
                    return;
                } else if (rootChildName == "_continue")
                    e.Args.Root.FirstChild.UnTie ();

                // Checking if we're overflowing maximum number of iterations, unless [_unchecked] was true
                if (!uncheck && iterations++ > 10000)
                    throw new LambdaException (
                        "Possible infinite loop encountered, more than 10.000 iterations of [while] loop", 
                        e.Args, 
                        context);
            }
        }
    }
}