using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel_Advisor.Parsing
{
    /// <summary>
    /// Represents the possible parsing moves.
    /// </summary>
    enum ParsingMoves: int
    {
        Expand,
        Advance,
        MomentaryInsuccess,
        Back,
        AnotherTry,
        Success
    }

    /// <summary>
    /// Represents a descendent recursive parser.
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// Returns the production string used to obtain the given expression from the given grammar or
        /// null if the expression cannot be obtained from the grammar.
        /// </summary>
        /// <param name="expression">The expression to validate.</param>
        /// <param name="grammar">The given grammar.</param>
        /// <returns>The production string.</returns>
        public bool ValidateExpression(List<string> expression, Grammar grammar)
        {
            Configuration configuration = new Configuration();
            configuration.ToBeConstructedStack.Push(grammar.StartingSymbol);
            ParsingMoves currentMove = ParsingMoves.Expand;
            string symbol;
            string pop;
            string temp;
            bool exit = false;

            do
            {
            evaluate:
                // process the current move.
                switch (currentMove)
                {
                    case ParsingMoves.Expand:
                        pop = configuration.ToBeConstructedStack.Pop();
                        symbol = pop;
                        string productionApplied = symbol + "00";
                        configuration.ConstructedStack.Push(productionApplied);

                        for (int j = grammar.GetProductionsForSymbol(symbol)[0].RightSide.Count - 1; j >= 0; j--)
                        {
                            configuration.ToBeConstructedStack.Push(grammar.GetProductionsForSymbol(symbol)[0].RightSide[j]);
                        }
                        break;
                    case ParsingMoves.Advance:
                        pop = configuration.ToBeConstructedStack.Pop();
                        symbol = pop;
                        configuration.ConstructedStack.Push(symbol.ToString());
                        configuration.Index++;
                        break;
                    case ParsingMoves.MomentaryInsuccess:
                        configuration.State = States.Back;
                        if (grammar.Terminals.Contains(configuration.ConstructedStack.Peek()))
                        {
                            currentMove = ParsingMoves.Back;
                        }
                        else
                        {
                            currentMove = ParsingMoves.AnotherTry;
                        }
                        goto evaluate;
                    case ParsingMoves.Back:
                        if (configuration.ConstructedStack.Count == 0)
                        {
                            return false;
                        }
                        symbol = configuration.ConstructedStack.Pop();
                        configuration.ToBeConstructedStack.Push(symbol.ToString());
                        configuration.Index--;
                        temp = configuration.ConstructedStack.Peek();
                        int length = temp.Length;
                        if (length >= 3 && grammar.Nonterminals.Contains(temp.Substring(0, length - 2)))
                        {
                            currentMove = ParsingMoves.AnotherTry;
                            goto evaluate;
                        }
                        else
                        {
                            currentMove = ParsingMoves.Back;
                            goto evaluate;
                        }
                    case ParsingMoves.AnotherTry:
                        if (configuration.ConstructedStack.Count == 0)
                        {
                            return false;
                        }
                        temp = configuration.ConstructedStack.Pop();
                        string production = temp.Substring(0, temp.Length - 2);
                        int value = Int32.Parse(temp.Substring(temp.Length - 2));

                        int final = 0;
                        foreach (string str in grammar.GetProductionsForSymbol(production)[value].RightSide)
                        {
                            final += str.Length;
                        }
                        int total = 0;
                        while (total < final)
                        {
                            total += configuration.ToBeConstructedStack.Pop().Length;
                        }

                        if (value < grammar.GetProductionsForSymbol(production).Count - 1)
                        {
                            string newProduction = production;
                            if ((value + 1).ToString().Length == 1)
                            {
                                newProduction += "0" + (value + 1);
                            }
                            else
                            {
                                newProduction += (value + 1);
                            }
                            configuration.ConstructedStack.Push(newProduction);

                            for (int j = grammar.GetProductionsForSymbol(production)[value + 1].RightSide.Count - 1; j >= 0; j--)
                            {
                                configuration.ToBeConstructedStack.Push(grammar.GetProductionsForSymbol(production)[value + 1].RightSide[j]);
                            }
                            configuration.State = States.Normal;
                        }
                        else
                        {
                            configuration.ToBeConstructedStack.Push(production);

                            if (configuration.ConstructedStack.Count == 0)
                            {
                                return false;
                            }

                            string peek = configuration.ConstructedStack.Peek();
                            if (peek.Length >= 3 && grammar.Nonterminals.Contains(peek.Substring(0, peek.Length - 2)))
                            {
                                configuration.State = States.Normal;
                                currentMove = ParsingMoves.AnotherTry;
                                goto evaluate;
                            }
                            else
                            {
                                configuration.State = States.Back;
                                currentMove = ParsingMoves.Back;
                                goto evaluate;
                            }
                        }
                        break;
                    case ParsingMoves.Success:
                        configuration.State = States.Terminal;
                        exit = true;
                        break;
                }

                if (exit)
                {
                    break;
                }

                if (expression.Count == configuration.Index)
                {
                    if (configuration.ToBeConstructedStack.Count == 0)
                    {
                        currentMove = ParsingMoves.Success;
                        goto evaluate;
                    }
                }

                if (configuration.ToBeConstructedStack.Count == 0)
                {
                    currentMove = ParsingMoves.MomentaryInsuccess;
                    goto evaluate;
                }
                else
                {
                    // determine next move based on stack content.
                    string nextSymbol = configuration.ToBeConstructedStack.Peek();
                    if (grammar.Terminals.Contains(nextSymbol))
                    {
                        if (expression.Count <= configuration.Index || nextSymbol != expression[configuration.Index])
                        {
                            currentMove = ParsingMoves.MomentaryInsuccess;
                        }
                        else
                        {
                            currentMove = ParsingMoves.Advance;
                        }
                    }
                    else
                    {
                        currentMove = ParsingMoves.Expand;
                    }
                }
            }
            while (true);

            return true;
        }
    }
}
