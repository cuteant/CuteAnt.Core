/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace CuteAnt.OrmLite
{
  /// <summary>实体列表接口</summary>
  public partial interface IEntityList : /*IList, */ IEnumerable, IList<IEntity>
  {
    #region 对象查询

    /// <summary>根据指定项查找</summary>
    /// <param name="name">属性名</param>
    /// <param name="value">属性值</param>
    /// <returns></returns>
    IEntityList FindAll(String name, Object value);

    /// <summary>根据指定项查找字符串。忽略大小写</summary>
    /// <param name="name">属性名</param>
    /// <param name="value">属性值</param>
    /// <returns></returns>
    IEntityList FindAllIgnoreCase(String name, String value);

    /// <summary>根据指定项查找</summary>
    /// <param name="names">属性名</param>
    /// <param name="values">属性值</param>
    /// <returns></returns>
    IEntityList FindAll(String[] names, Object[] values);

    /// <summary>根据指定项查找，对于字符串字段忽略大小写</summary>
    /// <param name="names">属性名</param>
    /// <param name="values">属性值</param>
    /// <returns></returns>
    IEntityList FindAllIgnoreCase(String[] names, Object[] values);

    /// <summary>检索与指定谓词定义的条件匹配的所有元素。</summary>
    /// <param name="predicate">用于测试每个元素是否满足条件的函数</param>
    /// <returns></returns>
    IEntityList FindAll(Func<IEntity, Boolean> predicate);

    /// <summary>根据指定项查找</summary>
    /// <param name="name">属性名</param>
    /// <param name="value">属性值</param>
    /// <returns></returns>
    IEntity Find(String name, Object value);

    /// <summary>根据指定项查找字符串。忽略大小写</summary>
    /// <param name="name">属性名</param>
    /// <param name="value">属性值</param>
    /// <returns></returns>
    IEntity FindIgnoreCase(String name, String value);

    /// <summary>集合是否包含指定项</summary>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <returns></returns>
    Boolean Exists(String name, Object value);

    /// <summary>分页</summary>
    /// <param name="startRowIndex">起始索引，0开始</param>
    /// <param name="maximumRows">最大个数</param>
    /// <returns></returns>
    IEntityList Page(Int32 startRowIndex, Int32 maximumRows);

    #endregion

    #region 对象操作

    /// <summary>把整个集合插入到数据库</summary>
    /// <param name="useTransition">是否使用事务保护</param>
    /// <returns></returns>
    Int32 Insert(Boolean useTransition);

    /// <summary>把整个集合插入到数据库，不需要验证</summary>
    /// <param name="useTransition">是否使用事务保护</param>
    /// <remarks>## 苦竹 添加 2014.04.01 23:45 ##</remarks>
    /// <returns></returns>
    Int32 InsertWithoutValid(Boolean useTransition);

    /// <summary>把整个集合更新到数据库</summary>
    /// <param name="useTransition">是否使用事务保护</param>
    /// <returns></returns>
    Int32 Update(Boolean useTransition);

    /// <summary>把整个保存更新到数据库</summary>
    /// <param name="useTransition">是否使用事务保护</param>
    /// <returns></returns>
    Int32 Save(Boolean useTransition);

    /// <summary>把整个保存更新到数据库，保存时不需要验证</summary>
    /// <param name="useTransition">是否使用事务保护</param>
    /// <remarks>## 苦竹 添加 2014.04.01 16:45 ##</remarks>
    /// <returns></returns>
    Int32 SaveWithoutValid(Boolean useTransition);

    /// <summary>把整个集合从数据库中删除</summary>
    /// <param name="useTransition">是否使用事务保护</param>
    /// <returns></returns>
    Int32 Delete(Boolean useTransition);

    /// <summary>设置所有实体中指定项的值</summary>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    IEntityList SetItem(String name, Object value);

    /// <summary>获取所有实体中指定项的值</summary>
    /// <typeparam name="TResult">指定项的类型</typeparam>
    /// <param name="name">名称</param>
    /// <returns></returns>
    List<TResult> GetItem<TResult>(String name);

    /// <summary>串联指定成员，方便由实体集合构造用于查询的子字符串</summary>
    /// <param name="name">名称</param>
    /// <param name="separator"></param>
    /// <returns></returns>
    String Join(String name, String separator);

    /// <summary>串联</summary>
    /// <param name="separator"></param>
    /// <returns></returns>
    String Join(String separator);

    #endregion

    #region 排序

    /// <summary>按指定字段排序</summary>
    /// <param name="name">字段</param>
    /// <param name="isDesc">是否降序</param>
    IEntityList Sort(String name, Boolean isDesc);

    /// <summary>按指定字段数组排序</summary>
    /// <param name="names">字段</param>
    /// <param name="isDescs">是否降序</param>
    IEntityList Sort(String[] names, Boolean[] isDescs);

    // ## 苦竹 添加 2013.01.10 PM 14:13 ##
    /// <summary>置顶指定实体，加大排序键的值</summary>
    /// <param name="entity"></param>
    /// <param name="sortKey"></param>
    /// <returns></returns>
    IEntityList Top(IEntity entity, String sortKey);

    /// <summary>提升指定实体在当前列表中的位置，加大排序键的值</summary>
    /// <param name="entity"></param>
    /// <param name="sortKey"></param>
    /// <returns></returns>
    IEntityList Up(IEntity entity, String sortKey);

    /// <summary>降低指定实体在当前列表中的位置，减少排序键的值</summary>
    /// <param name="entity"></param>
    /// <param name="sortKey"></param>
    /// <returns></returns>
    IEntityList Down(IEntity entity, String sortKey);

    // ## 苦竹 添加 2013.01.10 PM 14:13 ##
    /// <summary>置底指定实体，减少排序键的值</summary>
    /// <param name="entity"></param>
    /// <param name="sortKey"></param>
    /// <returns></returns>
    IEntityList Bottom(IEntity entity, String sortKey);

    #endregion

    #region 导入导出

    // ## 苦竹 屏蔽 2013.01.07 AM01:42 ##
    ///// <summary>导出Xml文本</summary>
    ///// <returns></returns>
    //String ToXml();

    /// <summary>导入Xml文本</summary>
    /// <param name="xml"></param>
    IEntityList FromXml(String xml);

    /// <summary>导出Json</summary>
    /// <returns></returns>
    String ToJson();

    ///// <summary>
    ///// 导入Json
    ///// </summary>
    ///// <param name="json"></param>
    ///// <returns></returns>
    //IEntityList FromJson(String json);

    /// <summary>实体列表转为字典。主键为Key</summary>
    /// <param name="valueField">作为Value部分的字段，默认为空表示整个实体对象为值</param>
    /// <returns></returns>
    IDictionary ToDictionary(String valueField);

    #endregion

    #region 导出DataSet数据集

    /// <summary>转为DataTable</summary>
    /// <param name="allowUpdate">是否允许更新数据，如果允许，将可以对DataTable进行添删改等操作</param>
    /// <returns></returns>
    DataTable ToDataTable(Boolean allowUpdate);

    /// <summary>转为DataSet</summary>
    /// <returns></returns>
    DataSet ToDataSet();

    #endregion
  }
}