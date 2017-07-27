using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using FrisyLUL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrisyLULTest.Test
{
    /// <summary>
    /// Test-class for testing the <see cref="Screenshot"/> methods.
    /// </summary>
    [TestClass]
    public class TestScreenshot
    {
        /// <summary>
        /// Name of a valid process, which is running in the background and is not minimized.
        /// </summary>
        private String _validProcess = "visual studio";

        /// <summary>
        /// Name of an invalid process, which is minimized.
        /// </summary>
        private String _invalidProcess = "notepad";

        /// <summary>
        /// Name of a not existing process.
        /// </summary>
        private String _notExistingProcess = "notExistingProcess";

        #region Screenshot.TakeScreenshot

        /// <summary>
        /// Test what happens when user tries to screenshot NULL.
        /// Using null as process name should throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestTakerWithNull()
        {
            Screenshot.TakeScreenshot(null);
        }

        /// <summary>
        /// Test what happens when user tries to screenshot a not existing process.
        /// An not existing process/ screenshot should be empty.
        /// </summary>
        [TestMethod]
        public void TestTakerWithNotExistingProc()
        {
            Assert.IsTrue(Screenshot.TakeScreenshot(_notExistingProcess).IsEmpty);
        }

        /// <summary>
        /// Test what happens when user tries to screenshot an invalid existing process.
        /// An invalid process/ screenshot should be empty.
        /// </summary>
        [TestMethod]
        public void TestTakerWithInvalidProc()
        {
            Assert.IsTrue(Screenshot.TakeScreenshot(_invalidProcess).IsEmpty);
        }

        /// <summary>
        /// Test what happens when user tries to screenshot a valid process.
        /// A valid/ screenshot should not be empty.
        /// </summary>
        [TestMethod]
        public void TestTakerWithValidProc()
        {
            Assert.IsFalse(Screenshot.TakeScreenshot(_validProcess).IsEmpty);
        }

        #endregion

        #region Screenshot.Dispose

        /// <summary>
        /// Test the dispose method with a valid process/ screenshot.
        /// The disposed object which is based on a valid process/ screenshot should
        /// be throwing an exception when accessing its attributes.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestDisposeWithValid()
        {
            var shot = Screenshot.TakeScreenshot(_validProcess);
            shot.Dispose();

            Assert.AreEqual(shot.Height, 0);
        }

        /// <summary>
        /// Test the dispose method with an invalid process/ screenshot.
        /// The disposed object which is based on an invalid process/ screenshot should
        /// be height/ width = 0 (not optimally)
        /// </summary>
        [TestMethod]
        public void TestDisposeWithInvalid()
        {
            var shot = Screenshot.TakeScreenshot(_invalidProcess);
            shot.Dispose();

            Assert.AreEqual(shot.Height + shot.Width, 0);
        }

        #endregion

        #region Screenshot.Extract

        /// <summary>
        /// Test Extract with zero-height or zero-width destination image
        /// </summary>
        [TestMethod]
        public void TestExtractWithZeroDestinations()
        {
            var shot = Screenshot.TakeScreenshot(_validProcess).Extract(0, 0, 100, 0);
            Assert.AreEqual(shot.Height + shot.Width, 0);
            shot.Dispose();

            shot = Screenshot.TakeScreenshot(_validProcess).Extract(0, 0, 0, 100);
            Assert.AreEqual(shot.Height + shot.Width, 0);
            shot.Dispose();
        }

        /// <summary>
        /// Test Extract on an empty screenshot.
        /// An unvalid process/ screenshot should be empty.
        /// </summary>
        [TestMethod]
        public void TestExtractWithEmpty()
        {
            Assert.IsTrue(Screenshot.TakeScreenshot(_notExistingProcess).Extract(0, 0, 2560, 1080).IsEmpty);
        }

        /// <summary>
        /// Test Extract on an valid screenshot.
        /// A valid process/ screenshot should not be empty.
        /// </summary>
        [TestMethod]
        public void TestExtractWithValid()
        {
            Assert.IsFalse(Screenshot.TakeScreenshot(_validProcess).Extract(0, 0, 2560, 1080).IsEmpty);
        }

        #endregion

        #region Screenshot.Save

        /// <summary>
        /// Test saving an empty screenshot.
        /// An empty screenshot should have a height/ width of 1 pixel.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestSaveWithEmpty()
        {
            Screenshot.TakeScreenshot(_notExistingProcess).Save(@"tmp.png");
            Image.FromFile(@"tmp.png"); //should throw an exception
        }

        #endregion

        #region Screenshot.ToBase64

        /// <summary>
        /// Test the ToBase method.
        /// Only test if there occures an exception, and nothing else.
        /// Cannot check if the returned string is correct, because their is no
        /// getter for the internal bitmanp object.
        /// </summary>
        [TestMethod]
        public void TestToBase64WithEmpty()
        {
            Assert.IsTrue(String.IsNullOrEmpty(
                Screenshot.TakeScreenshot(_invalidProcess).ToBase64(ImageFormat.Png))
            ); //see you in /dev/null
        }

        /// <summary>
        /// Test the ToBase64 method with null as ImageFormat
        /// </summary>
        [TestMethod]
        public void TestToBase64WithNullFormat()
        {
            var shot = Screenshot.TakeScreenshot(_validProcess);
            var base64 = shot.ToBase64(null);
            shot.Dispose();
            
            Assert.IsTrue(string.IsNullOrEmpty(base64));
        }

        /// <summary>
        /// Test the ToBase64 method with new created ImageFormat
        /// </summary>
        [TestMethod]
        public void TestToBase64WithCustomFormat()
        {
            var format = new ImageFormat(Guid.NewGuid());

            var shot = Screenshot.TakeScreenshot(_validProcess);
            var base64 = shot.ToBase64(format);
            shot.Dispose();
            
            Assert.IsTrue(string.IsNullOrEmpty(base64));
        }

        #endregion
    }
}
