using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.JDBC.Core.Models
{
    public class ErrorMessages
    {
        /// <summary>
        /// 名字重复
        /// </summary>
        public static string NameDuplicateError
        {
            get { return "The entity with the same name already exists under the same parent!"; }
        }

        /// <summary>
        /// 添加不合法的父节点
        /// </summary>
        public static string NotValidParentError
        {
            get { return "The parent is not valid, Cannot add signal to the root, or , You can not add entity as child to a Signal entity, you can only to Experiment entity!"; }
        }
  

        /// <summary>
        /// 节点循环
        /// </summary>
        public static string ParentLoopError
        {
            get { return "The new parent is already a child entity of the eneity you are adding to it!"; }
        }

        /// <summary>
        /// 删除的节点有子节点，必须指定Recursive=true
        /// </summary>
        public static string DeleteEntityWithChildError
        {
            get { return "The entity you try to delet has child entities, you can not delete it unless you have specified. (set Recursive=true)"; }
        }

        /// <summary>
        /// 父节点不存在
        /// </summary>
        public static string ParentNotExistError
        {
            get { return "The parents you have specified does not exists!"; }
        }

        /// <summary>
        /// 节点不存在
        /// </summary>
        public static string EntityNotFoundError
        {
            get { return "Could not find the entity!"; }
        }
        //感觉可以和上面的合并
        public static string ExperimentOrSignalNotFoundError
        {
            get { return "Experiment or Signal you required is not found!"; }
        }
        /// <summary>
        /// 不合法的更新操作
        /// </summary>
        public static string NotValidUpdateOperatorError
        {
            get { return "The operator of update is not valid!"; }
        }
        /// <summary>
        /// 不允许对此字段进行更新
        /// </summary>
        public static string NotValidUpdateFieldError
        {
            get { return "The Field of update is not valid!"; }
        }
        public static string SampleNotExistsError
        {
            get { return "Some or all the samples you required does not exist!"; }
        }

        public static string DataTypePlugInIsNull
        {
            get { return "DataTypePlugIn should not be null!"; }
        }
        public static string NotValidDataTypeError
        {
            get { return "The data type is not supported , cannot get proper plugin!"; }
        }
        public static string NotValidURIError
        {
            get { return "The string is not valid for URI!"; }
        }
        //public static string NotValidUpdateValueError
        //{
        //    get { return "The value of the field is not valid!"; }
        //}
        //public static string PropertyNotExistsError
        //{
        //    get { return "The field of the object dosen't exists!"; }
        //}

        /// <summary>
        /// 复制数据时，需指定isRecuresive == true && duplicatePayload == true
        /// </summary>
        public static string NotValidDuplicateOperatorError
        {
            get { return "The Experiment cannot duplicate payload,make sure isRecuresive == true && duplicatePayload == true!"; }
        }

        /// <summary>
        /// 数据库配置文件不合法
        /// </summary>
        public static string NotValidConfigDatabaseError
        {
            get { return "The configString is not valid to start database!"; }
        }

        /// <summary>
        /// 数据库引擎为空
        /// </summary>
        public static string NotValidStorageEngine
        {
            get { return "The storage engine is null,initialize a storage engine  first!"; }
        }

        /// <summary>
        /// 读取数据时，不合法的字符串
        /// </summary>
        public static string NotValidSignalFragmentError
        {
            get { return "The signal fragment for get data is not valid!"; }
        }
        public static string NotValidSignalError
        {
            get { return "The signal is not valid!"; }
        }
      
        /// <summary>
        /// 不合法的ID
        /// </summary>
        public static string NotValidIdError
        {
            get { return "The string is not valid for Guid!"; }
        }

        /// <summary>
        ///请求数据超出边界错误
        /// </summary>
        public static string OutOfRangeError
        {
            get { return "The data you wanted get is out of range ,please check again!"; }
        }

        /// <summary>
        /// 找不到数据
        /// </summary>
        public static string DataNotFoundError
        {
            get { return "The data you wanted get could not find,please check again!"; }
        }
        public static string NotValidFileError
        {
            get { return "The file is not Dynamic Link Library file,please check again!"; }
        }

        public static string FileNotFoundError
        {
            get { return "The file can not be found!"; }
        }

        public static string SignalLockedError
        {
            get { return "The signal you want to write is been using by others,please free the signal first!"; }
        }
        public static string WrongUserError
        {
            get { return "The user do not have the permission to authorize!"; }
        }
        public static string NotValidOperateofSignalError
        {
            get { return "Can not put data of Expression Signal!"; }
        }

        /// <summary>
        /// 初始化FixedIntervalWaveSignal字符串不合法
        /// </summary>
        public static string NotValidInitStringError
        {
            get { return "The Initial string must contain SampleInterval field or SampleInterval>0!"; }
        }

        /// <summary>
        /// 表达式不合法
        /// </summary>
        public static string NotValidExpressionStringError
        {
            get { return "The Expression String is not valid，JDBC.Signal must contain!"; }
        }

        /// <summary>
        /// 没有找到表达式
        /// </summary>
        public static string ExpressionNotFoundError
        {
            get { return "The signal must have extra information of 'expression'!"; }
        }
    }
}
