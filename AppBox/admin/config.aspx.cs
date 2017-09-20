using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json.Linq;
using FineUI;


namespace AppBox.admin
{
    public partial class config : PageBase
    {
        #region ViewPower

        /// <summary>
        /// 本页面的浏览权限，空字符串表示本页面不受权限控制
        /// </summary>
        public override string ViewPower
        {
            get
            {
                return "CoreConfigView";
            }
        }

        #endregion

        #region Page_Load

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            // 权限检查
            CheckPowerWithButton("CoreConfigEdit", btnSave);


            tbxTitle.Text = ConfigHelper.Title;
            nbxPageSize.Text = ConfigHelper.PageSize.ToString();
            tbxHelpList.Text = StringUtil.GetJSBeautifyString(ConfigHelper.HelpList);
            ddlMenuType.SelectedValue = ConfigHelper.MenuType;
            ddlTheme.SelectedValue = ConfigHelper.Theme;

        }

        #endregion

        #region Events

        protected void btnSave_OnClick(object sender, EventArgs e)
        {
            // 在操作之前进行权限检查
            if (!CheckPower("CoreConfigEdit"))
            {
                CheckPowerFailWithAlert();
                return;
            }

            string helpListStr = tbxHelpList.Text.Trim();
            try
            {
                JArray.Parse(helpListStr);
            }
            catch (Exception)
            {
                tbxHelpList.MarkInvalid("格式不正确，必须是JSON字符串！");

                return;
            }



            ConfigHelper.Title = tbxTitle.Text.Trim();
            ConfigHelper.PageSize = Convert.ToInt32(nbxPageSize.Text.Trim());
            ConfigHelper.HelpList = helpListStr;
            ConfigHelper.MenuType = ddlMenuType.SelectedValue;
            ConfigHelper.Theme = ddlTheme.SelectedValue;
            ConfigHelper.SaveAll();

            Alert.ShowInTop("修改系统配置成功（点击确定刷新页面）！", String.Empty, "top.window.location.reload(false);");

        }

        #endregion
    }
}
