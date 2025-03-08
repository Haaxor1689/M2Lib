using M2Lib.m2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class M2Tests
    {
        [TestMethod]
        public void SaveLoadWotlk()
        {
            var origModel = TestUtils.LoadModel("GnomePounder.m2");
            TestUtils.SaveModel("GnomePounder-saved.m2", origModel);
            var savedModel = TestUtils.LoadModel("GnomePounder-saved.m2");
            savedModel.Version = M2Lib.m2.M2.Format.Cataclysm;

            TestUtils.AssertDeepEqual("SaveLoadWotlk", origModel, savedModel);
        }

        [TestMethod]
        public void SaveLoadVanilla()
        {
            var origModel = TestUtils.LoadModel("GnomePounder-vanilla.m2");
            TestUtils.SaveModel("GnomePounder-vanilla-saved.m2", origModel);
            var savedModel = TestUtils.LoadModel("GnomePounder-vanilla-saved.m2");

            TestUtils.AssertDeepEqual("SaveLoadVanilla", origModel, savedModel);
        }

        [TestMethod]
        public void WotlkToVanilla()
        {
            var origModel = TestUtils.LoadModel("GnomePounder.m2");
            origModel.Version = M2.Format.Classic;
            TestUtils.SaveModel("GnomePounder-converted.m2", origModel);
            var convertedModel = TestUtils.LoadModel("GnomePounder-converted.m2");
            var targetModel = TestUtils.LoadModel("GnomePounder-vanilla.m2");

            TestUtils.AssertDeepEqual("WotlkToVanilla", convertedModel, targetModel);
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestUtils.CleanupFiles(
                [
                    "GnomePounder-saved.m2",
                    "GnomePounder-saved00.skin",
                    "GnomePounder-saved01.skin",
                    "GnomePounder-saved02.skin",
                    "GnomePounder-vanilla-saved.m2",
                    "GnomePounder-converted.m2",
                ]
            );
        }
    }
}
