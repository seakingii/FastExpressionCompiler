﻿using System;
using System.Linq;
using BenchmarkDotNet.Attributes;

using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace FastExpressionCompiler.Benchmarks
{
    public class ClosureConstantsBenchmark
    {
        private static Expression<Func<A>> CreateExpression()
        {
            var q = Constant(new Q());
            var x = Constant(new X());
            var y = Constant(new Y());
            var z = Constant(new Z());

            var fe = Lambda<Func<A>>(
                New(typeof(A).GetTypeInfo().DeclaredConstructors.First(),
                    q, x, y, z, New(typeof(B).GetTypeInfo().DeclaredConstructors.First(),
                        q, x, y, z), New(typeof(C).GetTypeInfo().DeclaredConstructors.First(),
                        q, x, y, z)));

            return fe;
        }

        [MemoryDiagnoser]
        public class Compilation
        {
            /*
            |      Method |       Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
            |------------ |-----------:|----------:|----------:|------:|--------:|-------:|-------:|-------:|----------:|
            |     Compile | 406.719 us | 1.4416 us | 1.2780 us | 55.79 |    0.28 | 0.9766 | 0.4883 |      - |   6.26 KB |
            | CompileFast |   7.290 us | 0.0321 us | 0.0285 us |  1.00 |    0.00 | 0.4501 | 0.2213 | 0.0305 |   2.06 KB |

            ## Optimizing storing closure constants into variables

            |      Method |       Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
            |------------ |-----------:|----------:|----------:|------:|--------:|-------:|-------:|-------:|----------:|
            |     Compile | 407.657 us | 0.9090 us | 0.8058 us | 58.66 |    0.32 | 0.9766 | 0.4883 |      - |   6.26 KB |
            | CompileFast |   6.947 us | 0.0392 us | 0.0367 us |  1.00 |    0.00 | 0.4272 | 0.2136 | 0.0305 |   1.96 KB |

            ## Everything with closure

            |      Method |       Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
            |------------ |-----------:|----------:|----------:|------:|--------:|-------:|-------:|-------:|----------:|
            |     Compile | 410.013 us | 2.5495 us | 2.2600 us | 58.70 |    0.29 | 0.9766 | 0.4883 |      - |   6.26 KB |
            | CompileFast |   6.985 us | 0.0259 us | 0.0242 us |  1.00 |    0.00 | 0.4120 | 0.2060 | 0.0305 |   1.88 KB |

            |      Method |       Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
            |------------ |-----------:|----------:|----------:|------:|--------:|-------:|-------:|-------:|----------:|
            |     Compile | 414.094 us | 0.9048 us | 0.8021 us | 58.15 |    0.26 | 0.9766 | 0.4883 |      - |   6.26 KB |
            | CompileFast |   7.121 us | 0.0345 us | 0.0306 us |  1.00 |    0.00 | 0.3662 | 0.1831 | 0.0305 |   1.66 KB |

            */

            private readonly Expression<Func<A>> _expr = CreateExpression();

            [Benchmark]
            public object Compile() =>
                _expr.Compile();

            [Benchmark(Baseline = true)]
            public object CompileFast() =>
                _expr.CompileFast(true);
        }

        [MemoryDiagnoser]
        public class Invocation
        {
            /*
            |              Method |     Mean |     Error |    StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
            |-------------------- |---------:|----------:|----------:|------:|-------:|------:|------:|----------:|
            |     Invoke_Compiled | 33.46 ns | 0.1606 ns | 0.1341 ns |  1.07 | 0.0339 |     - |     - |     160 B |
            | Invoke_CompiledFast | 31.36 ns | 0.1900 ns | 0.1778 ns |  1.00 | 0.0339 |     - |     - |     160 B |

            ## Optimizing storing closure constants into variables

            |              Method |     Mean |     Error |    StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
            |-------------------- |---------:|----------:|----------:|------:|-------:|------:|------:|----------:|
            |     Invoke_Compiled | 34.06 ns | 0.1056 ns | 0.0937 ns |  1.01 | 0.0339 |     - |     - |     160 B |
            | Invoke_CompiledFast | 33.70 ns | 0.0580 ns | 0.0453 ns |  1.00 | 0.0339 |     - |     - |     160 B |
            */

            private readonly Func<A> _compiled     = CreateExpression().Compile();
            private readonly Func<A> _compiledFast = CreateExpression().CompileFast(true);

            [Benchmark]
            public object Invoke_Compiled() =>
                _compiled();

            [Benchmark(Baseline = true)]
            public object Invoke_CompiledFast() =>
                _compiledFast();
        }

        public class Q { }
        public class X { }
        public class Y { }
        public class Z { }

        public class A
        {
            public Q Q { get; }
            public X X { get; }
            public Y Y { get; }
            public Z Z { get; }
            public B B { get; }
            public C C { get; }

            public A(Q q, X x, Y y, Z z, B b, C c)
            {
                Q = q;
                X = x;
                Y = y;
                Z = z;
                B = b;
                C = c;
            }
        }

        public class B
        {
            public Q Q { get; }
            public X X { get; }
            public Y Y { get; }
            public Z Z { get; }

            public B(Q q, X x, Y y, Z z)
            {
                Q = q;
                X = x;
                Y = y;
                Z = z;
            }
        }

        public class C
        {
            public Q Q { get; }
            public X X { get; }
            public Y Y { get; }
            public Z Z { get; }

            public C(Q q, X x, Y y, Z z)
            {
                Q = q;
                X = x;
                Y = y;
                Z = z;
            }
        }
    }
}
