using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace SignalR_MVC_Chat.Hubs
{
    public class ChatHub : Hub
    {
        //用戶連線ID列表
        public static List<string> ConnIDList = new List<string>();

        /// <summary>
        /// 連線事件
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync() 
        {
            if (!ConnIDList.Contains(Context.ConnectionId))
            {
                ConnIDList.Add(Context.ConnectionId); //basic hub 中有實作，註冊完service應該就會自動幫忙接資料了
            }
            //更新連線 ID 列表 (全部人發送?)
            await Clients.All.SendAsync("UpdList", JsonConvert.SerializeObject(ConnIDList)); //也是basic hub 中有實作，吃json字串? 結果好像是前端另外自己要實作@@
            //更新個人 ID(連進來這個實體的這位?用ID定位?單獨發送?)
            await Clients.Client(Context.ConnectionId).SendAsync("UpdSelfID",Context.ConnectionId);
            //更新聊天內容
            await Clients.All.SendAsync("UpdContent", "新連線 ID: " + Context.ConnectionId);
            //這些Upd開頭的字串好像是觸發前端的事件方法ㄟ?
            //用override + base 方法來擴充原本的功能
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            //remove 失敗會 return false 不會錯誤，就沒另外鑒察
            ConnIDList.Remove(Context.ConnectionId);
            //更新連線 ID 列表 
            await Clients.All.SendAsync("UpdList", JsonConvert.SerializeObject(ConnIDList));
            //更新聊天內容
            await Clients.All.SendAsync("UpdContent", "已離線 ID: " + Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);
        }
        /// <summary>
        /// 傳送訊息(這好像是自己實作的?)
        /// </summary>
        /// <param name="selfID"></param>
        /// <param name="message"></param>
        /// <param name="sendToID"></param>
        /// <returns></returns>
        public async Task SendMessage(string selfID, string message, string sendToID)
        {
            if (string.IsNullOrEmpty(sendToID))
            {
                //全部發送
                await Clients.All.SendAsync("UpdContent", selfID + " 說: " + message);
            }
            else
            {
                //單獨發送
                //接收人
                await Clients.Client(sendToID).SendAsync("UpdContent", selfID + " 私訊向你說: " + message);
                //發送人
                await Clients.Client(Context.ConnectionId).SendAsync("UpdContent", "你向 " + sendToID + " 私訊說: " + message);
            }
        }
        
    }
}
