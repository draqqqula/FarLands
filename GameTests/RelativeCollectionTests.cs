using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGame;

namespace GameTests
{
    [TestFixture]
    internal class RelativeCollectionTests
    {

        [Test]
        public void PlaceAbove_InsertsItemAboveSpecifiedItem()
        {
            var list = new RelativeCollection<string>();
            list.PlaceTop("A");
            list.PlaceTop("B");
            list.PlaceAbove("C", "B");

            Assert.AreEqual(3, list.Length);
            Assert.AreEqual("C", list.ElementAt(2));
        }

        [Test]
        public void PlaceBelow_InsertsItemBelowSpecifiedItem()
        {
            var list = new RelativeCollection<string>();
            list.PlaceTop("A");
            list.PlaceTop("B");
            list.PlaceBelow("C", "B");

            Assert.AreEqual(3, list.Length);
            Assert.AreEqual("C", list.ElementAt(1));
        }

        [Test]
        public void PlaceAbove_InsertsItemAboveNonexistentItem()
        {
            var list = new RelativeCollection<string>();
            list.PlaceTop("A");
            list.PlaceTop("B");
            list.PlaceAbove("C", "D");

            Assert.AreEqual(2, list.Length);
            Assert.AreEqual("B", list.First());
        }

        [Test]
        public void PlaceBelow_InsertsItemBelowNonexistentItem()
        {
            var list = new RelativeCollection<string>();
            list.PlaceTop("A");
            list.PlaceTop("B");
            list.PlaceBelow("C", "D");

            Assert.AreEqual(2, list.Length);
            Assert.AreEqual("B", list.First());
        }
    }
}
