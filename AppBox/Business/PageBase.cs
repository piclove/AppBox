using System;
using System.Web.Security;
using System.Web.UI;
using System.Reflection;
using System.Collections.Generic;
using FineUI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using System.Web;
using System.Data.Entity;


namespace AppBox
{
    public class PageBase : System.Web.UI.Page
    {
        #region 只读静态变量

        // Session key
        private static readonly string SK_ONLINE_UPDATE_TIME = "OnlineUpdateTime";
        //private static readonly string SK_USER_ROLE_ID = "UserRoleId";

        private static readonly string CHECK_POWER_FAIL_PAGE_MESSAGE = "您无权访问此页面！";
        private static readonly string CHECK_POWER_FAIL_ACTION_MESSAGE = "您无权进行此操作！";



        #endregion

        #region 实体上下文

        public static AppBoxContext DB
        {
            get
            {
                // http://stackoverflow.com/questions/6334592/one-dbcontext-per-request-in-asp-net-mvc-without-ioc-container
                if (!HttpContext.Current.Items.Contains("__AppBoxContext"))
                {
                    HttpContext.Current.Items["__AppBoxContext"] = new AppBoxContext();
                }
                return HttpContext.Current.Items["__AppBoxContext"] as AppBoxContext;
            }
        }

        #endregion

        #region 浏览权限

        /// <summary>
        /// 本页面的浏览权限，空字符串表示本页面不受权限控制
        /// </summary>
        public virtual string ViewPower
        {
            get
            {
                return String.Empty;
            }
        }

        #endregion

        #region 页面初始化

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // 此用户是否有访问此页面的权限
            if (!CheckPowerView())
            {
                CheckPowerFailWithPage();
                return;
            }

            // 设置主题
            if (PageManager.Instance != null)
            {
                PageManager.Instance.Theme = (Theme)Enum.Parse(typeof(Theme), ConfigHelper.Theme, true);
            }

            UpdateOnlineUser(User.Identity.Name);

            // 设置页面标题
            Page.Title = ConfigHelper.Title;
        }

        #endregion

        #region 请求参数

        /// <summary>
        /// 获取查询字符串中的参数值
        /// </summary>
        protected string GetQueryValue(string queryKey)
        {
            return Request.QueryString[queryKey];
        }


        /// <summary>
        /// 获取查询字符串中的参数值
        /// </summary>
        protected int GetQueryIntValue(string queryKey)
        {
            int queryIntValue = -1;
            try
            {
                queryIntValue = Convert.ToInt32(Request.QueryString[queryKey]);
            }
            catch (Exception)
            {
                // TODO
            }

            return queryIntValue;
        }

        #endregion

        #region 表格相关

        protected int GetSelectedDataKeyID(Grid grid)
        {
            int id = -1;
            int rowIndex = grid.SelectedRowIndex;
            if (rowIndex >= 0)
            {
                id = Convert.ToInt32(grid.DataKeys[rowIndex][0]);
            }
            return id;
        }

        protected string GetSelectedDataKey(Grid grid, int dataIndex)
        {
            string data = String.Empty;
            int rowIndex = grid.SelectedRowIndex;
            if (rowIndex >= 0)
            {
                data = grid.DataKeys[rowIndex][dataIndex].ToString();
            }
            return data;
        }

        /// <summary>
        /// 获取表格选中项DataKeys的第一个值，并转化为整型列表
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        protected List<int> GetSelectedDataKeyIDs(Grid grid)
        {
            List<int> ids = new List<int>();
            foreach (int rowIndex in grid.SelectedRowIndexArray)
            {
                ids.Add(Convert.ToInt32(grid.DataKeys[rowIndex][0]));
            }

            return ids;
        }

        #endregion

        #region EF相关

        // 排序
        protected IQueryable<T> Sort<T>(IQueryable<T> q, FineUI.Grid grid)
        {
            return q.SortBy(grid.SortField + " " + grid.SortDirection);
        }

        // 排序后分页
        protected IQueryable<T> SortAndPage<T>(IQueryable<T> q, FineUI.Grid grid)
        {
            if (grid.PageIndex >= grid.PageCount && grid.PageCount >= 1)
            {
                grid.PageIndex = grid.PageCount - 1;
            }

            return Sort(q, grid).Skip(grid.PageIndex * grid.PageSize).Take(grid.PageSize);
        }


        // 附加实体到数据库上下文中（首先在Local中查找实体是否存在，不存在才Attach，否则会报错）
        // http://patrickdesjardins.com/blog/entity-framework-4-3-an-object-with-the-same-key-already-exists-in-the-objectstatemanager
        protected T Attach<T>(int keyID) where T : class, IKeyID, new()
        {
            T t = DB.Set<T>().Local.Where(x => x.ID == keyID).FirstOrDefault();
            if (t == null)
            {
                t = new T { ID = keyID };
                DB.Set<T>().Attach(t);
            }
            return t;
        }

        // 向现有实体集合中添加新项
        protected void AddEntities<T>(ICollection<T> existItems, int[] newItemIDs) where T : class,  IKeyID, new()
        {
            foreach (int roleID in newItemIDs)
            {
                T t = Attach<T>(roleID);
                existItems.Add(t);
            }
        }

        // 替换现有实体集合中的所有项
        // http://stackoverflow.com/questions/2789113/entity-framework-update-entity-along-with-child-entities-add-update-as-necessar
        protected void ReplaceEntities<T>(ICollection<T> existEntities, int[] newEntityIDs) where T : class,  IKeyID, new()
        {
            if (newEntityIDs.Length == 0)
            {
                existEntities.Clear();
            }
            else
            {
                int[] tobeAdded = newEntityIDs.Except(existEntities.Select(x => x.ID)).ToArray();
                int[] tobeRemoved = existEntities.Select(x => x.ID).Except(newEntityIDs).ToArray();

                AddEntities<T>(existEntities, tobeAdded);

                existEntities.Where(x => tobeRemoved.Contains(x.ID)).ToList().ForEach(e => existEntities.Remove(e));
                //foreach (int roleID in tobeRemoved)
                //{
                //    existEntities.Remove(existEntities.Single(r => r.ID == roleID));
                //}
            }
        }

        // http://patrickdesjardins.com/blog/validation-failed-for-one-or-more-entities-see-entityvalidationerrors-property-for-more-details-2
        // ((System.Data.Entity.Validation.DbEntityValidationException)$exception).EntityValidationErrors

        #endregion

        #region 在线用户相关

        protected void UpdateOnlineUser(string username)
        {
            DateTime now = DateTime.Now;
            object lastUpdateTime = Session[SK_ONLINE_UPDATE_TIME];
            if (lastUpdateTime == null || (Convert.ToDateTime(lastUpdateTime).Subtract(now).TotalMinutes > 5))
            {
                // 记录本次更新时间
                Session[SK_ONLINE_UPDATE_TIME] = now;

                Online online = DB.Onlines.Where(o => o.User.Name == username).FirstOrDefault();
                if (online != null)
                {
                    online.UpdateTime = now;
                    DB.SaveChanges();
                }
            }
        }

        protected void RegisterOnlineUser(User user)
        {
            Online online = DB.Onlines.Where(o => o.User.ID == user.ID).FirstOrDefault();

            // 如果不存在，就创建一条新的记录
            if (online == null)
            {
                online = new Online();
                DB.Onlines.Add(online);
            }
            DateTime now = DateTime.Now;
            online.User = user;
            online.IPAdddress = Request.UserHostAddress;
            online.LoginTime = now;
            online.UpdateTime = now;

            DB.SaveChanges();

            // 记录本次更新时间
            Session[SK_ONLINE_UPDATE_TIME] = now;

        }

        /// <summary>
        /// 在线人数
        /// </summary>
        /// <returns></returns>
        protected int GetOnlineCount()
        {
            DateTime lastM = DateTime.Now.AddMinutes(-15);
            return DB.Onlines.Where(o => o.UpdateTime > lastM).Count();
        }

        #endregion

        #region 当前登录用户信息

        // http://blog.163.com/zjlovety@126/blog/static/224186242010070024282/
        // http://www.cnblogs.com/gaoshuai/articles/1863231.html
        /// <summary>
        /// 当前登录用户的角色列表
        /// </summary>
        /// <returns></returns>
        protected List<int> GetIdentityRoleIDs()
        {
            List<int> roleIDs = new List<int>();

            if (User.Identity.IsAuthenticated)
            {
                FormsAuthenticationTicket ticket = ((FormsIdentity)User.Identity).Ticket;
                string userData = ticket.UserData;

                foreach (string roleID in userData.Split(','))
                {
                    if (!String.IsNullOrEmpty(roleID))
                    {
                        roleIDs.Add(Convert.ToInt32(roleID));
                    }
                }
            }

            return roleIDs;
        }

        /// <summary>
        /// 当前登录用户名
        /// </summary>
        /// <returns></returns>
        protected string GetIdentityName()
        {
            if (User.Identity.IsAuthenticated)
            {
                return User.Identity.Name;
            }
            return String.Empty;
        }


        /// <summary>
        /// 创建表单验证的票证并存储在客户端Cookie中
        /// </summary>
        /// <param name="userName">当前登录用户名</param>
        /// <param name="roleIDs">当前登录用户的角色ID列表</param>
        /// <param name="isPersistent">是否跨浏览器会话保存票证</param>
        /// <param name="expiration">过期时间</param>
        protected void CreateFormsAuthenticationTicket(string userName, string roleIDs, bool isPersistent, DateTime expiration)
        {
            // 创建Forms身份验证票据
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                userName,                       // 与票证关联的用户名
                DateTime.Now,                   // 票证发出时间
                expiration,                     // 票证过期时间
                isPersistent,                   // 如果票证将存储在持久性 Cookie 中（跨浏览器会话保存），则为 true；否则为 false。
                roleIDs                         // 存储在票证中的用户特定的数据
             );

            // 对Forms身份验证票据进行加密，然后保存到客户端Cookie中
            string hashTicket = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hashTicket);
            cookie.HttpOnly = true;
            // 1. 关闭浏览器即删除（Session Cookie）：DateTime.MinValue
            // 2. 指定时间后删除：大于 DateTime.Now 的某个值
            // 3. 删除Cookie：小于 DateTime.Now 的某个值
            if (isPersistent)
            {
                cookie.Expires = expiration;
            }
            else
            {
                cookie.Expires = DateTime.MinValue;
            }
            Response.Cookies.Add(cookie);
        }

        #endregion

        #region 权限检查

        /// <summary>
        /// 检查当前用户是否拥有当前页面的浏览权限
        /// 页面需要先定义ViewPower属性，以确定页面与某个浏览权限的对应关系
        /// </summary>
        /// <returns></returns>
        protected bool CheckPowerView()
        {
            return CheckPower(ViewPower);
        }

        /// <summary>
        /// 检查当前用户是否拥有某个权限
        /// </summary>
        /// <param name="powerType"></param>
        /// <returns></returns>
        protected bool CheckPower(string powerName)
        {
            // 如果权限名为空，则放行
            if (String.IsNullOrEmpty(powerName))
            {
                return true;
            }

            // 当前登陆用户的权限列表
            List<string> rolePowerNames = GetRolePowerNames();
            if (rolePowerNames.Contains(powerName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取当前登录用户拥有的全部权限列表
        /// </summary>
        /// <param name="roleIDs"></param>
        /// <returns></returns>
        protected List<string> GetRolePowerNames()
        {
            // 将用户拥有的权限列表保存在Session中，这样就避免每个请求多次查询数据库
            if (Session["UserPowerList"] == null)
            {
                List<string> rolePowerNames = new List<string>();

                // 超级管理员拥有所有权限
                if (GetIdentityName() == "admin")
                {
                    rolePowerNames = DB.Powers.Select(p => p.Name).ToList();
                }
                else
                {
                    List<int> roleIDs = GetIdentityRoleIDs();

                    foreach (var role in DB.Roles.Include(r => r.Powers).Where(r => roleIDs.Contains(r.ID)))
                    {
                        foreach (var power in role.Powers)
                        {
                            if (!rolePowerNames.Contains(power.Name))
                            {
                                rolePowerNames.Add(power.Name);
                            }
                        }
                    }
                }

                Session["UserPowerList"] = rolePowerNames;
            }
            return (List<string>)Session["UserPowerList"];
        }

        #endregion

        #region 权限相关

        protected void CheckPowerFailWithPage()
        {
            Response.Write(CHECK_POWER_FAIL_PAGE_MESSAGE);
            Response.End();
        }

        protected void CheckPowerFailWithButton(FineUI.Button btn)
        {
            btn.Enabled = false;
            btn.ToolTip = CHECK_POWER_FAIL_ACTION_MESSAGE;
        }

        protected void CheckPowerFailWithLinkButtonField(FineUI.Grid grid, string columnID)
        {
            FineUI.LinkButtonField btn = grid.FindColumn(columnID) as FineUI.LinkButtonField;
            btn.Enabled = false;
            btn.ToolTip = CHECK_POWER_FAIL_ACTION_MESSAGE;
        }

        protected void CheckPowerFailWithWindowField(FineUI.Grid grid, string columnID)
        {
            FineUI.WindowField btn = grid.FindColumn(columnID) as FineUI.WindowField;
            btn.Enabled = false;
            btn.ToolTip = CHECK_POWER_FAIL_ACTION_MESSAGE;
        }

        protected void CheckPowerFailWithAlert()
        {
            PageContext.RegisterStartupScript(Alert.GetShowInTopReference(CHECK_POWER_FAIL_ACTION_MESSAGE));
        }

        protected void CheckPowerWithButton(string powerName, FineUI.Button btn)
        {
            if (!CheckPower(powerName))
            {
                CheckPowerFailWithButton(btn);
            }
        }

        protected void CheckPowerWithLinkButtonField(string powerName, FineUI.Grid grid, string columnID)
        {
            if (!CheckPower(powerName))
            {
                CheckPowerFailWithLinkButtonField(grid, columnID);
            }
        }

        protected void CheckPowerWithWindowField(string powerName, FineUI.Grid grid, string columnID)
        {
            if (!CheckPower(powerName))
            {
                CheckPowerFailWithWindowField(grid, columnID);
            }
        }

        /// <summary>
        /// 为删除Grid中选中项的按钮添加提示信息
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="grid"></param>
        protected void ResolveDeleteButtonForGrid(FineUI.Button btn, Grid grid)
        {
            ResolveDeleteButtonForGrid(btn, grid, "确定要删除选中的{0}项记录吗？");
        }

        protected void ResolveDeleteButtonForGrid(FineUI.Button btn, Grid grid, string confirmTemplate)
        {
            ResolveDeleteButtonForGrid(btn, grid, "请至少应该选择一项记录！", confirmTemplate);
        }

        protected void ResolveDeleteButtonForGrid(FineUI.Button btn, Grid grid, string noSelectionMessage, string confirmTemplate)
        {
            // 点击删除按钮时，至少选中一项
            btn.OnClientClick = grid.GetNoSelectionAlertInParentReference(noSelectionMessage);
            btn.ConfirmText = String.Format(confirmTemplate, "&nbsp;<span class=\"highlight\"><script>" + grid.GetSelectedCountReference() + "</script></span>&nbsp;");
            btn.ConfirmTarget = Target.Top;
        }

        #endregion

        #region 产品版本

        public string GetProductVersion()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            return String.Format("{0}.{1}", v.Major, v.Minor);
        }

        #endregion

        #region 隐藏字段相关

        /// <summary>
        /// 从隐藏字段中获取选择的全部ID列表
        /// </summary>
        /// <param name="hfSelectedIDS"></param>
        /// <returns></returns>
        public List<int> GetSelectedIDsFromHiddenField(FineUI.HiddenField hfSelectedIDS)
        {
            JArray idsArray = new JArray();

            string currentIDS = hfSelectedIDS.Text.Trim();
            if (!String.IsNullOrEmpty(currentIDS))
            {
                idsArray = JArray.Parse(currentIDS);
            }
            else
            {
                idsArray = new JArray();
            }
            return new List<int>(idsArray.ToObject<int[]>());
        }

        /// <summary>
        /// 跨页保持选中项 - 将表格当前页面选中行对应的数据同步到隐藏字段中
        /// </summary>
        /// <param name="hfSelectedIDS"></param>
        /// <param name="grid"></param>
        public void SyncSelectedRowIndexArrayToHiddenField(FineUI.HiddenField hfSelectedIDS, Grid grid)
        {
            List<int> ids = GetSelectedIDsFromHiddenField(hfSelectedIDS);

            List<int> selectedRows = new List<int>();
            if (grid.SelectedRowIndexArray != null && grid.SelectedRowIndexArray.Length > 0)
            {
                selectedRows = new List<int>(grid.SelectedRowIndexArray);
            }

            if (grid.IsDatabasePaging)
            {
                for (int i = 0, count = Math.Min(grid.PageSize, (grid.RecordCount - grid.PageIndex * grid.PageSize)); i < count; i++)
                {
                    int id = Convert.ToInt32(grid.DataKeys[i][0]);
                    if (selectedRows.Contains(i))
                    {
                        if (!ids.Contains(id))
                        {
                            ids.Add(id);
                        }
                    }
                    else
                    {
                        if (ids.Contains(id))
                        {
                            ids.Remove(id);
                        }
                    }
                }
            }
            else
            {
                int startPageIndex = grid.PageIndex * grid.PageSize;
                for (int i = startPageIndex, count = Math.Min(startPageIndex + grid.PageSize, grid.RecordCount); i < count; i++)
                {
                    int id = Convert.ToInt32(grid.DataKeys[i][0]);
                    if (selectedRows.Contains(i - startPageIndex))
                    {
                        if (!ids.Contains(id))
                        {
                            ids.Add(id);
                        }
                    }
                    else
                    {
                        if (ids.Contains(id))
                        {
                            ids.Remove(id);
                        }
                    }
                }
            }

            hfSelectedIDS.Text = new JArray(ids).ToString(Formatting.None);
        }

        /// <summary>
        /// 跨页保持选中项 - 根据隐藏字段的数据更新表格当前页面的选中行
        /// </summary>
        /// <param name="hfSelectedIDS"></param>
        /// <param name="grid"></param>
        public void UpdateSelectedRowIndexArray(FineUI.HiddenField hfSelectedIDS, Grid grid)
        {
            List<int> ids = GetSelectedIDsFromHiddenField(hfSelectedIDS);

            List<int> nextSelectedRowIndexArray = new List<int>();
            if (grid.IsDatabasePaging)
            {
                for (int i = 0, count = Math.Min(grid.PageSize, (grid.RecordCount - grid.PageIndex * grid.PageSize)); i < count; i++)
                {
                    int id = Convert.ToInt32(grid.DataKeys[i][0]);
                    if (ids.Contains(id))
                    {
                        nextSelectedRowIndexArray.Add(i);
                    }
                }
            }
            else
            {
                int nextStartPageIndex = grid.PageIndex * grid.PageSize;
                for (int i = nextStartPageIndex, count = Math.Min(nextStartPageIndex + grid.PageSize, grid.RecordCount); i < count; i++)
                {
                    int id = Convert.ToInt32(grid.DataKeys[i][0]);
                    if (ids.Contains(id))
                    {
                        nextSelectedRowIndexArray.Add(i - nextStartPageIndex);
                    }
                }
            }
            grid.SelectedRowIndexArray = nextSelectedRowIndexArray.ToArray();
        }

        #endregion

        #region 模拟树的下拉列表

        protected List<T> ResolveDDL<T>(List<T> mys) where T : ICustomTree, ICloneable, IKeyID, new()
        {
            return ResolveDDL<T>(mys, -1, true);
        }

        protected List<T> ResolveDDL<T>(List<T> mys, int currentId) where T : ICustomTree, ICloneable, IKeyID, new()
        {
            return ResolveDDL<T>(mys, currentId, true);
        }


        // 将一个树型结构放在一个下列列表中可供选择
        protected List<T> ResolveDDL<T>(List<T> source, int currentID, bool addRootNode) where T : ICustomTree, ICloneable, IKeyID, new()
        {
            List<T> result = new List<T>();

            if (addRootNode)
            {
                // 添加根节点
                T root = new T();
                root.Name = "--根节点--";
                root.ID = -1;
                root.TreeLevel = 0;
                root.Enabled = true;
                result.Add(root);
            }

            foreach (T item in source)
            {
                T newT = (T)item.Clone();
                result.Add(newT);

                // 所有节点的TreeLevel加一
                if (addRootNode)
                {
                    newT.TreeLevel++;
                }
            }

            // currentId==-1表示当前节点不存在
            if (currentID != -1)
            {
                // 本节点不可点击（也就是说当前节点不可能是当前节点的父节点）
                // 并且本节点的所有子节点也不可点击，你想如果当前节点跑到子节点的子节点，那么这些子节点就从树上消失了
                bool startChileNode = false;
                int startTreeLevel = 0;
                foreach (T my in result)
                {
                    if (my.ID == currentID)
                    {
                        startTreeLevel = my.TreeLevel;
                        my.Enabled = false;
                        startChileNode = true;
                    }
                    else
                    {
                        if (startChileNode)
                        {
                            if (my.TreeLevel > startTreeLevel)
                            {
                                my.Enabled = false;
                            }
                            else
                            {
                                startChileNode = false;
                            }
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region 日志记录

        protected void LogInfo(string message)
        {
            DB.Logs.Add(new Log
            {
                Level = "Info",
                Message = message,
                LogTime = DateTime.Now
            });
            DB.SaveChanges();

        }

        #endregion

    }
}
