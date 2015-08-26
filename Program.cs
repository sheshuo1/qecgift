using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using qecgift.DB;

namespace qecgift
{
    class Program
    {
        static void Main(string[] args)
        {
            qecDataContext DB = new qecDataContext();
            qec1DataContext DB1 = new qec1DataContext();
                           
            try
            {
                int[] gem = new int[5] { 15000, 5000, 1000, 250, 100 };  //各等级的玩家每日发放的钻石数
                string[] group = new string[5]{"豪R","大R","中R","小R","非R"};  //各等级玩家称谓
                //int sum = 0;   //各等级发放礼包个数
                var vip = DB.t_vip;
                
                for (int g = 1; g <= 5; g++)   //从1-5等级循环发放每日钻石礼包
                {
                    var test = from t in DB.t_testuser where t.groupid == g && t.gifttimes > 0 select t;//从封测玩家表中选出相应等级并且剩余礼包发放次数大于0的玩家列表
                    foreach (var i in test)//遍历该列表
                    {
                        try
                        {
                            var inf = from u in DB1.t_user_info where u.user_id == i.user_id select u;//选取玩家信息表中对应ID的玩家信息
                            if (inf.Count() > 0)//如果该玩家信息存在
                            {
                                int gem1 = gem[g - 1];
                                inf.First().user_gem += gem1;//玩家钻石增加相应数量

                                inf.First().topup_num += (gem1 / 10);//玩家充值记录增加相应金额

                                int vip_id = vip.Where(v => v.topUp_num <= inf.First().topup_num).Max(v => v.vip_id);//判断玩家充值金额可达到的VIP等级
                                inf.First().vip = vip_id;//设置玩家VIP等级
                                i.gifttimes--;//玩家礼包剩余发放次数减1
                                t_giftlog log = new t_giftlog();
                                log.user_id = inf.First().user_id;
                                log.addgem = gem1;
                                log.addtime = DateTime.Now;
                                DB.t_giftlog.InsertOnSubmit(log);

                            }
                        }
                        catch (System.Exception ex)
                        {
                            WriteLog.WriteError("用户ID:" + i.user_id + "；\r\n" + group[g - 1] + "；\r\n" + "账号：" + i.user_name + "；\r\n" + "错误信息：" + ex.ToString());
                        }

                    }
                    DB1.SubmitChanges();
                    WriteLog.WriteError(group[g - 1] + "发放完成！");
                    DB.SubmitChanges();//提交数据库更改
                    
                    //Console.WriteLine("成功1");
                    //Console.ReadKey();
                    //sum = 0;//礼包数量清零
                }
               
                
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine("错误1"+ex);
                //Console.ReadKey();
                WriteLog.WriteError(ex.ToString());
                //throw ex;
            }
        
        }
    }
}
