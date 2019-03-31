using NUnit.Framework;
using Oika.Libs.Merror;
using FooCompany.BarTeam.MerrorTestsTarget;
using System;

namespace Oika.Libs.MerrorTests
{
    [TestFixture]
    public class ReflectorTest
    {

        const string NSName = "FooCompany.BarTeam.MerrorTestsTarget";

        [TearDown]
        public void TearDown()
        {
            FieldTestTarget.SetStaticText(null);
            PropertyTestTarget.SetStaticText(null);
        }

        [Test]
        public void ThrowWhenTypeParamIsNull()
        {
            try
            {
                var refObj = new Reflector(null);
                Assert.Fail();
            }
            catch (ArgumentNullException)
            {
            }
        }

        #region Test Constructors

        [Test]
        public void TestPrivateConstructor()
        {

            var refObj = new Reflector(NSName + ".NewInstanceTarget", typeof(IndexerTestTarget));
            var obj = refObj.NewInstance();

            Assert.IsTrue((bool)refObj.GetField(obj, "isDefaultConstructorCalled"));
        }

        [Test]
        public void TestExactConstructor()
        {
            var refObj = new Reflector(NSName + ".NewInstanceTarget", typeof(IndexerTestTarget));
            var obj = refObj.NewInstanceExact(ReflectorParam.New<string>(null));

            Assert.IsTrue((bool)refObj.GetField(obj, "isNullTextReceived"));
        }

        [Test]
        public void TestConstructorWithRefParam()
        {
            var refObj = new Reflector(NSName + ".NewInstanceTarget", typeof(IndexerTestTarget));

            var pm = ReflectorParam.New(0, true);
            var obj = refObj.NewInstanceExact(pm);

            Assert.AreEqual(123, pm.Value);
        }

        #endregion

        #region Test Fields

        [Test]
        public void TestStaticField()
        {
            var reflector = new Reflector(typeof(FieldTestTarget));
            FieldTestTarget.SetStaticText("hoge");

            Assert.AreEqual("hoge", reflector.GetStaticField("text"));

            reflector.SetStaticField("text", "あああ");

            Assert.AreEqual("あああ", reflector.GetStaticField("text"));
        }

        [Test]
        public void TestInstanceField()
        {
            var reflector = new Reflector(typeof(FieldTestTarget));
            var obj = new FieldTestTarget(3333);

            Assert.AreEqual(3333, reflector.GetField(obj, "num"));

            reflector.SetField(obj, "num", -1);

            Assert.AreEqual(-1, reflector.GetField(obj, "num"));
        }

        [Test]
        public void ThrowIfFieldTypeUnmatch()
        {
            var reflector = new Reflector(typeof(FieldTestTarget));
            var obj = new FieldTestTarget(123);

            try
            {
                reflector.SetField(obj, "num", "aaaaa");

                Assert.Fail();

            }
            catch (ArgumentException)
            {
            }
        }

        [Test]
        public void ThrowIfFieldNotFound()
        {
            var reflector = new Reflector(typeof(FieldTestTarget));
            var obj = new FieldTestTarget(123);

            try
            {
                reflector.SetField(obj, "none", null);

                Assert.Fail();

            }
            catch (MemberAccessException)
            {
            }
            try
            {
                var val = reflector.GetField(obj, "none");

                Assert.Fail();

            }
            catch (MemberAccessException)
            {
            }
        }

        #endregion

        #region Test Properties

        [Test]
        public void TestStaticProperty()
        {
            var reflector = new Reflector(typeof(PropertyTestTarget));
            PropertyTestTarget.SetStaticText("hoge");

            Assert.AreEqual("hoge", reflector.GetStaticProperty("Text"));

            reflector.SetStaticProperty("Text", "あああ");

            Assert.AreEqual("あああ", reflector.GetStaticProperty("Text"));
        }

        [Test]
        public void TestInstanceProperty()
        {
            var reflector = new Reflector(typeof(PropertyTestTarget));
            var obj = new PropertyTestTarget(3333);

            Assert.AreEqual(3333, reflector.GetProperty(obj, "Num"));

            reflector.SetProperty(obj, "Num", -1);

            Assert.AreEqual(-1, reflector.GetProperty(obj, "Num"));
        }

        [Test]
        public void ThrowIfPropertyTypeUnmatch()
        {
            var reflector = new Reflector(typeof(PropertyTestTarget));
            var obj = new PropertyTestTarget(123);

            try
            {
                reflector.SetProperty(obj, "Num", "aaaaa");

                Assert.Fail();

            }
            catch (ArgumentException)
            {
            }
        }

        [Test]
        public void TestIfPropertyNotFound()
        {
            var reflector = new Reflector(typeof(PropertyTestTarget));
            var obj = new PropertyTestTarget(123);

            try
            {
                reflector.SetProperty(obj, "none", null);

                Assert.Fail();

            }
            catch (MemberAccessException)
            {
            }
            try
            {
                var val = reflector.GetProperty(obj, "none");

                Assert.Fail();

            }
            catch (MemberAccessException)
            {
            }
        }

        #endregion

        #region Test Indexers

        [Test]
        public void TestIndexerWithOneParam()
        {
            var refObj = new Reflector(typeof(IndexerTestTarget));
            var obj = new IndexerTestTarget();
            var refItem = new Reflector(NSName + ".IndexerItem", typeof(IndexerTestTarget));

            //値を取得
            var item02 = refObj.GetIndexer(obj, 2);
            Assert.AreEqual(20, refItem.GetField(item02, "num"));

            //値を設定
            var item11 = refItem.NewInstance(1234);
            refObj.SetIndexer(obj, item11, 11);

            //設定した値を確認
            var res = refObj.GetIndexer(obj, 11);
            Assert.AreEqual(1234, refItem.GetField(res, "num"));
        }

        [Test]
        public void TestIndexerWithTwoParams()
        {
            var refObj = new Reflector(typeof(IndexerTestTarget));
            var obj = new IndexerTestTarget();
            var refItem = new Reflector(NSName + ".IndexerItem", typeof(IndexerTestTarget));

            //値を取得
            var item4three = refObj.GetIndexer(obj, 4, "three");
            Assert.AreEqual(3, refItem.GetField(item4three, "num"));

            //値を設定
            var item12hoge = refItem.NewInstance(int.MaxValue);
            refObj.SetIndexer(obj, item12hoge, 12, "hoge");

            //設定した値を確認
            var res = refObj.GetIndexer(obj, 12, "hoge");
            Assert.AreEqual(int.MaxValue, refItem.GetField(res, "num"));
        }

        #endregion

        #region Test Methods

        [Test]
        public void TestStaticMethod()
        {
            var reflector = new Reflector(typeof(MethodTestTarget));
            var res = reflector.InvokeStatic("StaticSum", 10, 20);

            Assert.AreEqual(30, res);
        }

        [Test]
        public void TestInstanceMethod()
        {
            var reflector = new Reflector(typeof(MethodTestTarget));
            var obj = new MethodTestTarget(4);

            var res = reflector.Invoke(obj, "Sum", 5);

            Assert.AreEqual(9, res);
        }

        [Test]
        public void TestMethodWithOutParam()
        {
            var reflector = new Reflector(typeof(MethodTestTarget));

            var obj = new MethodTestTarget(-3);

            var outParam = ReflectorParam.New(0, true);

            reflector.InvokeExact(obj, "Sum", ReflectorParam.New(4), outParam);

            Assert.AreEqual(1, outParam.Value);
        }

        #endregion
    }
}
