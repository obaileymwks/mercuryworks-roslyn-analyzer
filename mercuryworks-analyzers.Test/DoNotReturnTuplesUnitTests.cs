﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = mercuryworks_analyzers.Test.CSharpCodeFixVerifier<
    mercuryworks_analyzers.DoNotReturnTuplesAnalyzer,
    mercuryworks_analyzers.DoNotReturnTuplesCodeFixProvider>;

namespace mercuryworks_analyzers.Test
{
    [TestClass]
    public class DoNotReturnTuplesUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task empty_string_has_no_issues()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task custom_classes_are_handled_appropriately()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
        public Tuple<ABC,int> {|#0:Test|}()
        {
            return Tuple.Create(new ABC(),1);
        }
    }

    public class ABC
    {
        public ABC() 
        {
        }
    }
}";

            var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
        public TestDTO {|#0:Test|}()
        {
            return new TestDTO { Item1 = new ABC(), Item2 = 1 };
        }
    }

    public class TestDTO
    {
        public ABC Item1 { get; set; }
        public int Item2 { get; set; }
    }

    public class ABC
    {
        public ABC() 
        {
        }
    }
}";

            var expected = VerifyCS.Diagnostic("DoNotReturnTuples").WithLocation(0).WithArguments("Test");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task code_fix_creates_dto()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
        public Tuple<int,int> {|#0:Test|}()
        {
            return Tuple.Create(1,1);
        }
    }
}";

            var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
        public TestDTO Test()
        {
            return new TestDTO { Item1 = 1, Item2 = 1 };
        }
    }

    public class TestDTO
    {
        public int Item1 { get; set; }
        public int Item2 { get; set; }
    }
}";

            var expected = VerifyCS.Diagnostic("DoNotReturnTuples").WithLocation(0).WithArguments("Test");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task tuple_create_triggers_diagnostic()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public Tuple<int,int> {|#0:Test|}()
            {
                return Tuple.Create(1,1);
            }
        }
    }";

            var expected = VerifyCS.Diagnostic("DoNotReturnTuples").WithLocation(0).WithArguments("Test");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task named_tuples_trigger_diagnostics()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
        public (int a, int b) {|#0:Test|}()
        {
            return (1,3);
        }
    }
}";

            var expected = VerifyCS.Diagnostic("DoNotReturnTuples").WithLocation(0).WithArguments("Test");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task named_tuples_preserve_names()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
        public (int a, int b) {|#0:Test|}()
        {
            return (1,3);
        }
    }
}";
            var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
        public TestDTO Test()
        {
            return new TestDTO { a = 1, b = 3 };
        }
    }

    public class TestDTO
    {
        public int a { get; set; }
        public int b { get; set; }
    }
}";

            var expected = VerifyCS.Diagnostic("DoNotReturnTuples").WithLocation(0).WithArguments("Test");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }

        [TestMethod]
        public async Task named_tuples_with_classes_work()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
        public (int a, testClass b) {|#0:Test|}()
        {
            return (1,new testClass());
        }
    }

    public class testClass {}
}";
            var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
        public TestDTO Test()
        {
            return new TestDTO { a = 1, b = new testClass() };
        }
    }

    public class TestDTO
    {
        public int a { get; set; }
        public testClass b { get; set; }
    }

    public class testClass {}
}";

            var expected = VerifyCS.Diagnostic("DoNotReturnTuples").WithLocation(0).WithArguments("Test");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }

        [TestMethod]
        public async Task it_triggers_in_an_interface()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    interface test {
        (int a, int b) {|#0:Test|}();
    }
}";
            var expected = VerifyCS.Diagnostic("DoNotReturnTuples").WithLocation(0).WithArguments("Test");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task codefix_works_in_an_interface()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    interface test {
        (int a, testClass b) {|#0:Test|}();
    }

    public class testClass {}
}";

            var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    interface test {
        TestDTO {|#0:Test|}();
    }

    public class TestDTO
    {
        public int a { get; set; }
        public testClass b { get; set; }
    }

    public class testClass {}
}";

            var expected = VerifyCS.Diagnostic("DoNotReturnTuples").WithLocation(0).WithArguments("Test");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}

namespace test
{
    class typename
    {
        public (int a, int b) Test()
        {
            return (a: 1, b: 2);
        }
    }

    interface test
    {
        (int a, int b) Test();
    }
}