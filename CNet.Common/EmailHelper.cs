using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
   public  class EmailHelper
    {

       /// <summary>
       /// 发送邮件
       /// </summary>
       /// <param name="smtpServer">smtp服务器</param>
       /// <param name="port">端口</param>
       /// <param name="mailFrom">发件人邮箱</param>
       /// <param name="userPassword">密码</param>
       /// <param name="mailSubject">邮件主题</param>
       /// <param name="mailContent">内容</param>
        ///  /// <param name="strcc">抄送人</param>
        /// <param name="strBcc">密送</param>
       /// <param name="strs">附件</param>
       /// <returns>发送成功返回true否则false</returns>
       public static bool SendEmail(string smtpServer,  
           string mailFrom, string userPassword, string mailTo, 
           string mailSubject, string mailContent,string strcc="",string strBcc="", string strs="")
       {
           try
           {
               // 设置发送方的邮件信息
               // 邮件服务设置
               SmtpClient smtpClient = new SmtpClient();
               smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式
               smtpClient.Host = smtpServer; //指定SMTP服务器
               //smtpClient.Port = port;//端口
               smtpClient.Credentials = new System.Net.NetworkCredential(mailFrom, userPassword);//验证用户名和密码
               smtpClient.EnableSsl = true; //使用SSL
               // 发送邮件设置       
               MailMessage mailMessage = new MailMessage(mailFrom, mailTo); // 发送人和收件人
 
               mailMessage.Subject = mailSubject;//主题
               mailMessage.Body = mailContent;//内容
               mailMessage.BodyEncoding = Encoding.UTF8;//正文编码
               mailMessage.IsBodyHtml = true;//设置为HTML格式
               mailMessage.Priority = MailPriority.Normal;//优先级
               //抄送人
               if (!string.IsNullOrEmpty(strcc))
                   mailMessage.CC.Add(strcc);
               //密送
               if (!string.IsNullOrEmpty(strBcc))
                   mailMessage.Bcc.Add(strBcc);
               //附件
               if (!string.IsNullOrEmpty(strs))
               {
                   List<string> paths = new List<string>();
                   if (strs.Contains(","))
                   {
                       paths = strs.Split(',').ToList();
                   }
                   else
                   {
                       paths.Add(strs);
 
                   }
                   foreach (var path in paths)
                   {
                       mailMessage.Attachments.Add(new Attachment(strs));
                   }
               }
               smtpClient.Send(mailMessage); // 发送邮件
               return true;
           }
           catch
           {
               return false;
           }
       }

       //调用
         ////获得各种参数，不需要的用空字符串
         //   string path = Directory.GetCurrentDirectory();
         //   string file = path + @"\Excel.xlsx"; //附件1
         //   string file2 = path + @"\Excel2.xlsx";//附件2
         //   string smtpServer = "smtp.163.com";//163邮箱的smtp服务器 
         //   int port = 25;//端口
         //   string mailFrom = "******@163.com";//发件人邮箱 
         //   string pwd = "*********";//密码
         //   string mailTo = "123@163.com,456@qq.com";//收件人邮箱,多个用户用逗号隔开
         //   string mailCC = "";//抄送人,多个用户用逗号隔开
         //   string mailBcc = "";//密送
         //   string mailSubject = "测试邮件";//主题
         //   string mailContent = "HI,这是我发给你的一个测试邮件";//内容
         //   string ah = file+","+file2; //附件-文件路径
         //   if (EmailHelper.SendEmail(smtpServer, port, mailFrom, pwd, mailTo, mailCC, mailBcc, mailSubject, mailContent, ah) == true)
         //   {
         //       Console.WriteLine("发送成功!");
         //   }
         //   else
         //       Console.WriteLine("发送失败");
           
         //   Console.ReadKey();
       
       /// <summary>
       /// 发送邮件
       /// </summary>
       /// <param name="smtpServer">smtp服务器</param>
       /// <param name="port">端口</param>
       /// <param name="mailFrom">发件人邮箱</param>
       /// <param name="userPassword">密码</param>
       /// <param name="mailSubject">邮件主题</param>
       /// <param name="mailContent">内容</param>
        ///  /// <param name="strcc">抄送人</param>
        /// <param name="strBcc">密送</param>
       /// <param name="strs">附件</param>
       /// <returns>发送成功返回true否则false</returns>
       public static bool SendEmail(string mailTo,
           string mailSubject, string mailContent, string strcc = "", string strBcc = "", string strs = "")
       {
           string smtpServer = ConfigurationManager.AppSettings["Mail.SmtpServer"];
           string mailFrom = ConfigurationManager.AppSettings["Mail.UserName"];
           string userPassword = ConfigurationManager.AppSettings["Mail.Pwd"]; ;
           return SendEmail(smtpServer,mailFrom,userPassword,mailTo,mailSubject,mailContent,strcc,strBcc,strs);
       }
    }
}
