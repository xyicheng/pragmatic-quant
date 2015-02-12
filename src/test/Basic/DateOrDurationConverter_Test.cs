﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pragmatic_quant_model.Basic.Structure;

namespace test.Basic
{
    [TestClass]
    public class DateOrDurationConverter_Test
    {
        [TestMethod]
        public void TestConvertDate()
        {
            var refDate = new DateTime(2015, 01, 27);
            Assert.AreEqual(DateAndDurationConverter.ConvertDate(refDate), refDate);
            Assert.AreEqual(DateAndDurationConverter.ConvertDate(new DateOrDuration(refDate)), refDate);
            Assert.AreEqual(DateAndDurationConverter.ConvertDate("27 Jan 2015"), refDate);
            Assert.AreEqual(DateAndDurationConverter.ConvertDate("01/27/2015"), refDate);
        }
        [TestMethod]
        public void TestConvertDuration()
        {
            var refDuration = 10 * Duration.Year;
            Assert.AreEqual(DateAndDurationConverter.ConvertDuration(10 * Duration.Year), refDuration);
            Assert.AreEqual(DateAndDurationConverter.ConvertDuration(new DateOrDuration(refDuration)), refDuration);
            Assert.AreEqual(DateAndDurationConverter.ConvertDuration("10y"), refDuration);
            Assert.AreEqual(DateAndDurationConverter.ConvertDuration("10Y"), refDuration);
        }
    }
}