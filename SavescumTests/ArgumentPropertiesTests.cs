using NUnit.Framework;
using Savescum;
using System;

namespace SavescumTests
{
    class ArgumentPropertiesTests
    {
        // Setup not needed (yet) but leaving template for it here when it is
        //[SetUp]
        //public void Setup()
        //{
        //}

        [Test]
        public void TestArgumentPropertiesConstructHappy()
        {
            string[] args = { "a:b" };
            new ArgumentProperties(args, ":");

            // expected: no exception thrown
            Assert.Pass();
        }

        [Test]
        // [ExpectedException] requires more dependencies that don't seem worth it for the sugar
        public void TestArgumentPropertiesConstructNullArgs()
        {
            try
            {
                new ArgumentProperties(null, "d");
            }
            catch (NullReferenceException)
            {
                // expected: exception
                Assert.Pass();
            }

            Assert.Fail("Expected NullReferenceException to be thrown");
        }

        [Test]
        public void TestArgumentPropertiesConstructBadArgs()
        {
            string[] badargs = { "", "a", "a:b:c", "a::b", ":a", "a:", ":", "::", ":::" };
            int exceptionCountExpected = badargs.Length;
            int exceptionCountActual = 0;

            foreach (string badarg in badargs)
            {
                try
                {
                    string[] badargarray = { badarg };
                    new ArgumentProperties(badargarray, ":");
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);

                    // expected: exception
                    exceptionCountActual++;
                }
            }

            Assert.AreEqual(exceptionCountExpected, exceptionCountActual, "Expected exception counts to match");
        }

        [Test]
        public void TestReadStringHappy()
        {
            string[] args = { "a:b", "c:d", "e:f" };
            ArgumentProperties properties = new ArgumentProperties(args, ":");

            StringAssert.AreEqualIgnoringCase("b", properties.GetString("a", null));
        }

        [Test]
        public void TestReadStringMissingRequired()
        {
            string[] args = { "a:b" };
            ArgumentProperties properties = new ArgumentProperties(args, ":");

            try
            {
                string value = properties.GetString(
                    "notexisting",  // wasn't added and shouldn't be found
                    null);          // no default supplied means required
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }

            Assert.Fail("Expected ArgumentException");
        }

        [Test]
        public void TestReadStringDefaulted()
        {
            string[] args = { "a:b" };
            ArgumentProperties properties = new ArgumentProperties(args, ":");

            string value = properties.GetString(
                "notexisting",          // wasn't added and shouldn't be found
                "somedefaultvalue");    // default supplied should be returned

            StringAssert.AreEqualIgnoringCase("somedefaultvalue", value);
        }
    }
}
