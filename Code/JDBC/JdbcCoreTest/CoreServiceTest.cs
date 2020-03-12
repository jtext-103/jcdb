using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JdbcCoreTest
{
    /// <summary>
    /// 测试CoreService的增删查改
    /// </summary>
    [TestClass]
    public class CoreServiceUnitTest
    {
        private static CoreService myCoreService;

        private static void clearDb()
        {
            myCoreService.ClearDb();
        }

        [ClassInitialize]
        public static void BasicSetup(TestContext context)
        {
           //第一步
            myCoreService = CoreService.GetInstance();
            myCoreService.Init("mongodb://127.0.0.1:27017", "JDBC-test", "Experiment");
        }
        [TestInitialize]
        public void Setup()
        {
            //第二步
            Debug.Write(2);
        }
        [TestCleanup]
        public void MyTestCleanup()
        {
            //最后一步
            clearDb();
        }

        /// <summary>
        /// 增加节点：添加新节点
        /// </summary>
        [TestMethod]
        public void AddNodeTest()
        {
            //arrange
            var exp1 = new Experiment("exp1");
            var exp2 = new Experiment("exp2");
            var exp2c = new Experiment("exp2");
            var exp11 = new Experiment("exp1-1");
            var exp12 = new Experiment("exp1-2");
            var sig121 = new Signal("sig1-2-1");
            var sig121c = new Signal("sig1-2-1");
            
            //act
            //add 2 root
            //    /exp1 
            //    /exp2
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp2).Wait();
            
            //1.1 add entity with same name to root.
            string errorSameNameOnRoot="";
            try
            {
                myCoreService.AddJdbcEntityToAsync(Guid.Empty,exp2c).Wait();
            }
            catch (Exception e)
            {
                //this is async call so exception is wrapped
                errorSameNameOnRoot = e.InnerException.Message;
            }
            Assert.AreEqual(ErrorMessages.NameDuplicateError, errorSameNameOnRoot);

            //1.2 add signal to root, expecting error, signal's parent can only be experiment.
            string errorSigOnRoot="";
            try
            {
                myCoreService.AddJdbcEntityToAsync(Guid.Empty, sig121).Wait();
            }
            catch (Exception e)
            {
                //this is async call so exception is wrapped
                errorSigOnRoot = e.InnerException.Message;
            }
            Assert.AreEqual(ErrorMessages.NotValidParentError, errorSigOnRoot);

            //add child
            //    /exp1/exp1-1/
            //    /exp1/exp1-2/sig1-2-1
            myCoreService.AddJdbcEntityToAsync(exp1.Id, exp11).Wait();     
            myCoreService.AddJdbcEntityToAsync(exp1.Id, exp12).Wait();
            myCoreService.AddJdbcEntityToAsync(exp12.Id, sig121).Wait();
            
            //1.3 add signal to another signal, expecting error, signal's parent can only be experiment.
            string errorAddToSignal="";
            try
            {
                myCoreService.AddJdbcEntityToAsync(sig121.Id, sig121c).Wait();
            }
            catch (Exception e)
            {
                //this is async call so exception is wrapped
                errorAddToSignal = e.InnerException.Message;
            }
            Assert.AreEqual(ErrorMessages.NotValidParentError, errorAddToSignal);

            //1.4 add the sig with the same name to the parent errexpected
            string errorSameName = "";
            try
            {
                myCoreService.AddJdbcEntityToAsync(exp12.Id, sig121c).Wait();
            }
            catch (Exception e)
            {

                errorSameName = e.InnerException.Message;
            }
            Assert.AreEqual(ErrorMessages.NameDuplicateError, errorSameName);

            //1.5 the parent is  non exist, error expected
            string errorParentNonExistWhenAdd = "";
            try
            {
                myCoreService.AddJdbcEntityToAsync(Guid.NewGuid(), sig121c).Wait();
            }
            catch (Exception e)
            {
                //this is async call so exception is wrapped
                errorParentNonExistWhenAdd = e.InnerException.Message;
            }
            Assert.AreEqual(ErrorMessages.ParentNotExistError, errorParentNonExistWhenAdd);

            //1.6 the parent is  non exist, error expected
            string errorParentNonExist2 = "";
            try
            {
                var tNothing = myCoreService.GetAllChildrenIdAsync(Guid.NewGuid()).Result;
            }
            catch (Exception e)
            {
                //this is async call so exception is wrapped
                errorParentNonExist2 = e.InnerException.Message;
            }
            Assert.AreEqual(ErrorMessages.ParentNotExistError, errorParentNonExist2);

            //1.7 assert id and entity.
            //    /exp1/exp1-1/
            //    /exp1/exp1-2/sig1-2-1
            var roots = myCoreService.GetAllChildrenAsync().Result;
            CollectionAssert.AreEqual(new List<Guid> { exp1.Id, exp2.Id }, roots.Select(r => r.Id).ToList());

            var rootsById = myCoreService.GetAllChildrenAsync(Guid.Empty).Result;
            CollectionAssert.AreEqual(new List<Guid> { exp1.Id, exp2.Id }, rootsById.Select(r => r.Id).ToList());

            var rootIdsById = myCoreService.GetAllChildrenIdAsync(Guid.Empty).Result;
            CollectionAssert.AreEqual(new List<Guid> { exp1.Id, exp2.Id }, rootIdsById.ToList());

            var secLevId = myCoreService.GetAllChildrenIdAsync(exp1).Result;
            CollectionAssert.AreEquivalent(new List<Guid> { exp11.Id, exp12.Id }, secLevId.ToList());

            var secLevIdById = myCoreService.GetAllChildrenIdAsync(exp1.Id).Result;
            CollectionAssert.AreEquivalent(new List<Guid> { exp11.Id, exp12.Id }, secLevIdById.ToList());

            var trdLevIdById = myCoreService.GetAllChildrenIdAsync(exp12.Id).Result;
            CollectionAssert.AreEquivalent(new List<Guid> { sig121.Id }, trdLevIdById.ToList());

            var shouldBeEmpty = myCoreService.GetAllChildrenAsync(exp11).Result;
            Assert.AreEqual(shouldBeEmpty.Count(), 0);

            var sig121ParentId = myCoreService.GetParentIdAsync(sig121.Id).Result;
            Assert.AreEqual(exp12.Id, sig121ParentId);

            var errorParentNullId = myCoreService.GetParentIdAsync(Guid.NewGuid()).Result;   
            Assert.AreEqual(Guid.Empty, errorParentNullId); 
    
            Assert.AreEqual(exp11.EntityType,JDBCEntityType.Experiment);
            Assert.AreEqual(sig121.EntityType, JDBCEntityType.Signal);
        }

        /// <summary>
        /// 通过名字查找节点
        /// </summary>
        [TestMethod]
        public void QueryByNameTest()
        {
            //arrange
            var exp1 = new Experiment("exp1");
            var exp2 = new Experiment("exp2");
            var exp2c = new Experiment("exp2");
            var exp11 = new Experiment("exp1-1");
            var exp12 = new Experiment("exp1-2");
            var sig121 = new Signal("sig1-2-1");
            var sig121c = new Signal("sig1-2-1");

            //add 2 root
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp2).Wait();

            //add child
            myCoreService.AddJdbcEntityToAsync(exp1.Id, exp11).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, exp12).Wait();
            myCoreService.AddJdbcEntityToAsync(exp12.Id, sig121).Wait();

            /*
            /exp1/exp11/
                 /exp12/sig121          
            */

            //act
            //1. query by name
            var root1Id = myCoreService.GetChildIdByNameAsync(Guid.Empty,exp1.Name).Result;
            Assert.AreEqual(exp1.Id, root1Id);

            var secLev11Id = myCoreService.GetChildIdByNameAsync(exp1.Id, exp11.Name).Result;
            Assert.AreEqual(exp11.Id, secLev11Id);

            var secLev12Id = myCoreService.GetChildIdByNameAsync(exp1.Id, exp12.Name).Result;
            Assert.AreEqual(exp12.Id, secLev12Id);

            var nothing = myCoreService.GetChildIdByNameAsync(exp1.Id, "ass").Result;
            Assert.AreEqual(Guid.Empty, nothing);

            var trdLevId = myCoreService.GetChildIdByNameAsync(exp12.Id,sig121c.Name).Result;
            Assert.AreEqual(sig121.Id, trdLevId);

            //2 the parent is  non exist, error expected
            string errorParentNonExist = "";
            try
            {
                var tNothing = myCoreService.GetChildIdByNameAsync(Guid.NewGuid(), sig121c.Name).Result;
            }
            catch (Exception e)
            {
                //this is async call so exception is wrapped
                errorParentNonExist = e.InnerException.Message;
            }
            Assert.AreEqual(ErrorMessages.ParentNotExistError, errorParentNonExist);
        }



        /// <summary>
        /// 更改路径即向数据库添加已有节点
        /// </summary>
        [TestMethod]
        public void ChangePathTest()
        {
            //arrange
            var exp1 = new Experiment("exp1");
            var exp2 = new Experiment("exp2");
            var exp11 = new Experiment("exp1-1");
            var exp111 = new Experiment("exp1-1-1");
            var exp12 = new Experiment("exp1-2");
            var sig121 = new Signal("sig1-2-1");
            var sig111 = new Signal("sig1-1-1");
            //add 2 root
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp2).Wait();

            //add child
            myCoreService.AddJdbcEntityToAsync(exp1.Id, exp11).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, exp12).Wait();
            myCoreService.AddJdbcEntityToAsync(exp12.Id, sig121).Wait();
            myCoreService.AddJdbcEntityToAsync(exp11.Id, sig111).Wait();
            myCoreService.AddJdbcEntityToAsync(exp11.Id, exp111).Wait();

            //old tree
            /*
             /exp1/exp11/sig111
                        /exp111
                  /exp12/sig121  
             /exp2           
             */

            //act
            //move exp 12 to root
            var tExp12 = myCoreService.GetOneByIdAsync(exp12.Id).Result;
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, tExp12).Wait();
           
            //1 move exp1 under its child exp111 will cause expetion!!!
            var tExp1 = myCoreService.GetOneByIdAsync(exp1.Id).Result;
            var errorNotToChild = "";
            try
            {
                myCoreService.AddJdbcEntityToAsync(exp111.Id, tExp1).Wait();
            }
            catch (Exception e)
            {
                errorNotToChild = e.InnerException.Message;
            }
            Assert.AreEqual(ErrorMessages.ParentLoopError, errorNotToChild);

            //2 move to another entity,正确更改节点路径检验.
            var tExp11 = myCoreService.GetOneByIdAsync(exp11.Id).Result;
            myCoreService.AddJdbcEntityToAsync(exp2.Id, tExp11).Wait();
            //new tree
            /*
             /exp1
             /exp12/sig121  
             /exp2/exp11/sig111
                        /exp111
             */

            // try get root still work
            var roots = myCoreService.GetAllChildrenAsync().Result;
            CollectionAssert.AreEquivalent(new List<Guid> { exp1.Id, exp12.Id, exp2.Id }, roots.Select(r => r.Id).ToList());

            // try get child still work
            var nSig121 = myCoreService.GetAllChildrenAsync(exp12.Id).Result;
            Assert.AreEqual(sig121.Id, nSig121.SingleOrDefault().Id);

            // try get child still work
            var nExp11 = myCoreService.GetAllChildrenAsync(exp2.Id).Result;
            Assert.AreEqual(exp11.Id, nExp11.SingleOrDefault().Id);

            // try get parent still work
            var nExp2 = myCoreService.GetParentAsync(exp11.Id).Result;
            Assert.AreEqual(exp2.Id, nExp2.Id);

            // just to check path
            var nSig111 = myCoreService.GetOneByIdAsync(sig111.Id).Result;
            var nExp111 = myCoreService.GetOneByIdAsync(exp111.Id).Result;
            Assert.AreEqual(@"/exp1-2", roots.SingleOrDefault(r => r.Name == exp12.Name).Path);
            Assert.AreEqual(@"/exp1-2/sig1-2-1", nSig121.SingleOrDefault().Path);
            Assert.AreEqual(@"/exp2/exp1-1", nExp11.SingleOrDefault().Path);
            Assert.AreEqual(@"/exp2/exp1-1/sig1-1-1", nSig111.Path);
            Assert.AreEqual(@"/exp2/exp1-1/exp1-1-1", nExp111.Path);      
        }

        /// <summary>
        /// 删除单个节点和级联删除节点
        /// </summary>
        [TestMethod]
        public void DeleteTest()
        {
            var exp1 = new Experiment("exp1");
            var exp2 = new Experiment("exp2");
            var exp11 = new Experiment("exp1-1");
            var exp111 = new Experiment("exp1-1-1");
            var exp12 = new Experiment("exp1-2");
            var sig121 = new Signal("sig1-2-1");
            var sig111 = new Signal("sig1-1-1");
            var sigerror = new Signal("sigerror");
            //add 2 root
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp2).Wait();

            //add child
            myCoreService.AddJdbcEntityToAsync(exp1.Id, exp11).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, exp12).Wait();
            myCoreService.AddJdbcEntityToAsync(exp12.Id, sig121).Wait();
            myCoreService.AddJdbcEntityToAsync(exp11.Id, sig111).Wait();
            myCoreService.AddJdbcEntityToAsync(exp11.Id, exp111).Wait();
            //old tree
            /*
             /exp1/exp11/sig111
                        /exp111
                  /exp12/sig121  
             /exp2
             */

            //act
            //try delet exp12 error expetced
            //1 cannot delete the parent nod if not set recuresive.
            var errorDelete = "";
            try
            {
                myCoreService.DeleteAsync(exp12.Id).Wait();
            }
            catch (Exception e)
            {
                errorDelete = e.InnerException.Message;
            }
            Assert.AreEqual(ErrorMessages.DeleteEntityWithChildError, errorDelete);

            //2 the deleted entity is  non exist, error expected.
            myCoreService.DeleteAsync(exp12.Id,true).Wait();
            myCoreService.DeleteAsync(exp2.Id).Wait();
            string errorDeleteNonExist = "";
            try
            {
                myCoreService.DeleteAsync(exp2.Id).Wait();
            }
            catch (Exception e)
            {
                //this is async call so exception is wrapped
                errorDeleteNonExist = e.InnerException.Message;
            }
            Assert.AreEqual(ErrorMessages.EntityNotFoundError, errorDeleteNonExist);

            //3 get for assert
            var root = myCoreService.GetAllChildrenAsync().Result;
            CollectionAssert.AreEquivalent(new List<Guid> { exp1.Id }, root.Select(r => r.Id).ToList());

            var nSig121 = myCoreService.GetOneByIdAsync(sig121.Id).Result;
            Assert.IsNull(nSig121);

            var nExp12 = myCoreService.GetChildIdByNameAsync(exp1.Id, exp12.Name).Result;
            Assert.AreEqual(Guid.Empty, nExp12);
        }

        /// <summary>
        /// 拷贝节点测试：递归和不递归
        /// </summary>
        [TestMethod]
        public void DuplicateTest()
        {
            //arrange
            //arrange
            var exp1 = new Experiment("exp1");
            var exp2 = new Experiment("exp2");
            var exp11 = new Experiment("exp1-1");
            var exp111 = new TestExp("exp1-1-1");
            var exp12 = new Experiment("exp1-2");
            var sig121 = new Signal("sig1-2-1");
            var sig111 = new TestSignal("sig1-1-1");

            exp111.TestString = "test";
            exp111.TestList = new List<string> { "1", "1", "1" };
            sig111.TestString = "test";
            sig111.TestList = new List<string> { "1", "1", "1" };

            //add 2 root
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp2).Wait();

            //add child
            myCoreService.AddJdbcEntityToAsync(exp1.Id, exp11).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, exp12).Wait();
            myCoreService.AddJdbcEntityToAsync(exp12.Id, sig121).Wait();
            myCoreService.AddJdbcEntityToAsync(exp11.Id, sig111).Wait();
            myCoreService.AddJdbcEntityToAsync(exp11.Id, exp111).Wait();

            //old tree                                       new tree
            /*               
             /exp1/exp11/sig111                        /exp1/exp11/sig111
                        /exp111                                   /exp111
                  /exp12/sig121                             /exp11c/sig111
             /exp2                                                 /exp111
                                                            /exp12/sig121
                                                       /exp2 /exp12  
             */

           // 1 父节点下有同名节点
            var errorSameName = "";
            try
            {
                myCoreService.DuplicateAsync(exp11.Id, exp1.Id, exp11.Name).Wait();
            }
            catch (Exception e)
            {
                errorSameName = e.InnerException.Message;
            }
            Assert.AreEqual(ErrorMessages.NameDuplicateError, errorSameName);

            myCoreService.DuplicateAsync(exp11.Id, exp1.Id, "exp1-1-c",true).Wait();
            myCoreService.DuplicateAsync(exp12.Id, exp2.Id, exp12.Name).Wait();

            //get for assert
            var childrenNameOfExp1 = myCoreService.GetAllChildrenAsync(exp1.Id).Result.Select(e=>e.Name);
            var exp11c = myCoreService.GetChildByNameAsync(exp1.Id,"exp1-1-c").Result;
            var exp111c = myCoreService.GetChildByNameAsync(exp11c.Id, "exp1-1-1").Result as TestExp;
            var sig111c = myCoreService.GetChildByNameAsync(exp11c.Id, "sig1-1-1").Result as TestSignal;
            var childrenOfExp2 = myCoreService.GetAllChildrenAsync(exp2.Id).Result.Single();
            var childOfNewExp12=myCoreService.GetAllChildrenAsync(childrenOfExp2.Id).Result;
            //assert 
            
            CollectionAssert.AreEquivalent(new List<string> { "exp1-1-c", "exp1-1", "exp1-2" }, childrenNameOfExp1.ToList());
            Assert.AreEqual(exp111.TestString,exp111c.TestString);
            Assert.AreNotEqual(exp111.Id, exp111c.Id);
            Assert.AreNotEqual(exp111.ParentId, exp111c.ParentId);
            Assert.AreEqual(sig111.TestString, sig111c.TestString);
            CollectionAssert.AreEqual(exp111.TestList,exp111c.TestList);
            CollectionAssert.AreEqual(sig111.TestList, sig111c.TestList);
            Assert.AreEqual(0, childOfNewExp12.Count());
        }


        /// <summary>
        /// 测试Update操作
        /// </summary>
        [TestMethod]
        public void UpdateTest()
        {
            //arrange
            var exp1 = new TestExp("exp1");
            var sig11 = new TestSignal("sig1-1");
            exp1.TestList.Add("String1");
          
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();

            //act&assert
            //1 update mistakenly
            string errorUpdateField = "";
            var updates = new Dictionary<string,UpdateEntity>();
            updates.Add("Id", new UpdateEntity { Operator = OperatorType.Set, Value = Guid.NewGuid() });
            try
            {
                myCoreService.UpDateAsync(exp1.Id, updates).Wait();
            }
            catch (Exception e)
            {
                errorUpdateField = e.InnerException.Message;
                updates.Clear();
            }
            Assert.AreEqual(ErrorMessages.NotValidUpdateFieldError,errorUpdateField);

            //2 update signal properly
            updates.Add("TestString", new UpdateEntity { Operator = OperatorType.Set, Value = "NewTestString" });
            updates.Add("TestList", new UpdateEntity { Operator = OperatorType.Push, Value = new List<string> { "String1", "String2"} });
            myCoreService.UpDateAsync(sig11.Id, updates).Wait();
            var sig11Result = (TestSignal)myCoreService.GetOneByIdAsync(sig11.Id).Result;
            Assert.AreEqual("NewTestString", sig11Result.TestString);
            Assert.AreEqual(2, sig11Result.TestList.Count);

            updates.Clear();
            //3 update expermient properly
            updates.Add("TestString", new UpdateEntity { Operator = OperatorType.Set, Value = "NewTestString" });
            updates.Add("TestList", new UpdateEntity { Operator = OperatorType.Push, Value = "String2" });
            myCoreService.UpDateAsync(exp1.Id, updates).Wait();
            var exp1Result = (TestExp)myCoreService.GetOneByIdAsync(exp1.Id).Result;
            Assert.AreEqual("NewTestString", exp1Result.TestString);
            Assert.AreEqual(2, exp1Result.TestList.Count);
        }

        /// <summary>
        /// 修改名字
        /// </summary>
        [TestMethod]
        public void RenameTest()
        {
            //arrange
            var exp1 = new TestExp("exp1");
            var exp2 = new Experiment("exp2");
            var sig11 = new TestSignal("sig1-1"); exp1.TestList.Add("String1");

            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp2).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();

            //act&assert
            //1 rename mistakely。
            var errorRenameEntity = "";
            try
            {
                myCoreService.ReNameAsync(exp1.Id, exp2.Name).Wait();
            }
            catch(Exception e)
            {
                errorRenameEntity = e.InnerException.Message;
            }
            Assert.AreEqual(ErrorMessages.NameDuplicateError,errorRenameEntity);

            //2 rename properly
            myCoreService.ReNameAsync(exp1.Id,"newExp1").Wait();
            var exp1Result = myCoreService.GetOneByIdAsync(exp1.Id).Result;
            var sig11Result = myCoreService.GetOneByIdAsync(sig11.Id).Result;
            Assert.AreEqual("newExp1",exp1Result.Name);
            Assert.AreEqual("/newExp1", exp1Result.Path);
            Assert.AreEqual(sig11.Name,sig11Result.Name);
            Assert.AreEqual("/newExp1/sig1-1", sig11Result.Path);
        }


        //[TestMethod]
        //public void ParentTest()
        //{
        //    //arrange
        //    var exp1 = new Experiment("exp1");
        //    var exp2 = new Experiment("exp2");
        //    var exp11 = new Experiment("exp1-1");
        //    var exp12 = new Experiment("exp1-2");
        //    var sig121 = new Signal("sig1-2-1");

        //    //add 2 root
        //    myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
        //    myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp2).Wait();

        //    //add child
        //    myCoreService.AddJdbcEntityToAsync(exp1.Id, exp11).Wait();
        //    myCoreService.AddJdbcEntityToAsync(exp1.Id, exp12).Wait();
        //    myCoreService.AddJdbcEntityToAsync(exp12.Id, sig121).Wait();

        //    //act
        //    //get child for later asserter
        //    var roots = myCoreService.GetAllChildrenAsync().Result;
        //    var secLev = myCoreService.GetAllChildrenAsync(exp1).Result;
        //    var trdLevById = myCoreService.GetAllChildrenAsync(exp12.Id).Result;

        //    //assert
        //    Assert.AreEqual(Guid.Empty, roots.SingleOrDefault(r => r.Name == exp1.Name).ParentId);
        //    Assert.AreEqual(Guid.Empty, roots.SingleOrDefault(r => r.Name == exp2.Name).ParentId);
        //    Assert.AreEqual(exp1.Id, secLev.SingleOrDefault(r => r.Name == exp11.Name).ParentId);
        //    Assert.AreEqual(exp1.Id, secLev.SingleOrDefault(r => r.Name == exp12.Name).ParentId);
        //    Assert.AreEqual(exp12.Id, trdLevById.SingleOrDefault(r => r.Name == sig121.Name).ParentId);
        //}

        //[TestMethod]
        //public void QueryByPathTest()
        //{
        //    //arrange
        //    var exp1 = new Experiment("exp1");
        //    var exp2 = new Experiment("exp2");
        //    var exp11 = new Experiment("exp1-1");
        //    var exp12 = new Experiment("exp1-2");
        //    var sig121 = new Signal("sig1-2-1");
        //    //add 2 root
        //    myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
        //    myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp2).Wait();
        //    //add child
        //    myCoreService.AddJdbcEntityToAsync(exp1.Id, exp11).Wait();
        //    myCoreService.AddJdbcEntityToAsync(exp1.Id, exp12).Wait();
        //    myCoreService.AddJdbcEntityToAsync(exp12.Id, sig121).Wait();

        //    //act
        //    //get child for later asserter
        //    var roots = myCoreService.GetAllChildrenAsync().Result;
        //    var secLev = myCoreService.GetAllChildrenAsync(exp1).Result;
        //    var trdLevById = myCoreService.GetAllChildrenAsync(exp12.Id).Result;
        //    var sig121ByPath = myCoreService.GetOneByPathAsync("/exp1/exp1-2/sig1-2-1").Result;///By WKH
        //    //assert
        //    //path assert
        //    Assert.AreEqual(@"/exp1", roots.SingleOrDefault(r => r.Name == exp1.Name).Path);
        //    Assert.AreEqual(@"/exp2", roots.SingleOrDefault(r => r.Name == exp2.Name).Path);
        //    Assert.AreEqual(@"/exp1/exp1-1", secLev.SingleOrDefault(r => r.Name == exp11.Name).Path);
        //    Assert.AreEqual(@"/exp1/exp1-2", secLev.SingleOrDefault(r => r.Name == exp12.Name).Path);
        //    Assert.AreEqual(@"/exp1/exp1-2/sig1-2-1", trdLevById.SingleOrDefault(r => r.Name == sig121.Name).Path);
        //    Assert.AreEqual(sig121.Id, sig121ByPath.Id);///By WKH
        //}
    }


    public class TestSignal:Signal
    {
        public string TestString { get; set; }
        public List<string> TestList { get; set; }

        public TestSignal(string name):base(name)
        {
            TestList = new List<string>();
        }
    }

    public class TestExp :Experiment
    {
        public TestExp(string name):base(name)
        {
            TestList = new List<string>();
        }

        public string TestString { get; set; }
        public List<string> TestList { get; set; }
    }


}
