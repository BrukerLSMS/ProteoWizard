using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pwiz.Skyline.FileUI;
using pwiz.Skyline.Model;
using pwiz.Skyline.SettingsUI;
using pwiz.SkylineTestUtil;

namespace pwiz.SkylineTestFunctional
{
    [TestClass]
    public class SkydFileModifiedTest : AbstractFunctionalTest
    {
        [TestMethod]
        public void TestSkydFileModified()
        {
            TestFilesZip = @"TestFunctional\SkydFileModifiedTest.zip";
            RunFunctionalTest();
        }

        protected override void DoTest()
        {
            RunUI(()=>
            {
                SkylineWindow.OpenFile(TestFilesDir.GetTestPath("SkydFileModifiedTest.sky"));
                SkylineWindow.SelectedPath = SkylineWindow.Document.GetPathTo((int)SrmDocument.Level.Molecules, 0);
                SkylineWindow.ShowCandidatePeaks();
            });
            var lastFile = TestFilesDir.GetTestPath("S_1.mzML");
            Assert.IsTrue(File.Exists(lastFile));
            for (int i = 2; i < 20; i++)
            {
                var nextFile = TestFilesDir.GetTestPath("S_" + i + ".mzML");
                File.Move(lastFile, nextFile);
                ImportResultsDlg importResultsDlg = ShowDialog<ImportResultsDlg>(SkylineWindow.ImportResults);
                RunDlg<OpenDataSourceDialog>(() => importResultsDlg.NamedPathSets = importResultsDlg.GetDataSourcePathsFile(null),
                    openDataSourceDialog =>
                    {
                        openDataSourceDialog.SelectFile(nextFile);
                        openDataSourceDialog.Open();
                    });
                WaitForConditionUI(() => importResultsDlg.NamedPathSets != null);
                OkDialog(importResultsDlg, importResultsDlg.OkDialog);
                RunDlg<TransitionSettingsUI>(SkylineWindow.ShowTransitionSettingsUI, transitionSettingsUi =>
                {
                    transitionSettingsUi.SelectedTab = TransitionSettingsUI.TABS.Instrument;
                    double methodMatchTolerance;
                    if (0 == (i & 1))
                    {
                        methodMatchTolerance = 0.055;
                    }
                    else
                    {
                        methodMatchTolerance = 0.0551;
                    }

                    transitionSettingsUi.MZMatchTolerance = methodMatchTolerance;
                    transitionSettingsUi.OkDialog();
                });
                WaitForDocumentLoaded();
                lastFile = nextFile;
            }
        }
    }

}
