using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Clifton.Kademlia;
using Clifton.Kademlia.Common;

namespace UnitTests2
{
    [TestClass]
    public class IDTests
    {
        [TestMethod]
        public void LittleEndianTest()
        {
            byte[] test = new byte[20];
            test[0] = 1;
            Assert.IsTrue(new ID(test) == new BigInteger(1), "Expected value to be 1.");
        }

        [TestMethod]
        public void PositiveValueTest()
        {
            byte[] test = new byte[20];
            test[19] = 0x80;
            Assert.IsTrue(new ID(test) == BigInteger.Pow(new BigInteger(2), 159), "Expected value to be 1.");
        }

        [TestMethod, ExpectedException(typeof(IDLengthException))]
        public void BadIDTest()
        {
            byte[] test = new byte[21];
            new ID(test);
        }

        [TestMethod]
        public void RandomWithinBucketTests()
        {
            // Must be powers of 2.
            List<(int low, int high)> testCases = new List<(int low, int high)>()
            {
                (0, 256),                   // 7 bits should be set
                (256, 1024),                // 2 bits (256 + 512) should be set
                (65536, 65536 * 2),          // no additional bits should be set.
                (65536, 65536 * 4),          // 2 bits (65536 and 65536*2) should be set.
                (65536, 65536 * 16),          // 4 bits (65536, 65536*2, 65536*4, 65536*8) should be set.
            };

            foreach (var testCase in testCases)
            {
                KBucket bucket = new KBucket(testCase.low, testCase.high);
                // We force all bits in the range we are "randomizing" to be true
                // so it's not really randomized.  This verifies the outer algorithm
                // that figures out which bits to randomize.
                ID id = ID.RandomIDWithinBucket(bucket, true);  

                Assert.IsTrue(id >= bucket.Low && id < bucket.High, "ID is outside of bucket range.");

                // The ID, because we're forcing bits, should always be (high - 1) & ~max(0, low - 1)
                int bitCheck = (testCase.high - 1) & ~Math.Max(0, testCase.low - 1);

                Assert.IsTrue(id.Value == bitCheck, "Expected bits are not correct.");
            }
        }
    }
}


