/*
 * Phosphorus Five, copyright 2014 - 2015, Thomas Hansen, phosphorusfive@gmail.com
 * Phosphorus Five is licensed under the terms of the MIT license, see the enclosed LICENSE file for details
 */

using System.Linq;
using p5.core;
using p5.exp;
using p5.exp.exceptions;

namespace p5.lambda.keywords
{
    /// <summary>
    ///     Class wrapping the [fetch] keyword in p5 lambda
    /// </summary>
    public static class Fetch
    {
        /// <summary>
        ///     The [fetch] keyword, allows you to forward retrieve value results of evaluation of its body
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "fetch", Protection = EventProtection.LambdaClosed)]
        private static void lambda_fetch (ApplicationContext context, ActiveEventArgs e)
        {
            // Basic syntax checking
            if (e.Args.Value != null)
                throw new LambdaException ("[fetch] does not take arguments as value, use [src] nodes for supplying arguments", e.Args, context);
            if (e.Args.Count == 0)
                throw new LambdaException ("[fetch] was not given any arguments at all, neither [src] nor lambda to evaluate", e.Args, context);
            if (e.Args.Children.Count (ix => ix.Name == "src") == 0)
                throw new LambdaException ("[fetch] was not given any [src] arguments", e.Args, context);
            if (e.Args.Children.Count (ix => ix.Name == "src") == e.Args.Count)
                throw new LambdaException ("[fetch] was not given a lambda block to evaluate", e.Args, context);

            // Figuring out offset to start evaluating from
            int offset = 0;
            Node curIdx = e.Args.FirstChild;

            // Figuring out [offset], or where to start executing lambda block
            while (curIdx.Name == "src") {

                // Incrementing [offset]
                offset += 1;

                // Moving to next sibling
                curIdx = curIdx.NextSibling;
            }

            // Making sure [eval] starts at right offset
            e.Args.Insert (0, new Node ("offset", offset + 1 /* Remember [offset] node itself! */));

            // Evaluating [fetch] lambda block, now with correct offset, 
            // making sure regardless of what happens, offset is removed after execution
            try
            {
                context.RaiseLambda ("eval-mutable", e.Args);
            }
            finally
            {
                // Removing [offset]
                e.Args [0].UnTie ();
            }

            // Now that [retrieve] lambda block is evaluated, we can start retrieving our source
            var source = XUtil.SourceSingle (context, e.Args);

            // Now we can remove entire lambda block, and [src] nodes, and replace with result from [src]
            e.Args.Clear ();
            e.Args.Value = source;
        }
    }
}