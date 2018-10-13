using System;
using System.Collections.Generic;
using System.Text;

public class Order
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int AccountId { get; set; }
    /// <summary>
    /// 期数编号
    /// </summary>
    public int PeriodNo { get; set; }
    /// <summary>
    /// 订单号
    /// </summary>
    public string OrderNo { get; set; }
    /// <summary>
    /// 单注金额
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// 注数
    /// </summary>
    public int BetCount { get; set; }
    /// <summary>
    /// 金额单位（元角分）
    /// </summary>
    public int MoneyUnit { get; set; }
    /// <summary>
    /// 总金额
    /// </summary>
    public decimal TotalAmount { get; set; }
    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; }
    /// <summary>
    /// 游戏ID
    /// </summary>
    public int GameId { get; set; }
    /// <summary>
    /// 玩法ID
    /// </summary>
    public int PlayItemId { get; set; }
    /// <summary>
    /// 订单时间
    /// </summary>
    public DateTime CreateTime { get; set; }
}
