using Jtext103.JDBC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    /// <summary>
    /// data transfer object，所有entity DTO应继承此类
    /// </summary>
    public class NodeDto
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public string Name { get; set; }
        public JDBCEntityType EntityType { get; set; }
        public string Path { get; set; }
        public Dictionary<string, object> ExtraInformation { get; set; }
        
        // 权限相关
        public string User { get; private set; }
        public Dictionary<string, string> GroupPermission { get; set; }
        public string OthersPermission { get; set; }
        public bool QueryToParentPermission { get; set; }
    }
    /// <summary>
    /// Experiment数据传输对象
    /// </summary>
    public class ExperimentDto: NodeDto
    {
        public int ChildrenCount { get; set; }
    }
    /// <summary>
    /// FixedIntervalWaveSignal数据传输对象
    /// </summary>
    public class FixedIntervalWaveSignalDto: NodeDto
    {
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public double SampleInterval { get; set; }
        public string Unit { get; set; }
    }
}