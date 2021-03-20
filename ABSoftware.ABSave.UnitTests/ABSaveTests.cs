﻿using ABSoftware.ABSave.Mapping;
using ABSoftware.ABSave.UnitTests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.ABSave.UnitTests
{
    [TestClass]
    public class ABSaveTests : TestBase
    {
        [TestMethod]
        public void Serialize_ByteArray()
        {
            var map = ABSaveMap.Get<string>(ABSaveSettings.GetSizeFocus(false));

            byte[] arr = ABSaveConvert.Serialize("A", map);
            Assert.AreNotEqual(0, arr.Length);
        }

        [TestMethod]
        public void Deserialize_ByteArray()
        {
            var map = ABSaveMap.Get<string>(ABSaveSettings.GetSizeFocus(false));
            byte[] arr = ABSaveConvert.Serialize("A", map);

            Assert.AreEqual("A", ABSaveConvert.Deserialize<string>(arr, map));
        }
    }
}