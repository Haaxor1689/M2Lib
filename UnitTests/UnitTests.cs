using M2Lib.m2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class M2Tests
    {
        [TestInitialize]
        public void Init()
        {
            TestUtils.Init();
        }

        [TestMethod]
        public void SaveLoadWotlk()
        {
            var origModel = TestUtils.LoadModel("GnomePounder.m2");
            TestUtils.SaveModel("tmp/GnomePounder-saved.m2", origModel);
            TestUtils.SaveJson("GnomePounder-saved.json", origModel);
            var savedModel = TestUtils.LoadModel("tmp/GnomePounder-saved.m2");

            TestUtils.AssertDeepEqual("SaveLoadWotlk", origModel, savedModel);
        }

        [TestMethod]
        public void SaveLoadVanilla()
        {
            var origModel = TestUtils.LoadModel("GnomePounder-vanilla.m2");
            TestUtils.SaveModel("tmp/GnomePounder-vanilla-saved.m2", origModel);
            var savedModel = TestUtils.LoadModel("tmp/GnomePounder-vanilla-saved.m2");

            TestUtils.AssertDeepEqual("SaveLoadVanilla", origModel, savedModel);
        }

        [TestMethod]
        public void WotlkToVanilla()
        {
            var origModel = TestUtils.LoadModel("GnomePounder.m2");
            origModel.Version = M2.Format.Classic;
            TestUtils.SaveModel("tmp/GnomePounder-converted.m2", origModel);
            var convertedModel = TestUtils.LoadModel("tmp/GnomePounder-converted.m2");
            var targetModel = TestUtils.LoadModel("GnomePounder-vanilla.m2");

            TestUtils.AssertDeepEqual("WotlkToVanilla", convertedModel, targetModel);
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestUtils.Cleanup();
        }
    }
}
